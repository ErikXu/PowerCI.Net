using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json;
using System.Text;

namespace PowerCI.Commands.Jenkins
{
    [Command("get", Description = "Get configs of jenkins")]
    internal class JenkinsGetCommand
    {
        public void OnExecute(IConsole console)
        {
            var json = File.ReadAllText(JenkinsCommand.ConfigPath);
            var configs = JsonConvert.DeserializeObject<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();

            foreach (var config in configs)
            {
                console.WriteLine($"{config.Key}: {config.Value}");
            }
        }
    }
}
