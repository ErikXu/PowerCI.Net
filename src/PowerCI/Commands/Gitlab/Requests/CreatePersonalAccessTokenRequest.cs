using Newtonsoft.Json;

namespace PowerCI.Commands.Gitlab.Requests
{
    public class CreatePersonalAccessTokenRequest
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("expires_at")]
        public string ExpiresAt { get; set; }

        [JsonProperty("scopes")]
        public List<string> Scopes { get; set; }
    }

}
