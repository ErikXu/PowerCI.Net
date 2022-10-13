using McMaster.Extensions.CommandLineUtils;
using System.ComponentModel.DataAnnotations;

namespace PowerCI.Commands.Kubernetes
{
    [Command("install", Description = "Install kubernetes")]
    internal class KubernetesInstallCommand
    {
        [Option(Description = "Master IP or IP Range", ShortName = "m")]
        [Required]
        public string Masters { get; set; }

        [Option(Description = "Node IP or IP Range", ShortName = "n")]
        [Required]
        public string Nodes { get; set; }

        [Option(Description = "SSH Password", ShortName = "p")]
        public string Password { get; set; }

        private readonly string _script = @"#!/bin/bash
wget https://github.com/labring/sealos/releases/download/v4.1.3/sealos_4.1.3_linux_amd64.tar.gz
tar -zxvf sealos_4.1.3_linux_amd64.tar.gz sealos
chmod +x sealos
mv sealos /usr/bin

sealos run labring/kubernetes:v1.24.0 labring/calico:v3.22.1 \
    --masters {masters} \
    --nodes {nodes} -p {password}
";

        public void OnExecute(IConsole console, ICommandService commandService)
        {
            var path = Path.Combine(Program.Workspace, "install-kubernetes.sh");

            var script = _script.Replace("{masters}", Masters).Replace("{nodes}", Nodes).Replace("{password}", Password);
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
