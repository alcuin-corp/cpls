using Microsoft.Extensions.CommandLineUtils;
using PLS.CommandBuilders.Dev;
using PLS.Services;

namespace PLS.CommandBuilders.Config
{
    public class ImportConfigCommandBuilder : ICommandBuilder
    {
        private readonly ConfigApiClientFactory _connect;

        public ImportConfigCommandBuilder(ConfigApiClientFactory connect)
        {
            _connect = connect;
        }

        public string Name => "import";

        public void Configure(CommandLineApplication self)
        {
            var urlArg = self.AddUrlArgument();
            var loginArg = self.AddLoginArgument();
            var passwordArg = self.AddPasswordArgument();
            var fileArg = self.AddJsonFileArgument();
            var maybeOutputFile = self.AddResultFileOption();

            self.OnExecute(async () =>
            {
                var api = _connect(urlArg.Value, loginArg.Value, passwordArg.Value);
                maybeOutputFile.WriteResultIfPossible(await api.PostConfig(fileArg.Value));
                return 0;
            });
        }
    }
}