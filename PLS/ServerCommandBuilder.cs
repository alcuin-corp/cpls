using Microsoft.Extensions.CommandLineUtils;

namespace PLS
{
    public class ServerCommandBuilder : ICommandBuilder
    {
        private readonly ICommandBuilder[] _builders;

        public ServerCommandBuilder(params ICommandBuilder[] builders)
        {
            _builders = builders;
        }

        public void Apply(CommandLineApplication self)
        {
            self.Command("server", command =>
            {
                command.Description = "Those commands are related to server manipulations.";
                command.AddHelp();
                foreach (var builder in _builders)
                {
                    builder.Apply(command);
                }
            });
        }
    }
}