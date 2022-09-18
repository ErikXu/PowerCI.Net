﻿using McMaster.Extensions.CommandLineUtils;

namespace PowerCI.Commands.Jenkins
{
    [Command("jenkins", Description = "Jenkins tools"), Subcommand(typeof(JenkinsInstallCommand)), Subcommand(typeof(JenkinsPasswordCommand))]
    internal class JenkinsCommand
    {
        public void OnExecute(IConsole console)
        {
            console.WriteLine("Jenkins tools");
        }
    }

    [Command("password", Description = "Get initial admin password of jenkins")]
    internal class JenkinsPasswordCommand
    {
        public void OnExecute(IConsole console)
        {
            var password = File.ReadAllText("/var/lib/jenkins/secrets/initialAdminPassword");
            console.WriteLine(password);
        }
    }
}
