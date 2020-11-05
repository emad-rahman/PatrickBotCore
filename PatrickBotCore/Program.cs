using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using RedditSharp;

namespace PatrickBotCore
{
    public class Program
    {
        public static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();


        private DiscordSocketClient _client;
        public async Task MainAsync()
        {
            _client = new DiscordSocketClient();

            _client.Log += Log;

            // Remember to keep token private or to read it from an 
            // external source! In this case, we are reading the token 
            // from an environment variable. If you do not know how to set-up
            // environment variables, you may find more information on the 
            // Internet or by using other methods such as reading from 
            // a configuration.

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            var configuration = builder.Build();


            var discordToken = configuration["DiscordToken"];

            await _client.LoginAsync(TokenType.Bot, discordToken);
            await _client.StartAsync();

            _client.MessageReceived += MessageReceived;

 
            // Block this task until the program is closed.
            await Task.Delay(-1);
        }

        private async Task MessageReceived(SocketMessage message)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            var configuration = builder.Build();


            var RedditAppId = configuration["RedditAppId"];
            var RedditAppSecret = configuration["RedditAppSecret"];


            if (message.Content == "!ping")
            {
                await message.Channel.SendMessageAsync("Pong!");
            }
            else if (message.Content == "!meme")
            {
                var subredditName = "programmerHumor";

                var reddit = new Reddit();
                var subreddit = reddit.GetSubreddit($"/r/{subredditName}");

                var random = new Random();

                var post = subreddit.Posts
                    .Where(x => x.IsStickied == false)
                    .Where(x => x.NSFW == false)
                    .Skip(random.Next(1,40))
                    .Take(1)
                    .First();

                var embedBuilder = new EmbedBuilder();

                var badWords = new List<string>() {
                    "fuk",
                    "fuck",
                    "bitch",
                    "cunt",
                    "ass"
                };


                var topComment = post.Comments.Any() 
                    ? post.Comments
                        .Where(c => !c.Body.ToLower().Split(" ").ToList()
                            .Any(p => badWords.Contains(p)))
                        .OrderBy(c => c.Upvotes)
                        .Select(c => c.Body)
                        .FirstOrDefault()
                    : "";

                embedBuilder.WithTitle(post.Title);
                embedBuilder.WithFooter($"r/{post.SubredditName}");
                embedBuilder.AddField("Upvotes", post.Upvotes, true);    // true - for inline
                embedBuilder.AddField("Top comment", topComment, false);
                embedBuilder.WithImageUrl(post.Url.ToString());
                embedBuilder.WithColor(Color.Red);

                await message.Channel.SendMessageAsync("", false, embedBuilder.Build());
            }
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}