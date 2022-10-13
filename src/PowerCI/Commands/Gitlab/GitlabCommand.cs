using McMaster.Extensions.CommandLineUtils;

namespace PowerCI.Commands.Gitlab
{

    [Command("gitlab", Description = "Gitlab tools"), 
     Subcommand(typeof(GitlabInstallCommand)), 
     Subcommand(typeof(GitlabPasswordCommand)),
     Subcommand(typeof(GitlabInitCommand))]
    internal class GitlabCommand
    {
        public static string ConfigPath { get; } = Path.Combine(Program.Workspace, "gitlab.json");

        public void OnExecute(IConsole console)
        {
            console.WriteLine("Gitlab tools");
        }
    }

    [Command("password", Description = "Get initial root password of gitlab")]
    internal class GitlabPasswordCommand
    {
        public void OnExecute(IConsole console)
        {
            var password = File.ReadAllText("/etc/gitlab/initial_root_password");
            console.WriteLine(password);
        }
    }
}
