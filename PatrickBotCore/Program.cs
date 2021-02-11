using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
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
            if (message.MentionedUsers.Any(x => x.Username == _client.CurrentUser.Username))
            {
                await SendMemeAsync(message);
            }
        }

        [Command(RunMode = RunMode.Async)]
        public async Task SendMemeAsync(SocketMessage message)
        {
            await Log(msg: new LogMessage(message: "Starting the SendMessageAsync function", severity: LogSeverity.Info, source: "SendMessageAsync"));

            var random = new Random();

            var subreddits = new List<string>(){ "programmerHumor" };
            var subredditName = subreddits[random.Next(subreddits.Count - 1)];

            var reddit = new Reddit();
            var subreddit = reddit.GetSubreddit($"/r/{subredditName}");
            await Log(msg: new LogMessage(message: "Got the subreddit", severity: LogSeverity.Info, source: "SendMessageAsync"));
            await message.Channel.TriggerTypingAsync();
            

            var post = subreddit.Posts
                .Where(x => x.IsStickied == false)
                .Where(x => x.NSFW == false)
                .Skip(random.Next(1, 40))
                .Take(1)
                .First();
            await Log(msg: new LogMessage(message: "Got the post", severity: LogSeverity.Info, source: "SendMessageAsync"));

            var embedBuilder = new EmbedBuilder();

            var badWords = new List<string>() {
                    "fuk",
                    "fuck",
                    "fucked",
                    "fucking",
                    "bitch",
                    "bitching",
                    "cunt",
                    "ass",
                    "asshole",
                };

            var topComment = post.Comments.Any()
                ? post.Comments
                    .Where(c => !c.Body.ToLower().Split(" ").ToList()
                        .Any(p => badWords.Any(bw => p.Contains(bw))))
                    .OrderBy(c => c.Upvotes)
                    .Select(c => c.Body)
                    .FirstOrDefault() ?? "The top comment was too spicy for work, shame :("
                : "Can't find a top comment";
            await Log(msg: new LogMessage(message: "Got the top comment", severity: LogSeverity.Info, source: "SendMessageAsync"));

            embedBuilder.WithTitle(post.Title);
            embedBuilder.WithImageUrl(post.Url.ToString());
            embedBuilder.WithFooter($"r/{post.SubredditName}");
            embedBuilder.AddField("Upvotes", post.Upvotes, true);    // true - for inline
            embedBuilder.AddField("Top comment", topComment, false);
            embedBuilder.WithColor(Color.Red);
            await Log(msg: new LogMessage(message: "Built the embed", severity: LogSeverity.Info, source: "SendMessageAsync"));

            // await message.Channel.TriggerTypingAsync();
            await message.Channel.SendMessageAsync("", false, embedBuilder.Build());
            await Log(msg: new LogMessage(message: "Message sent", severity: LogSeverity.Info, source: "SendMessageAsync"));
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}