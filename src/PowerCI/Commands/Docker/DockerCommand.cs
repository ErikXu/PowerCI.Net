using McMaster.Extensions.CommandLineUtils;

namespace PowerCI.Commands.Docker
{
    [Command("docker", Description = "Docker tools"),
     Subcommand(typeof(DockerInstallCommand))]
    internal class DockerCommand
    {
        public void OnExecute(IConsole console)
        {
            console.WriteLine("Docker tools");
        }
    }
}
