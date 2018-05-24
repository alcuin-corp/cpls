using System.IO;
using Microsoft.Extensions.CommandLineUtils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PLS.CommandBuilders.Agit
{
    public class FormatJsonBuilder : ICommandBuilder
    {
        public string Name => "format-json";

        public void Configure(CommandLineApplication command)
        {
            command.Description = "prettify or uglify the given json file, prettifying expands it into readable format, uglifying compresses it";

            var jsonFileNameArg = command.Argument("source", "the json we want to work with");
            command.Option("--ugly | -u", "uglify the patch", CommandOptionType.NoValue);
            var prettyOpt = command.Option("--pretty | -p", "prettify the json", CommandOptionType.NoValue);

            command.OnExecute(() =>
            {
                var jObj = JObject.Parse(File.ReadAllText(jsonFileNameArg.Value));
                using (var file = new StreamWriter(File.Open(jsonFileNameArg.Value, FileMode.Create, FileAccess.Write)))
                {
                    file.Write(jObj.ToString(prettyOpt.HasValue() ? Formatting.Indented : Formatting.None));
                }
                return 0;
            });
        }
    }
}