using Newtonsoft.Json;

namespace PowerCI.Commands.Gitlab.Requests
{
    internal class OauthRequest
    {
        [JsonProperty("grant_type")]
        public string GrantType { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }
    }
}
