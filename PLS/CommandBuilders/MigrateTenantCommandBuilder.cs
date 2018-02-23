using Microsoft.Extensions.CommandLineUtils;

namespace PLS.CommandBuilders
{
    public class MigrateTenantCommandBuilder : ICommandBuilder
    {
        public string Name => "migrate-tenant";
        public void Configure(CommandLineApplication command)
        {
            command.AddHelp();
            command.Description = "Migrates a tenant to the last compiled version";
            var nameArg = command.Argument("name", "The tenant name");
        }
    }
}