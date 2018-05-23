using System.IO;
using Microsoft.Extensions.CommandLineUtils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PLS.CommandBuilders.Agit
{
    public class FormatPatchBuilder : ICommandBuilder
    {
        public string Name => "format-patch";

        public void Configure(CommandLineApplication command)
        {
            command.Description = "prettify or uglify the given patch, prettifying expands the JSON into readable format, uglifying compresses it";

            var patchFileNameArg = command.Argument("source", "the patch we want to work with");
            command.Option("--ugly | -u", "uglify the patch", CommandOptionType.NoValue);
            var prettyOpt = command.Option("--pretty | -p", "prettify the patch", CommandOptionType.NoValue);

            command.OnExecute(() =>
            {
                var jObj = JObject.Parse(File.ReadAllText(patchFileNameArg.Value));
                using (var file = new StreamWriter(File.Open(patchFileNameArg.Value, FileMode.Create, FileAccess.Write)))
                {
                    file.Write(jObj.ToString(prettyOpt.HasValue() ? Formatting.Indented : Formatting.None));
                }
                return 0;
            });
        }
    }
}