using Newtonsoft.Json;

namespace PowerCI.Commands.Gitlab.Responses
{
    public class CreateUserResponse
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        public string username { get; set; }
        public string name { get; set; }
        public string state { get; set; }
        public string avatar_url { get; set; }
        public string web_url { get; set; }
        public DateTime created_at { get; set; }
        public string bio { get; set; }
        public object location { get; set; }
        public object public_email { get; set; }
        public string skype { get; set; }
        public string linkedin { get; set; }
        public string twitter { get; set; }
        public string website_url { get; set; }
        public object organization { get; set; }
        public string job_title { get; set; }
        public object pronouns { get; set; }
        public bool bot { get; set; }
        public object work_information { get; set; }
        public int followers { get; set; }
        public int following { get; set; }
        public bool is_followed { get; set; }
        public object local_time { get; set; }
        public object last_sign_in_at { get; set; }
        public object confirmed_at { get; set; }
        public object last_activity_on { get; set; }
        public string email { get; set; }
        public int theme_id { get; set; }
        public int color_scheme_id { get; set; }
        public int projects_limit { get; set; }
        public object current_sign_in_at { get; set; }
        public object[] identities { get; set; }
        public bool can_create_group { get; set; }
        public bool can_create_project { get; set; }
        public bool two_factor_enabled { get; set; }
        public bool external { get; set; }
        public bool private_profile { get; set; }
        public string commit_email { get; set; }
        public bool is_admin { get; set; }
        public object note { get; set; }
        public int namespace_id { get; set; }
    }
}
