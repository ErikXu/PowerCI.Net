using McMaster.Extensions.CommandLineUtils;
using System.ComponentModel.DataAnnotations;

namespace PowerCI.Commands.Gitlab
{
    // https://about.gitlab.com/install/#centos-7
    [Command("install", Description = "Install gitlab")]
    internal class GitlabInstallCommand
    {
        [Required]
        [Option(Description = "Gitlab Url", ShortName = "u")]
        public string Url { get; set; }

        private readonly string _script = @"#!/bin/bash
yum install curl policycoreutils-python openssh-server perl -y

yum install -y postfix
systemctl enable postfix
systemctl start postfix

curl https://packages.gitlab.com/install/repositories/gitlab/gitlab-ce/script.rpm.sh | bash

EXTERNAL_URL=""{EXTERNAL_URL}"" yum install gitlab-ce -y

gitlab-ctl reconfigure";

        public void OnExecute(IConsole console, ICommandService commandService)
        {
            var path = Path.Combine(Program.Workspace, "install-gitlab.sh");
            var script = _script.Replace("{EXTERNAL_URL}", Url);
            File.WriteAllText(path, script);

            var code = commandService.ExecuteCommand($"sed -i 's/\r$//' {path}");
            if (code != 0)
            {
                console.WriteLine("Install failed");
                return;
            }

            code = commandService.ExecuteCommand($"bash {path}");

            if (code != 0)
            {
                console.WriteLine("Install failed");
                return;
            }

            console.WriteLine("Install success");
        }
    }
}
