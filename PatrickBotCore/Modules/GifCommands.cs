
using System;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TenorSharp;

namespace PatrickBotCore.Modules
{
    public class GifCommands : BaseCommands
    {
        private readonly IConfigurationRoot _config;

        public GifCommands(IServiceProvider services)
        {
            _config = services.GetRequiredService<IConfigurationRoot>();
        }

        [Command("gifp")]
        public async Task GifCommand(params String[] stringArray) {
            await Log(msg: new Discord.LogMessage(message: "Starting GifCommand function", severity: Discord.LogSeverity.Info, source: "GifCommand"));

            await Context.Channel.TriggerTypingAsync();

            var query = string.Join(" ", stringArray);
            await Log(msg: new Discord.LogMessage(message: $"User typed: [{query}]", severity: Discord.LogSeverity.Info, source: "GifCommand"));

            query = $"spongebob {query}".Trim();

            await TenorSearch(query);
        }

        public async Task TenorSearch(string query) {
            var tenor = new TenorClient(_config["TenorApiKey"]);
            tenor.SetContentFilter(TenorSharp.Enums.ContentFilter.high);
            
            await Log(msg: new Discord.LogMessage(message: $"Getting gif results for: [{query}]", severity: Discord.LogSeverity.Info, source: "GifCommand"));
            var gifs = tenor.Search(query).GifResults;

            var rand = new Random();
            var gif = gifs[rand.Next(0, Math.Min(3, gifs.Length))].Url.OriginalString;
            await Log(msg: new Discord.LogMessage(message: $"Got the gif: [{gif}]", severity: Discord.LogSeverity.Info, source: "GifCommand"));

            await Context.Channel.SendMessageAsync(gif);
            await Log(msg: new Discord.LogMessage(message: "Gif sent", severity: Discord.LogSeverity.Info, source: "GifCommand"));
        }
    }
}