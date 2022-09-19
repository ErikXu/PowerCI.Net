using McMaster.Extensions.CommandLineUtils;
using PowerCI.Commands.Jenkins;

namespace PowerCI.Commands.Gitlab
{

    [Command("gitlab", Description = "Gitlab tools"), Subcommand(typeof(GitlabInstallCommand)), Subcommand(typeof(GitlabPasswordCommand))]
    internal class GitlabCommand
    {
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
