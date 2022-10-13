using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json;
using PowerCI.Commands.Gitlab.Requests;
using PowerCI.Commands.Gitlab.Responses;
using PowerCI.Commands.Jenkins;
using System.ComponentModel.DataAnnotations;

namespace PowerCI.Commands.Gitlab
{
    [Command("init", Description = "Init gitlab")]
    internal class GitlabInitCommand
    {
        [Required]
        [Option(Description = "Gitlab Host", ShortName = "H")]
        public string Host { get; set; }

        [Required]
        [Option(Description = "Gitlab User", ShortName = "u")]
        public string User { get; set; }

        [Required]
        [Option(Description = "Gitlab User Password", ShortName = "p")]
        public string Password { get; set; }

        public void OnExecute(IConsole console)
        {
            if (Host == null || !Host.StartsWith("http"))
            {
                console.WriteLine("Host is not valid");
                return;
            }

            var host = Host.TrimEnd('/');

            using var client = new GitlabHttpClient();
            GrantOauthToken(client, Host, User, Password);

            var userList = GetUserByUsername(client, Host);

            var devopsUserId = 0;
            if (userList.Count >= 1)
            {
                devopsUserId = userList[0].Id;
            }
            else
            {
                var devopsUser = CreateUser(client, Host);
                devopsUserId = devopsUser.Id;
            }
            console.WriteLine(devopsUserId);
            var personalToken = CreatePersonalAccessToken(client, Host, devopsUserId);

            var json = File.ReadAllText(GitlabCommand.ConfigPath);
            var configs = JsonConvert.DeserializeObject<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
            configs["host"] = host;
            configs["token"] = personalToken.Token;
            json = JsonConvert.SerializeObject(configs);
            File.WriteAllText(GitlabCommand.ConfigPath, json);

            console.WriteLine("Init gitlab");
        }

        private static void GrantOauthToken(GitlabHttpClient client, string host, string user, string password)
        {
            var request = new OauthRequest
            {
                GrantType = "password",
                Username = user,
                Password = password
            };

            client.GrantOauthToken(host, request);
        }

        private static ListUserResponse GetUserByUsername(GitlabHttpClient client, string host)
        {
            return client.GetUserByUsername(host, "devops_user");
        }

        private static CreateUserResponse CreateUser(GitlabHttpClient client, string host)
        {
            var request = new CreateUserRequest
            {
                Admin = true,
                Username = "devops_user",
                Name = "Devops_User",
                Email = "devops@example.com",
                Password = Guid.NewGuid().ToString()
            };

            var response = client.CreateUser(host, request);
            return response;
        }

        private static CreatePersonalAccessTokenResponse CreatePersonalAccessToken(GitlabHttpClient client, string host, int userId)
        {
            var request = new CreatePersonalAccessTokenRequest
            {
                Name = "devops_token",
                Scopes = new List<string> { "api" },
                ExpiresAt = "2099-12-31"
            };

            var response = client.CreatePersonalAccessToken(host, userId, request);
            return response;
        }
    }
}
