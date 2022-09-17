using McMaster.Extensions.CommandLineUtils;

namespace PowerCI.Commands.Docker
{
    [Command("install", Description = "Install docker")]
    internal class DockerInstallCommand
    {
        public void OnExecute(IConsole console, ICommandService commandService)
        {
            var (code, message) = commandService.ExecuteCommand("ls");

            if (code != 0)
            {
                console.WriteLine($"Install failed, message: {message}");
            }
            else
            {
                console.WriteLine(message);
            }
        }
    }
}
