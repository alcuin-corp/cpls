using Microsoft.Extensions.CommandLineUtils;

namespace PLS.CommandBuilders
{
    public class TenantListCommandBuilder : ICommandBuilder
    {
        public string Name => "tenant-list";
        public void Configure(CommandLineApplication command)
        {
            command.AddHelp();
        }
    }
}