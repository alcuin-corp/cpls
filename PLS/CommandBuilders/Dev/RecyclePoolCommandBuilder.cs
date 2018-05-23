using System;
using Microsoft.Extensions.CommandLineUtils;
using PLS.Services;

namespace PLS.CommandBuilders.Dev
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
                var pool = _iis.GetPool(poolNameArg.Value)
                    .IfNone(() => throw new Exception($"Pool {poolNameArg.Value} does not exist."));
                pool.Recycle();
                return 0;
            });
        }
    }
}