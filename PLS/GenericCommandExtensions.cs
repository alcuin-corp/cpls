using Microsoft.Extensions.CommandLineUtils;
using PLS.CommandBuilders;

namespace PLS
{
    public static class GenericCommandExtensions
    {
        public static CommandLineApplication AddHelp(this CommandLineApplication self)
        {
            self.HelpOption("-?|-h|--help");
            return self;
        }

        public static CommandLineApplication Use(this CommandLineApplication self, params ICommandBuilder[] builders)
        {
            foreach (var builder in builders)
            {
                self.Command(builder.Name, builder.Configure);
            }
            return self;
        }
    }
}