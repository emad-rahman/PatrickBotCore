using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RedditSharp;

namespace PatrickBotCore
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get; }
        private DiscordSocketClient _client;

        public Startup(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            Configuration = builder.Build();
        }

        public static async Task RunAsync(string[] args)
        {
            var startup = new Startup(args);
            await startup.RunAsync();
        }

        public async Task RunAsync()
        {
            // var services = new ServiceCollection();
            // ConfigureServices(services);

            _client = new DiscordSocketClient();
            _client.Log += Log;

            var discordToken = Configuration["DiscordToken"];

            await _client.LoginAsync(TokenType.Bot, discordToken);
            await _client.StartAsync();

            _client.MessageReceived += MessageReceived;

            // Block this task until the program is closed.
            await Task.Delay(-1);

        }

        private void ConfigureServices(IServiceCollection services)
        {

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

            var subreddits = new List<string>(){ "programmerHumor", "programmerreactions" };
            var subredditName = subreddits[random.Next(subreddits.Count)];

            var reddit = new Reddit();
            var subreddit = reddit.GetSubreddit($"/r/{subredditName}");
            await Log(msg: new LogMessage(message: "Got the subreddit", severity: LogSeverity.Info, source: "SendMessageAsync"));
            await message.Channel.TriggerTypingAsync();
            

            var post = subreddit.Posts
                .Where(x => x.IsStickied == false)
                .Where(x => x.NSFW == false)
                .Where(x => x.Upvotes > 100)
                .Skip(random.Next(1, 40))
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
            var msg = await message.Channel.SendMessageAsync("", false, embedBuilder.Build());

            var emojis = new List<Emoji>() { 
                new Emoji("⬆️"),
                new Emoji("⬇️")
            }.ToArray<IEmote>();

            await msg.AddReactionsAsync(emojis);

            await Log(msg: new LogMessage(message: "Message sent", severity: LogSeverity.Info, source: "SendMessageAsync"));
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}