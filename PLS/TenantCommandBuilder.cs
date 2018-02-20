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

        public void Apply(CommandLineApplication self)
        {
            self.Command("tenant", command =>
            {
                command.AddHelp();

                command.Description = "Those commands are related to tenant manipulations.";
                foreach (var builder in _builders)
                {
                    builder.Apply(command);
                }
            });
        }
    }
}