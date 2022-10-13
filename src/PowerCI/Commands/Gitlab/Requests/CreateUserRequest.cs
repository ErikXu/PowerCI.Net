using Newtonsoft.Json;

namespace PowerCI.Commands.Gitlab.Requests
{
    internal class CreateUserRequest
    {
        [JsonProperty("admin")]
        public bool Admin { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }
    }
}
