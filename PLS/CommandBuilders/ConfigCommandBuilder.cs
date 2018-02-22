using System.IO;
using Microsoft.Extensions.CommandLineUtils;

namespace PLS.CommandBuilders
{
    public static class ConfigCommandBuilderExtensions
    {
        public static CommandArgument AddUrlArgument(this CommandLineApplication cli)
        {
            return cli.Argument("[url]", "The config api url");
        }
    }

    public class ConfigCommandBuilder : IConfigCommandBuilder
    {
        private readonly ApiClientFactory _connect;

        public static (CommandArgument, CommandArgument, CommandArgument, CommandArgument) AddImportExportArguments(CommandLineApplication self)
        {
            var urlArg = self.AddUrlArgument();
            var loginArg = self.Argument("[login]", "The api login");
            var passwordArg = self.Argument("[password]", "The api password");
            var fileArg = self.Argument("[file]", "The destination file");
            return (urlArg, loginArg, passwordArg, fileArg);
        }

        public ConfigCommandBuilder(ApiClientFactory connect)
        {
            _connect = connect;
        }

        public void AddConfigExportCommand(CommandLineApplication self)
        {
            self.AddHelp();

            self.Description = "Create a export of the targetted application into a JSON file.";

            var (urlArg, loginArg, passwordArg, fileArg) = AddImportExportArguments(self);

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
            self.AddHelp();

            self.Description = "Import targeted JSON file into an application.";

            var (urlArg, loginArg, passwordArg, fileArg) = AddImportExportArguments(self);
            var maybeOutputFile = self.Option("-o|--outputFile", "A file in which the import result will be stored.",
                CommandOptionType.SingleValue);

            self.OnExecute(async () =>
            {
                var api = _connect(urlArg.Value, loginArg.Value, passwordArg.Value);
                var result = await api.PostConfig(fileArg.Value);
                if (maybeOutputFile.HasValue())
                {
                    var outputFile = maybeOutputFile.Value();
                    using (var file = File.OpenWrite(outputFile))
                    using (var writer = new StreamWriter(file))
                    {
                        writer.Write(result);
                    }
                }
                return 0;
            });
        }

        public void Apply(CommandLineApplication self)
        {
            self.Command("config", command =>
            {
                command.AddHelp();

                command.Description = "Those commands are related to config import/export from api.";
                command.Command("export", AddConfigExportCommand);
                command.Command("import", AddConfigImportCommand);
            });
        }

    }
}