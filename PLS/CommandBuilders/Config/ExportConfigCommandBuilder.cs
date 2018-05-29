using System.IO;
using Microsoft.Extensions.CommandLineUtils;
using PLS.CommandBuilders.Dev;
using PLS.Services;

namespace PLS.CommandBuilders.Config
{
    public class ExportConfigCommandBuilder : ICommandBuilder
    {
        private readonly ConfigApiClientFactory _connect;
        public ExportConfigCommandBuilder(ConfigApiClientFactory connect)
        {
            _connect = connect;
        }

        public string Name => "export";
        public void Configure(CommandLineApplication self)
        {
            var urlArg = self.AddUrlArgument();
            var loginArg = self.AddLoginArgument();
            var passwordArg = self.AddPasswordArgument();
            var fileArg = self.AddJsonFileArgument();

            self.OnExecute(async () =>
            {
                var api = _connect(urlArg.Value, loginArg.Value, passwordArg.Value);
                var config = await api.GetConfig();
                using (var fl = File.OpenWrite(fileArg.Value))
                using (var tw = new StreamWriter(fl))
                {
                    tw.Write(config);
                }
                return 0;
            });
        }
    }
}