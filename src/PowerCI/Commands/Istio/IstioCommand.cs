using McMaster.Extensions.CommandLineUtils;

namespace PowerCI.Commands.Istio
{
    [Command("istio", Description = "Istio tools"), Subcommand(typeof(IstioInstallCommand))]
    internal class IstioCommand
    {
        public static string Workspace { get; } = Path.Combine(Program.Workspace, "istio");

        public void OnExecute(IConsole console)
        {
            console.WriteLine("Istio tools");
        }
    }
}
