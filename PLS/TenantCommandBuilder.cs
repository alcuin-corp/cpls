using Microsoft.Extensions.CommandLineUtils;

namespace PLS
{
    public class TenantCommandBuilder : ICommandBuilder
    {
        private readonly ICommandBuilder[] _builders;

        public TenantCommandBuilder(params ICommandBuilder[] builders)
        {
            _builders = builders;
        }

        public void Apply(CommandLineApplication target)
        {
            target.Command("tenant", command =>
            {
                command.Description = "Those commands are related to tenant manipulations.";
                command.AddHelp();
                foreach (var builder in _builders)
                {
                    builder.Apply(command);
                }
            });
        }
    }
}