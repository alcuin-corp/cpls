using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.CommandLineUtils;

namespace PLS
{
    public static class RequestsExtensions
    {
        public static HttpResponseMessage Get(this Requests self, string uri, object headers)
        {
            return self.Get(new Uri(uri), headers);
        }
    }

    public class Requests
    {
        private readonly Uri _baseUri;

        public Requests(string baseUri)
        {
            _baseUri = new Uri(baseUri);
        }

        public Task<HttpResponseMessage> GetAsync(string uri, object headers)
        {
            var req = new HttpRequestMessage(HttpMethod.Get, uri);
            var cli = new HttpClient {BaseAddress = new Uri(uri)};
            return cli.SendAsync(req);
        }

        public Task<HttpResponseMessage> PostAsync(string uri, object headers, object body)
        {
            var req = new HttpRequestMessage(HttpMethod.Post, uri);
            var cli = new HttpClient { BaseAddress = new Uri(uri) };
            return cli.SendAsync(req);
        }
    }

    public class ConfigCommandBuilder : IConfigCommandBuilder
    {
        public void AddConfigExportCommand(CommandLineApplication self)
        {
            self.Description = "Create a export of the targetted application into a JSON file.";
            self.AddHelp();

            var urlArg = self.Argument("[url]", "The config api url");
            var loginArg = self.Argument("[login]", "The api login");
            var passwordArg = self.Argument("[password]", "The api password");

            self.OnExecute(async () =>
            {
                var cli = new HttpClient {BaseAddress = new Uri(urlArg.Value)};

                
                cli.GetAsync("/login");

                var resp = await cli.GetAsync("/config");
                if (!resp.IsSuccessStatusCode)
                {
                    throw new Exception($"Status: {resp.StatusCode}");
                }
                return 0;
            });
        }

        public void AddConfigImportCommand(CommandLineApplication self)
        {
            
        }

        public void Apply(CommandLineApplication self)
        {
            self.Command("config", command =>
            {
                command.Description = "Those commands are related to config import/export from api.";
                command.AddHelp();

                self.Command("export", AddConfigExportCommand);
                self.Command("import", AddConfigImportCommand);
            });
        }

    }
}