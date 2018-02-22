using Microsoft.Extensions.CommandLineUtils;

namespace PLS.CommandBuilders
{
    public class TenantCommandBuilder : ICommandBuilder
    {
        private readonly AddTenantCommandBuilder _add;

        public TenantCommandBuilder(AddTenantCommandBuilder add)
        {
            _add = add;
        }

        public string Name => "tenant";
        public void Configure(CommandLineApplication command)
        {
            command.AddHelp();
            command.Command(_add.Name, _add.Configure);
        }
    }
}