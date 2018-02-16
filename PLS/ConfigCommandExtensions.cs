using Microsoft.Extensions.CommandLineUtils;

namespace PLS
{
    public class ConfigCommandBuilder : IConfigCommandBuilder
    {
        public void AddConfigExportCommand(CommandLineApplication self)
        {
            self.Description = "Create a export of the targetted application into a JSON file.";
            self.AddHelp();
            self.OnExecute(() =>
            {



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