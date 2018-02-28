using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PLS.Utils
{
    public static class RequestsExtensions
    {
        public static async Task<string> SendAsync(this HttpClient cli, HttpMethod method, string uri, Action<HttpRequestMessage> conf)
        {
            var req = new HttpRequestMessage(method, uri);
            conf(req);
            var res = await cli.SendAsync(req);
            if (!res.IsSuccessStatusCode)
            {
                throw new Exception(await res.Content.ReadAsStringAsync());
            }
            return await res.Content.ReadAsStringAsync();
        }

        public static async Task<T> SendAsyncAs<T>(this HttpClient cli, string uri, Action<HttpRequestMessage> conf)
        {
            var res = await cli.GetAsync(uri, conf);
            return JsonConvert.DeserializeObject<T>(res);
        }

        public static Task<string> PostAsync(this HttpClient cli, string uri, Action<HttpRequestMessage> conf)
        {
            return cli.SendAsync(HttpMethod.Post, uri, conf);
        }

        public static Task<string> GetAsync(this HttpClient cli, string uri, Action<HttpRequestMessage> conf)
        {
            return cli.SendAsync(HttpMethod.Get, uri, conf);
        }

        public static Task<T> GetAsyncAs<T>(this HttpClient cli, string uri, Action<HttpRequestMessage> conf)
        {
            return cli.SendAsyncAs<T>(uri, conf);
        }

        public static Task<string> GetToken(this HttpClient cli, string uri, string login, string password)
        {
            return cli.GetAsyncAs<string>(uri, _ =>
            {
                _.SetBasicAuth(login, password);
            });
        }

        public static HttpRequestMessage SetToken(this HttpRequestMessage r, string token)
        {
            r.Headers.Add("Token", token);
            return r;
        }
        public static async Task<HttpRequestMessage> SetBasicAuth(this Task<HttpRequestMessage> r, string login, string password)
        {
            return (await r).SetBasicAuth(login, password);
        }
        public static HttpRequestMessage SetBasicAuth(this HttpRequestMessage r, string login, string password)
        {
            var cred = $"{login}:{password}";
            var encodedBytes = Encoding.UTF8.GetBytes(cred);
            var encodedTxt = Convert.ToBase64String(encodedBytes);
            r.Headers.Add("Authorization", $"Basic {encodedTxt}");
            return r;
        }

        public static void SetJsonContentFromString(this HttpRequestMessage r, string body)
        {
            r.Content = new StringContent(body, Encoding.UTF8, "application/json");
        }

        public static void SetJsonContentFromFile(this HttpRequestMessage r, string filename)
        {
            if (File.Exists(filename))
            {
                r.SetJsonContentFromString(File.ReadAllText(filename));
            }
            else
            {
                throw new Exception("File does not exists.");
            }
        }
    }
}