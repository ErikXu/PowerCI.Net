using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json;
using RestSharp;
using System.ComponentModel.DataAnnotations;

namespace PowerCI.Commands.Jenkins
{
    [Command("folder", Description = "Handle jenkins folders"), 
     Subcommand(typeof(JenkinsFolderListCommand)),
     Subcommand(typeof(JenkinsFolderCreateCommand))]
    internal class JenkinsFolderCommand
    {
        public void OnExecute(IConsole console)
        {
            console.WriteLine("Handle jenkins folders");
        }
    }

    [Command("list", Description = "List jenkins folders")]
    internal class JenkinsFolderListCommand : JenkinsParameter
    {
        public void OnExecute(IConsole console, IJenkinsService jenkinsService)
        {
            var (isValid, host, user, tokenOrPassword) = ValidParameters(console);
            if (!isValid)
            {
                return;
            }

            var (success, list) = jenkinsService.ListFolder(host, user, tokenOrPassword);
            if (!success)
            {
                return;
            }

            console.WriteLine(JsonConvert.SerializeObject(list, Formatting.Indented));
        }
    }

    [Command("create", Description = "Create jenkins folders")]
    internal class JenkinsFolderCreateCommand : JenkinsParameter
    {
        [Option(Description = "Folder Name", ShortName = "n")]
        [Required]
        public string? Name { get; set; }

        public void OnExecute(IConsole console, IJenkinsService jenkinsService)
        {
            var (isValid, host, user, tokenOrPassword) = ValidParameters(console);
            if (!isValid)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(Name))
            {
                console.WriteLine("Name is required");
                return;
            }

            (var success, var isExisted) = jenkinsService.CheckExists(host, user, tokenOrPassword, string.Empty, Name);
            if (!success)
            {
                return;
            }

            if (isExisted)
            {
                console.WriteLine($"Name [{Name}] exists");
                return;
            }

            using var client = new RestClient();
            (success, var crumb) = jenkinsService.GetCrumb(client, host, user, tokenOrPassword);
            if (!success || crumb == null)
            {
                return;
            }

            success = jenkinsService.CreateFolder(client, host, user, tokenOrPassword, Name, crumb);
            if (!success)
            {
                return;
            }

            console.WriteLine($"Folder [{Name}] created");
        }
    }
}
