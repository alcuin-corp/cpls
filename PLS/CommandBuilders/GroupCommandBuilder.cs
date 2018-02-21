using Microsoft.Extensions.CommandLineUtils;

namespace PLS
{
    public class GroupCommandBuilder : ICommandBuilder
    {
        private readonly string _commandName;
        private readonly ICommandBuilder[] _builders;

        public GroupCommandBuilder(string commandName, params ICommandBuilder[] builders)
        {
            _commandName = commandName;
            _builders = builders;
        }

        public void Apply(CommandLineApplication self)
        {
            self.Command(_commandName, command =>
            {
                command.AddHelp();

                command.Description = "Those commands are related to server manipulations.";
                foreach (var builder in _builders)
                {
                    builder.Apply(command);
                }
            });
        }
    }
}