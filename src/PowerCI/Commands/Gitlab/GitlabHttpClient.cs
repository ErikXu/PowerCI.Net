using Newtonsoft.Json;
using PowerCI.Commands.Gitlab.Requests;
using PowerCI.Commands.Gitlab.Responses;
using RestSharp;
using RestSharp.Authenticators.OAuth2;

namespace PowerCI.Commands.Gitlab
{
    internal class GitlabHttpClient : IDisposable
    {
        private readonly RestClient _client;
        private readonly string _apiVersion;

        public GitlabHttpClient()
        {
            _client = new RestClient();
            _apiVersion = "/api/v4";
        }

        public OauthResponse GrantOauthToken(string host, OauthRequest request)
        {
            var response = Call<OauthRequest, OauthResponse>(Method.Post, host, "/oauth/token", request);
            _client.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(response.AccessToken, "Bearer");
            return response;
        }

        public ListUserResponse GetUserByUsername(string host, string username)
        {
            return Call<dynamic, ListUserResponse>(Method.Get, host, $"{_apiVersion}/users?username={username}", null);
        }

        public CreateUserResponse CreateUser(string host, CreateUserRequest request)
        {
            return Call<CreateUserRequest, CreateUserResponse>(Method.Post, host, $"{_apiVersion}/users", request);
        }

        public CreatePersonalAccessTokenResponse CreatePersonalAccessToken(string host, int userId, CreatePersonalAccessTokenRequest request)
        {
            return Call<CreatePersonalAccessTokenRequest, CreatePersonalAccessTokenResponse>(Method.Post, host, $"{_apiVersion}/users/{userId}/personal_access_tokens", request);
        }

        public string Call<TRequest>(Method method, string host, string api, TRequest request = null, string token = null) where TRequest : class
        {
            var url = $"{host.TrimEnd('/')}/{api.Trim('/')}";
            var restRequest = new RestRequest
            {
                Method = method,
                Resource = url
            };

            if (!string.IsNullOrWhiteSpace(token))
            {
                restRequest.AddHeader("PRIVATE-TOKEN", token);
            }

            if (request != null)
            {
                var content = JsonConvert.SerializeObject(request);
                restRequest.AddBody(content);
            }

            var restResponse = _client.Execute(restRequest);
            return restResponse.Content;
        }

        public TResponse Call<TRequest, TResponse>(Method method, string host, string api, TRequest request = null, string token = null) where TResponse : class where TRequest : class
        {
            var url = $"{host.TrimEnd('/')}/{api.Trim('/')}";
            var restRequest = new RestRequest
            {
                Method = method,
                Resource = url
            };

            if (!string.IsNullOrWhiteSpace(token))
            {
                restRequest.AddHeader("PRIVATE-TOKEN", token);
            }

            if (request != null)
            {
                var content = JsonConvert.SerializeObject(request);
                restRequest.AddBody(content);
            }
           
            var restResponse = _client.Execute(restRequest);

            if (!restResponse.IsSuccessful)
            {
                throw new Exception($"Call [{method}]{url} failed, status code: {restResponse.StatusCode}, message: {restResponse.Content}");
            }

            var response = JsonConvert.DeserializeObject<TResponse>(restResponse.Content);

            return response;
        }

        public void Dispose()
        {
            if (_client != null)
            {
                _client.Dispose();
            }
        }
    }
}
