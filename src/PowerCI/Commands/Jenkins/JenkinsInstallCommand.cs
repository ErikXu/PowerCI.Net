using McMaster.Extensions.CommandLineUtils;

namespace PowerCI.Commands.Jenkins
{
    // https://www.jenkins.io/doc/book/installing/linux/#red-hat-centos
    [Command("install", Description = "Install jenkins")]
    internal class JenkinsInstallCommand
    {
        private readonly string _script = @"#!/bin/bash
wget -O /etc/yum.repos.d/jenkins.repo \
    https://pkg.jenkins.io/redhat-stable/jenkins.repo --no-check-certificate

rpm --import https://pkg.jenkins.io/redhat-stable/jenkins.io.key

yum upgrade -y

yum install java-11-openjdk -y

yum install jenkins -y

# sed -i 's|\JENKINS_USER=""jenkins""|JENKINS_USER=""root""|g' /etc/sysconfig/jenkins

# chown -R root:root /var/lib/jenkins
# chown -R root:root /var/cache/jenkins
# chown -R root:root /var/log/jenkins

systemctl daemon-reload

systemctl start jenkins

systemctl enable jenkins
";

        public void OnExecute(IConsole console, ICommandService commandService)
        {
            var path = Path.Combine(Program.Workspace, "install-jenkins.sh");
            File.WriteAllText(path, _script);

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
