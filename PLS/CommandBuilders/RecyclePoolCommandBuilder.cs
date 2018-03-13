using Microsoft.Extensions.CommandLineUtils;
using Optional.Unsafe;
using PLS.Services;

namespace PLS.CommandBuilders
{
    public class RecyclePoolCommandBuilder : ICommandBuilder
    {
        private readonly IIisService _iis;

        public RecyclePoolCommandBuilder(IIisService iis)
        {
            _iis = iis;
        }

        public string Name => "recycle-pool";
        public void Configure(CommandLineApplication command)
        {
            command.Description = "recycle the named pool";
            var poolNameArg = command.Argument("pool-name", "the name of the pool");

            command.OnExecute(() =>
            {
                var pool = _iis.GetPool(poolNameArg.Value).ValueOrFailure($"Pool {poolNameArg.Value} does not exist.");
                pool.Recycle();
                return 0;
            });
        }
    }
}