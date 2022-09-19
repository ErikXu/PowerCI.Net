using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;

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
    internal class JenkinsJobListCommand
    {
        [Option(Description = "Jenkins Host", ShortName = "H")]
        public string? Host { get; set; }

        [Option(Description = "Jenkins User", ShortName = "u")]
        public string? User { get; set; }

        [Option(Description = "Jenkins User Password", ShortName = "p")]
        public string? Password { get; set; }

        public void OnExecute(IConsole console)
        {
            var json = File.ReadAllText(JenkinsCommand.ConfigPath);
            var configs = JsonConvert.DeserializeObject<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();

            var host = string.Empty;

            if (Host != null)
            {
                host = Host.TrimEnd('/');
            }
            else if (configs.ContainsKey("host"))
            {
                host = configs["host"];
            }

            if (host == string.Empty)
            {
                console.WriteLine("Host is required");
            }

            var user = string.Empty;

            if (User != null)
            {
                user = User;
            }
            else if (configs.ContainsKey("user"))
            {
                user = configs["user"];
            }

            if (user == string.Empty)
            {
                console.WriteLine("User is required");
            }

            var password = string.Empty;

            if (Password != null)
            {
                password = Password;
            }
            else if (configs.ContainsKey("password"))
            {
                password = configs["password"];
            }

            if (password == string.Empty)
            {
                console.WriteLine("Password is required");
            }

            using var client = new RestClient();
            var request = new RestRequest
            {
                Method = Method.Get,
                Resource = $"{host}/api/json?tree=jobs[name,url]&pretty=true"
            };

            client.Authenticator = new HttpBasicAuthenticator(user, password);
            var response = client.Execute(request);
            if (response.IsSuccessful)
            {
                var content = response.Content;
                console.WriteLine(content ?? string.Empty);
            }
            else
            {
                console.WriteLine("Call jenkins api failed");
            }
        }
    }
}
