using Microsoft.Extensions.CommandLineUtils;

namespace PLS.CommandBuilders
{
    public class HelpDecoratorCommandBuilder : ICommandBuilder
    {
        private readonly ICommandBuilder _inner;
        public HelpDecoratorCommandBuilder(ICommandBuilder inner)
        {
            _inner = inner;
        }

        public string Name => _inner.Name;
        public void Configure(CommandLineApplication command)
        {
            command.AddHelp();
            _inner.Configure(command);
        }
    }
}