using System.IO;
using Microsoft.Extensions.CommandLineUtils;

namespace PLS.CommandBuilders.Dev
{
    public static class ConfigCommandBuilderExtensions
    {
        public static CommandArgument AddUrlArgument(this CommandLineApplication self)
        {
            return self.Argument("[url]", "The config api url");
        }

        public static CommandArgument AddLoginArgument(this CommandLineApplication self)
        {
            return self.Argument("[login]", "The api login");
        }

        public static CommandArgument AddPasswordArgument(this CommandLineApplication self)
        {
            return self.Argument("[password]", "The api password");
        }

        public static CommandArgument AddJsonFileArgument(this CommandLineApplication self)
        {
            return self.Argument("[file]", "The destination file");
        }

        public static CommandOption AddResultFileOption(this CommandLineApplication self)
        {
            return self.Option("-o|--outputFile", "A file in which the import result will be stored.",
                CommandOptionType.SingleValue);
        }

        public static void WriteResultIfPossible(this CommandOption self, string result)
        {
            if (self.HasValue())
            {
                var outputFile = self.Value();
                using (var file = File.OpenWrite(outputFile))
                using (var writer = new StreamWriter(file))
                {
                    writer.Write(result);
                }
            }
        }
    }
}