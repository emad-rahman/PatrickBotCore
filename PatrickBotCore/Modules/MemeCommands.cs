using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using RedditSharp;

namespace PatrickBotCore.Modules
{
    // for commands to be available, and have the Context passed to them, we must inherit ModuleBase
    public class MemeCommands : BaseCommands
    {
        [Command("meme")]
        [Alias("m")]
        public async Task MemeCommand()
        {
            await Log(msg: new LogMessage(message: "Starting MemeCommand function", severity: LogSeverity.Info, source: "MemeCommand"));

            var random = new Random();

            var subreddits = new List<string>(){ "programmerHumor", "programmerreactions" };
            var subredditName = subreddits[random.Next(subreddits.Count)];

            var reddit = new Reddit();
            var subreddit = reddit.GetSubreddit($"/r/{subredditName}");
            await Log(msg: new LogMessage(message: "Got the subreddit", severity: LogSeverity.Info, source: "MemeCommand"));
            await Context.Channel.TriggerTypingAsync();
            

            var post = subreddit.Posts
                .Where(x => x.IsStickied == false)
                .Where(x => x.NSFW == false)
                .Where(x => x.Upvotes > 250)
                .Skip(random.Next(1, 40))
                .First();
            await Log(msg: new LogMessage(message: "Got the post", severity: LogSeverity.Info, source: "MemeCommand"));

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
                    .OrderByDescending(c => c.Upvotes)
                    .Select(c => c.Body)
                    .FirstOrDefault() ?? "The top comment was too spicy for work, shame :("
                : "Can't find a top comment";
            await Log(msg: new LogMessage(message: "Got the top comment", severity: LogSeverity.Info, source: "MemeCommand"));

            embedBuilder.WithTitle(post.Title);
            embedBuilder.WithImageUrl(post.Url.ToString());
            embedBuilder.WithFooter($"r/{post.SubredditName}");
            embedBuilder.AddField("Upvotes", post.Upvotes, true);    // true - for inline
            embedBuilder.AddField("Top comment", topComment, false);
            embedBuilder.WithColor(Color.Red);
            await Log(msg: new LogMessage(message: "Built the embed", severity: LogSeverity.Info, source: "MemeCommand"));

            var msg = await Context.Channel.SendMessageAsync("", false, embedBuilder.Build());

            var emojis = new List<Emoji>() { 
                new Emoji("⬆️"),
                new Emoji("⬇️")
            }.ToArray<IEmote>();

            await msg.AddReactionsAsync(emojis);

            await Log(msg: new LogMessage(message: "Message sent", severity: LogSeverity.Info, source: "MemeCommand"));
        }
    }
}