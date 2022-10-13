using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace PowerCI.Commands.Jenkins
{
    [Command("set", Description = "Set configs of jenkins"), 
     Subcommand(typeof(JenkinsSetHostCommand)),
     Subcommand(typeof(JenkinsSetUserCommand)),
     Subcommand(typeof(JenkinsSetPasswordCommand))]
    internal class JenkinsSetCommand
    {
        public void OnExecute(IConsole console)
        {
            console.WriteLine("Set configs of jenkins");
        }
    }

    [Command("host", Description = "Set host of jenkins")]
    internal class JenkinsSetHostCommand
    {
        [Argument(0)]
        [Required]
        public string Host { get; }

        public void OnExecute(IConsole console)
        {
            if (Host == null || !Host.StartsWith("http"))
            {
                console.WriteLine("Host is not valid");
                return;
            }

            var host = Host.TrimEnd('/');

            var json = File.ReadAllText(JenkinsCommand.ConfigPath);
            var configs = JsonConvert.DeserializeObject<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
            configs["host"] = host;
            json = JsonConvert.SerializeObject(configs);
            File.WriteAllText(JenkinsCommand.ConfigPath, json);

            console.WriteLine($"Host [{Host}] set");
        }
    }

    [Command("user", Description = "Set user of jenkins")]
    internal class JenkinsSetUserCommand
    {
        [Argument(0)]
        [Required]
        public string User { get; }

        public void OnExecute(IConsole console)
        {
            var json = File.ReadAllText(JenkinsCommand.ConfigPath);
            var configs = JsonConvert.DeserializeObject<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
            configs["user"] = User ?? string.Empty;
            json = JsonConvert.SerializeObject(configs);
            File.WriteAllText(JenkinsCommand.ConfigPath, json);

            console.WriteLine($"User [{User}] set");
        }
    }

    [Command("password", Description = "Set password of jenkins")]
    internal class JenkinsSetPasswordCommand
    {
        [Argument(0)]
        [Required]
        public string Password { get; }

        public void OnExecute(IConsole console)
        {
            var json = File.ReadAllText(JenkinsCommand.ConfigPath);
            var configs = JsonConvert.DeserializeObject<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
            configs["password"] = Password ?? string.Empty;
            json = JsonConvert.SerializeObject(configs);
            File.WriteAllText(JenkinsCommand.ConfigPath, json);

            console.WriteLine("Password set");
        }
    }
}
