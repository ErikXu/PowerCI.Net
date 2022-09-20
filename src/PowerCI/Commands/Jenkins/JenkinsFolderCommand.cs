using McMaster.Extensions.CommandLineUtils;

namespace PowerCI.Commands.Jenkins
{
    [Command("folder", Description = "Handle jenkins folders")]
    internal class JenkinsFolderCommand
    {
        public void OnExecute(IConsole console)
        {
            console.WriteLine("Handle jenkins folders");
        }
    }
}
