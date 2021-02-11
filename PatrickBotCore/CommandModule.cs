using System.Threading.Tasks;
using Discord.Commands;

// Keep in mind your module **must** be public and inherit ModuleBase.
// If it isn't, it will not be discovered by AddModulesAsync!
public class InfoModule : ModuleBase<SocketCommandContext>
{
    // ~say hello world -> hello world
	[Command("say")]
	[Summary("Echoes a message.")]
	public Task SayAsync([Remainder] [Summary("The text to echo")] string echo)
		=> ReplyAsync(echo);
}