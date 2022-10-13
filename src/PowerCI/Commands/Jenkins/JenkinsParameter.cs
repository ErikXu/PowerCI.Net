using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json;

namespace PowerCI.Commands.Jenkins
{
    internal class JenkinsParameter
    {
        [Option(Description = "Jenkins Host", ShortName = "H")]
        public string Host { get; set; }

        [Option(Description = "Jenkins User", ShortName = "u")]
        public string User { get; set; }

        [Option(Description = "Jenkins User Token/Password", ShortName = "p")]
        public string Password { get; set; }

        protected (bool isvalid, string host, string user, string tokenOrPassword) ValidParameters(IConsole console)
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
                return (false, string.Empty, string.Empty, string.Empty);
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
                return (false, host, string.Empty, string.Empty);
            }

            var tokenOrPassword = string.Empty;

            if (Password != null)
            {
                tokenOrPassword = Password;
            }
            else if (configs.ContainsKey("password"))
            {
                tokenOrPassword = configs["password"];
            }

            if (tokenOrPassword == string.Empty)
            {
                console.WriteLine("Token/Password is required");
                return (false, host, user, string.Empty);
            }

            return (true, host, user, tokenOrPassword);
        }
    }
}
