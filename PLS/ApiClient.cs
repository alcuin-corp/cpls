using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace PLS
{
    public delegate IApiClient ApiClientFactory(string uri, string login, string password);

    public class ApiClient : IApiClient
    {
        private readonly string _apiUri;
        private readonly string _login;
        private readonly string _password;
        private readonly HttpClient _cli;

        public static IApiClient Factory(string uri, string login, string password)
        {
            return new ApiClient(uri, login, password);
        }

        public ApiClient(string apiUri, string login, string password)
        {
            _apiUri = apiUri;
            _login = login;
            _password = password;
            _cli = new HttpClient
            {
                Timeout = Timeout.InfiniteTimeSpan
            };
        }

        public async Task<string> GetToken()
        {
            return await _cli.GetToken($"{_apiUri}/login", _login, _password);
        }

        public async Task<string> GetConfig()
        {
            var token = await GetToken();
            return await _cli.GetAsync($"{_apiUri}/config", _ => _.SetToken(token));
        }

        public async Task<string> PostConfig(string filename)
        {
            var token = await GetToken();
            return await _cli.PostAsync($"{_apiUri}/config", _ =>
            {
                _.SetToken(token);
                _.SetJsonContentFromFile(filename);
            });
        }
    }

    public interface IApiClient
    {
        Task<string> GetToken();
        Task<string> GetConfig();
        Task<string> PostConfig(string filename);
    }
}