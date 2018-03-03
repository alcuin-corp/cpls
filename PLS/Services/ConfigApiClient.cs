using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using PLS.Utils;

namespace PLS.Services
{
    public class ConfigApiClient : IConfigApiClient
    {
        private readonly string _apiUri;
        private readonly string _login;
        private readonly string _password;
        private readonly HttpClient _cli;

        public static ConfigApiClientFactory Factory { get; } = (uri, login, password) => new ConfigApiClient(uri, login, password);

        public ConfigApiClient(string apiUri, string login, string password)
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
}