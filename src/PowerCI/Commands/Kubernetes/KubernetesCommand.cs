using McMaster.Extensions.CommandLineUtils;

namespace PowerCI.Commands.Kubernetes
{
    [Command("k8s", Description = "Kubernetes tools"), Subcommand(typeof(KubernetesInstallCommand))]
    internal class KubernetesCommand
    {
        public void OnExecute(IConsole console)
        {
            console.WriteLine("Kubernetes tools");
        }
    }
}
