using Microsoft.Extensions.CommandLineUtils;

namespace PLS.CommandBuilders.Dev
{
    public class HelpDecoratorCommandBuilder : ICommandBuilder
    {
        private readonly ICommandBuilder _self;

        public HelpDecoratorCommandBuilder(ICommandBuilder self)
        {
            _self = self;
        }

        public string Name => _self.Name;

        public void Configure(CommandLineApplication command)
        {
            command.HelpOption("-h|--help");
            _self.Configure(command);
        }
    }
}