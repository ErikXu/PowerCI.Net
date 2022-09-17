using McMaster.Extensions.CommandLineUtils;

namespace PowerCI.Commands.Info
{
    [Command("info", Description = "Get info")]
    internal class InfoCommand
    {
        public void OnExecute(IConsole console)
        {
            console.WriteLine("Power CI tools");
        }
    }
}
