using Microsoft.Extensions.CommandLineUtils;

namespace PLS
{
    public static class GenericCommandExtensions
    {
        public static CommandLineApplication AddHelp(this CommandLineApplication self)
        {
            self.HelpOption("-?|-h|--help");
            return self;
        }
    }
}