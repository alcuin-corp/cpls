using Microsoft.Extensions.CommandLineUtils;

namespace PLS.CommandBuilders
{
    public class GroupCommandBuilder : ICommandBuilder
    {
        private readonly ICommandBuilder[] _builders;

        public GroupCommandBuilder(string name, params ICommandBuilder[] builders)
        {
            _builders = builders;
            Name = name;
        }
        public string Name { get; }
        public void Configure(CommandLineApplication command)
        {
            command.AddHelp();
            foreach (var builder in _builders)
            {
                command.Command(builder.Name, builder.Configure);
            }
        }
    }
}