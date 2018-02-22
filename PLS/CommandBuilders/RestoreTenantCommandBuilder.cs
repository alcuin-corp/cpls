using System.Threading.Tasks;
using Microsoft.Extensions.CommandLineUtils;

namespace PLS.CommandBuilders
{
    public class RestoreTenantCommandBuilder : ICommandBuilder
    {
        public CommandBuilder()
        {
        }

        public void Apply(CommandLineApplication target)
        {
            target.Command(_config.Name, app =>
            {
                _config.Configure(app);
                app.OnExecute(() => _config.OnExecute());
            });
        }
    }
}