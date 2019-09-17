using System;
using System.IO;
using Microsoft.Extensions.CommandLineUtils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PLS.Services;

namespace PLS.CommandBuilders.Agit
{
    public class AgitMergeCommandBuilder : ICommandBuilder
    {
        private readonly AgitApiClientFactory _connect;

        public AgitMergeCommandBuilder(AgitApiClientFactory connect)
        {
            _connect = connect;
        }

        public string Name => "merge";

        public void Configure(CommandLineApplication self)
        {
            var urlArg = self.Argument("[url]", "The agit api url");
            var repoArg = self.Argument("[repository]", "The agit repository to use");
            var receivingArg = self.Argument("[receiving]", "The receiving revision");
            var incomingArg = self.Argument("[incoming]", "The incoming revision");
            var authorArg = self.Argument("[author]", "The author");
            var messageArg = self.Argument("[message]", "The merge commit message");

            self.OnExecute(async () =>
            {
                var api = _connect(urlArg.Value, repoArg.Value);
                var obj = JsonConvert.DeserializeObject<JObject>(
                    await api.Merge(receivingArg.Value, incomingArg.Value, authorArg.Value, messageArg.Value));
                WriteOutputToConsole(obj.Value<string>("commitId"));
                return 0;
            });
        }

        private static void WriteOutputToConsole(string result)
        {
            using (var writer = new StreamWriter(Console.OpenStandardOutput()))
            {
                writer.Write(result);
            }
        }
    }
}