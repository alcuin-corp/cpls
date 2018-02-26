using System.IO;
using Microsoft.Extensions.CommandLineUtils;

namespace PLS.CommandBuilders
{
    public class ConfigCommandBuilder : ICommandBuilder
    {
        private readonly ApiClientFactory _connect;

        public ConfigCommandBuilder(ApiClientFactory connect)
        {
            _connect = connect;
        }

        public void AddConfigExportCommand(CommandLineApplication self)
        {
            self.Description = "Create a export of the targetted application into a JSON file.";

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

        public void AddConfigImportCommand(CommandLineApplication self)
        {
            self.Description = "Import targeted JSON file into an application.";

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

        public string Name => "config";

        public void Configure(CommandLineApplication command)
        {
            

            command.Description = "Those commands are related to config import/export from api.";
            command.Command("export", AddConfigExportCommand);
            command.Command("import", AddConfigImportCommand);
        }
    }
}