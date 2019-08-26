using System;
using System.IO;
using Microsoft.Extensions.CommandLineUtils;
using PLS.CommandBuilders.Dev;
using PLS.Services;

namespace PLS.CommandBuilders.Config
{
    public class InstanceInfoCommandBuilder : ICommandBuilder
    {
        private readonly ConfigApiClientFactory _connect;

        public InstanceInfoCommandBuilder(ConfigApiClientFactory connect)
        {
            _connect = connect;
        }

        public string Name => "info";

        public void Configure(CommandLineApplication self)
        {
            var urlArg = self.AddUrlArgument();
            var loginArg = self.AddLoginArgument();
            var passwordArg = self.AddPasswordArgument();

            self.OnExecute(async () =>
            {
                var api = _connect(urlArg.Value, loginArg.Value, passwordArg.Value);
                WriteOutputToConsole(await api.GetInstanceInfo());
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