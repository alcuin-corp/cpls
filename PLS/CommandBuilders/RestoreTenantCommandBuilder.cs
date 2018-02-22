using System.Threading.Tasks;
using Microsoft.Extensions.CommandLineUtils;

namespace PLS.CommandBuilders
{
    public class RestoreTenantCommandBuilder : ICommandBuilder
    {
        public string Name => "restore";

        public void Configure(CommandLineApplication target)
        {
        }
    }
}