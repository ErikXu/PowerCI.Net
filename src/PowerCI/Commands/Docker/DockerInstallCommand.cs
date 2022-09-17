using McMaster.Extensions.CommandLineUtils;

namespace PowerCI.Commands.Docker
{
    [Command("install", Description = "Install docker")]
    internal class DockerInstallCommand
    {
        private readonly string _path = "/tmp/install-docker.sh";
        private readonly string _script = @"#!/bin/bash
yum remove docker \
                  docker-client \
                  docker-client-latest \
                  docker-common \
                  docker-latest \
                  docker-latest-logrotate \
                  docker-logrotate \
                  docker-engine

yum install yum-utils -y

yum-config-manager \
    --add-repo \
    https://download.docker.com/linux/centos/docker-ce.repo

yum install docker-ce docker-ce-cli containerd.io docker-compose-plugin -y

systemctl start docker

systemctl enable docker
";
        public void OnExecute(IConsole console, ICommandService commandService)
        {
            File.WriteAllText(_path, _script);
            var code =  commandService.ExecuteCommand($"sed -i 's/\r$//' {_path}");
            if(code != 0)
            {
                console.WriteLine("Install failed");
                return;
            }

            code = commandService.ExecuteCommand($"bash {_path}");

            if (code != 0)
            {
                console.WriteLine("Install failed");
                return;
            }
           
            console.WriteLine("Install success");
        }
    }
}
