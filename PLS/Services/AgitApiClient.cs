using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PLS.Utils;

namespace PLS.Services
{
    public class AgitApiClient : IAgitApiClient
    {
        private readonly string _apiUri;
        private readonly string _repo;
        private readonly HttpClient _cli;

        public static AgitApiClientFactory Factory { get; } = (uri, repo) => new AgitApiClient(uri, repo);

        protected AgitApiClient(string apiUri, string repo)
        {
            _apiUri = apiUri;
            _repo = repo;
            _cli = new HttpClient
            {
                Timeout = Timeout.InfiniteTimeSpan
            };
        }

        public async Task<string> PostCommit(string filename, string author, string message, string branchName)
        {
            using (var stream = File.OpenRead(filename))
            {
                return await _cli.PostAsync($"{_apiUri}/{_repo}/commit", requestMessage =>
                    requestMessage.Content = new MultipartFormDataContent
                    {
                        {new StringContent(author), "Author"},
                        {new StringContent(message), "Message"},
                        {new StringContent(branchName), "BranchName"},
                        {new StreamContent(stream), "JsonFile", "JsonFile"}
                    }
                );
            }
        }

        public async Task<string> PostBranch(string branchName, string revision)
        {
            var json = JsonConvert.SerializeObject(new
            {
                Name = branchName,
                Revision = revision,
            });
            return await _cli.PostAsync($"{_apiUri}/{_repo}/branch", requestMessage =>
                requestMessage.Content = new StringContent(json, Encoding.UTF8, "application/json")
            );
        }

        public async Task<string> GetBranch(string branchName)
        {
            return await _cli.GetAsync($"{_apiUri}/{_repo}/branch", requestMessage =>
                requestMessage.Content = new MultipartFormDataContent
                {
                    {new StringContent(branchName), "BranchName"},
                }
            );
        }

        public async Task<string> PostTag(string tagName, string revision)
        {
            var json = JsonConvert.SerializeObject(new
            {
                Name = tagName,
                Revision = revision,
            });
            return await _cli.PostAsync($"{_apiUri}/{_repo}/tag", requestMessage =>
                requestMessage.Content = new StringContent(json, Encoding.UTF8, "application/json")
            );
        }

        public async Task<string> Checkout(string revision)
        {
            return await _cli.GetAsync($"{_apiUri}/{_repo}/checkout", requestMessage => 
                requestMessage.Content = new MultipartFormDataContent
                {
                    { new StringContent(revision), "Revision" }
                }
            );
        }

        public async Task<string> Merge(string receiving, string incoming, string author, string message)
        {
            var json = JsonConvert.SerializeObject(new
            {
                ReceivingRevision = receiving,
                IncomingRevision = incoming,
                Author = author,
                Message = message,
            });
            return await _cli.PostAsync($"{_apiUri}/{_repo}/merge", requestMessage =>
                requestMessage.Content = new StringContent(json, Encoding.UTF8, "application/json")
            );
        }
    }
}