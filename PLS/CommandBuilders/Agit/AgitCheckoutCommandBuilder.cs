using System;
using System.IO;
using Microsoft.Extensions.CommandLineUtils;
using PLS.Services;

namespace PLS.CommandBuilders.Agit
{
    public class AgitCheckoutCommandBuilder : ICommandBuilder
    {
        private readonly AgitApiClientFactory _connect;

        public AgitCheckoutCommandBuilder(AgitApiClientFactory connect)
        {
            _connect = connect;
        }

        public string Name => "checkout";

        public void Configure(CommandLineApplication self)
        {
            var urlArg = self.Argument("[url]", "The agit api url");
            var repoArg = self.Argument("[repository]", "The agit repository to use");
            var revisionArg = self.Argument("[revision]", "The revision to checkout");
            var outputFileArg = self.Argument("[output]", "A file in which the checkout result will be stored");

            self.OnExecute(async () =>
            {
                var api = _connect(urlArg.Value, repoArg.Value);
                var result = await api.Checkout(revisionArg.Value);
                using (var file = File.OpenWrite(outputFileArg.Value))
                using (var writer = new StreamWriter(file))
                {
                    writer.Write(result);
                }
                return 0;
            });
        }
    }
}