using Newtonsoft.Json;

namespace PowerCI.Commands.Gitlab.Responses
{
    public class CreatePersonalAccessTokenResponse
    {
        public int id { get; set; }
        public string name { get; set; }
        public bool revoked { get; set; }
        public DateTime created_at { get; set; }
        public string[] scopes { get; set; }
        public int user_id { get; set; }
        public object last_used_at { get; set; }
        public bool active { get; set; }
        public string expires_at { get; set; }

        [JsonProperty("token")]
        public string Token { get; set; }
    }
}
