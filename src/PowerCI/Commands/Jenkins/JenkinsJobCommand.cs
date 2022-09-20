using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json;

namespace PowerCI.Commands.Jenkins
{
    [Command("job", Description = "Handle jenkins jobs"), Subcommand(typeof(JenkinsJobListCommand))]
    internal class JenkinsJobCommand
    {
        public void OnExecute(IConsole console)
        {
            console.WriteLine("Handle jenkins jobs");
        }
    }

    [Command("list", Description = "List jenkins jobs")]
    internal class JenkinsJobListCommand : JenkinsParameter
    {
        [Option(Description = "Folder Name", ShortName = "f")]
        public string? Folder { get; set; }

        public void OnExecute(IConsole console, IJenkinsService jenkinsService)
        {
            var (isValid, host, user, tokenOrPassword) = ValidParameters(console);
            if (!isValid)
            {
                return;
            }

            var (success, list) = jenkinsService.ListJob(host, user, tokenOrPassword, Folder ?? string.Empty);
            if (!success)
            {
                return;
            }

            console.WriteLine(JsonConvert.SerializeObject(list, Formatting.Indented));
        }
    }
}
