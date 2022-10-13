using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;

namespace PowerCI.Commands.Jenkins
{
    internal interface IJenkinsService
    {
        (bool success, List<JobItem>) ListFolder(string host, string user, string tokenOrPassword);

        (bool success, List<JobItem>) ListJob(string host, string user, string tokenOrPassword, string folder);

        (bool success, bool) CheckExists(string host, string user, string tokenOrPassword, string folder, string name);

        (bool success, JenkinsCrumb) GetCrumb(RestClient client, string host, string user, string tokenOrPassword);

        (bool success, string) GetJobSpec(string host, string user, string tokenOrPassword, string folder, string name);

        bool CreateFolder(RestClient client, string host, string user, string tokenOrPassword, string name, JenkinsCrumb crumb);

        bool CreateJob(RestClient client, string host, string user, string tokenOrPassword, string folder, string name, JenkinsCrumb crumb, string content);
    }

    internal class JenkinsService : IJenkinsService
    {
        private readonly IConsole _console;

        public JenkinsService(IConsole console)
        {
            _console = console;
        }

        public (bool success, List<JobItem>) ListFolder(string host, string user, string tokenOrPassword)
        {
            using var client = new RestClient();
            var request = new RestRequest
            {
                Method = Method.Get,
                Resource = $"{host}/api/json?tree=jobs[name,url]&pretty=true"
            };

            client.Authenticator = new HttpBasicAuthenticator(user, tokenOrPassword);

            var response = client.Execute(request);
            if (!response.IsSuccessful)
            {
                _console.WriteLine("List folder failed");
                _console.WriteLine(response.Content ?? string.Empty);
                return (false, new List<JobItem>());
            }

            if (string.IsNullOrWhiteSpace(response.Content))
            {
                _console.WriteLine("List folder response is empty");
                return (false, new List<JobItem>());
            }

            var result = JsonConvert.DeserializeObject<ListJobResult>(response.Content);
            if (result == null)
            {
                _console.WriteLine("Deserialize object failed");
                return (false, new List<JobItem>());
            }

            var list = result.Jobs ?? new List<JobItem>();
            list = list.Where(n => n.Class == "com.cloudbees.hudson.plugins.folder.Folder").ToList();

            return (true, list);
        }

        public (bool success, List<JobItem>) ListJob(string host, string user, string tokenOrPassword, string folder)
        {
            using var client = new RestClient();
            var request = new RestRequest
            {
                Method = Method.Get,
                Resource = $"{host}/api/json?tree=jobs[name,url]&pretty=true"
            };

            if (!string.IsNullOrWhiteSpace(folder))
            {
                request.Resource = $"{host}/job/{folder}/api/json?tree=jobs[name,url]&pretty=true";
            }

            client.Authenticator = new HttpBasicAuthenticator(user, tokenOrPassword);

            var response = client.Execute(request);
            if (!response.IsSuccessful)
            {
                _console.WriteLine("List job failed");
                _console.WriteLine(response.Content ?? string.Empty);
                return (false, new List<JobItem>());
            }

            if (string.IsNullOrWhiteSpace(response.Content))
            {
                _console.WriteLine("List job response is empty");
                return (false, new List<JobItem>());
            }

            var result = JsonConvert.DeserializeObject<ListJobResult>(response.Content);
            if (result == null)
            {
                _console.WriteLine("Deserialize object failed");
                return (false, new List<JobItem>());
            }

            var list = result.Jobs ?? new List<JobItem>();
            list = list.Where(n => n.Class != "com.cloudbees.hudson.plugins.folder.Folder").ToList();

            return (true, list);
        }

        public (bool success, bool) CheckExists(string host, string user, string tokenOrPassword, string folder, string name)
        {
            using var client = new RestClient();
            var request = new RestRequest
            {
                Method = Method.Get,
                Resource = $"{host}/checkJobName?value={name}"
            };

            if (!string.IsNullOrWhiteSpace(folder))
            {
                request.Resource = $"{host}/job/{folder}/checkJobName?value={name}";
            }

            client.Authenticator = new HttpBasicAuthenticator(user, tokenOrPassword);

            var response = client.Execute(request);
            if (!response.IsSuccessful)
            {
                _console.WriteLine("Check exists failed");
                _console.WriteLine(response.Content ?? string.Empty);
                return (false, default(bool));
            }

            if (string.IsNullOrWhiteSpace(response.Content) || !response.Content.Contains("already exists"))
            {
                return (true, false);
            }

            return (true, true);
        }

        public (bool success, JenkinsCrumb) GetCrumb(RestClient client, string host, string user, string tokenOrPassword)
        {
            var request = new RestRequest
            {
                Method = Method.Get,
                Resource = $"{host}/crumbIssuer/api/json?pretty=true"
            };

            client.Authenticator = new HttpBasicAuthenticator(user, tokenOrPassword);

            var response = client.Execute(request);
            if (!response.IsSuccessful)
            {
                _console.WriteLine("Get jenkins crumb failed");
                _console.WriteLine(response.Content ?? string.Empty);
                return (false, new JenkinsCrumb());
            }

            var content = response.Content;
            var crumb = JsonConvert.DeserializeObject<JenkinsCrumb>(content ?? string.Empty);
            if (crumb == null || string.IsNullOrWhiteSpace(crumb.Crumb) || string.IsNullOrWhiteSpace(crumb.CrumbRequestField))
            {
                _console.WriteLine("Crumb response is invalid");
                return (false, new JenkinsCrumb());
            }

            return (true, crumb);
        }

        public (bool success, string) GetJobSpec(string host, string user, string tokenOrPassword, string folder, string name)
        {
            using var client = new RestClient();
            var request = new RestRequest
            {
                Method = Method.Get,
                Resource = $"{host}/job/{name}/config.xml"
            };

            if (!string.IsNullOrWhiteSpace(folder))
            {
                request.Resource = $"{host}/job/{folder}/job/{name}/config.xml";
            }

            client.Authenticator = new HttpBasicAuthenticator(user, tokenOrPassword);

            var response = client.Execute(request);
            if (!response.IsSuccessful)
            {
                _console.WriteLine("Get job spec failed");
                _console.WriteLine(response.Content ?? string.Empty);
                return (false, string.Empty);
            }

            if (string.IsNullOrWhiteSpace(response.Content))
            {
                _console.WriteLine("Job spec is empty");
                return (false, string.Empty);
            }

            return (true, response.Content);
        }

        public bool CreateFolder(RestClient client, string host, string user, string tokenOrPassword, string name, JenkinsCrumb crumb)
        {
            var content = @"<com.cloudbees.hudson.plugins.folder.Folder>
<description/>
<properties/>
<folderViews class=""com.cloudbees.hudson.plugins.folder.views.DefaultFolderViewHolder"">
<views>
<hudson.model.AllView>
<owner class=""com.cloudbees.hudson.plugins.folder.Folder"" reference=""../../../..""/>
<name>All</name>
<filterExecutors>false</filterExecutors>
<filterQueue>false</filterQueue>
<properties class=""hudson.model.View$PropertyList""/>
</hudson.model.AllView>
</views>
<tabBar class=""hudson.views.DefaultViewsTabBar""/>
</folderViews>
<healthMetrics/>
<icon class=""com.cloudbees.hudson.plugins.folder.icons.StockFolderIcon""/>
</com.cloudbees.hudson.plugins.folder.Folder>";

            var request = new RestRequest
            {
                Method = Method.Post,
                Resource = $"{host}/createItem?name={name}"
            };

            request.AddHeader(crumb.CrumbRequestField ?? string.Empty, crumb.Crumb ?? string.Empty);
            request.AddStringBody(content, DataFormat.Xml);

            var response = client.Execute(request);
            if (!response.IsSuccessful)
            {
                _console.WriteLine("Create folder failed");
                _console.WriteLine(response.Content ?? string.Empty);
                return false;
            }
            else
            {
                return true;
            }
        }

        public bool CreateJob(RestClient client, string host, string user, string tokenOrPassword, string folder, string name, JenkinsCrumb crumb, string content)
        {
            var request = new RestRequest
            {
                Method = Method.Post,
                Resource = $"{host}/createItem?name={name}"
            };

            if (!string.IsNullOrWhiteSpace(folder))
            {
                request.Resource = $"{host}/job/{folder}/createItem?name={name}";
            }

            request.AddHeader(crumb.CrumbRequestField ?? string.Empty, crumb.Crumb ?? string.Empty);
            request.AddStringBody(content, DataFormat.Xml);

            var response = client.Execute(request);
            if (!response.IsSuccessful)
            {
                _console.WriteLine("Create job failed");
                _console.WriteLine(response.Content ?? string.Empty);
                return false;
            }
            else
            {
                return true;
            }
        }
    }

    public class JenkinsCrumb
    {
        public string Crumb { get; set; }

        public string CrumbRequestField { get; set; }
    }

    public class ListJobResult
    {
        public List<JobItem> Jobs { get; set; }
    }

    public class JobItem
    {
        [JsonProperty("_class")]
        public string Class { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }
}
