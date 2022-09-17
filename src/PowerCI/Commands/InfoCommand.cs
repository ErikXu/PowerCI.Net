using McMaster.Extensions.CommandLineUtils;

namespace PowerCI.Commands
{
    [Command("info", Description = "Get info")]
    internal class InfoCommand
    {
        private readonly IConsole _console;

        public InfoCommand(IConsole console)
        {
            _console = console;

        }
        public void OnExecute()
        {
            _console.WriteLine("Power CI tools");
        }
    }
}
