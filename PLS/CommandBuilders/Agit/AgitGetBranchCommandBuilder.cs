using System;
using System.IO;
using Microsoft.Extensions.CommandLineUtils;
using PLS.Services;

namespace PLS.CommandBuilders.Agit
{
    public class AgitGetBranchCommandBuilder : ICommandBuilder
    {
        private readonly AgitApiClientFactory _connect;

        public AgitGetBranchCommandBuilder(AgitApiClientFactory connect)
        {
            _connect = connect;
        }

        public string Name => "get-branch";

        public void Configure(CommandLineApplication self)
        {
            var urlArg = self.Argument("[url]", "The agit api url");
            var repoArg = self.Argument("[repository]", "The agit repository to use");
            var branchNameArg = self.Argument("[branchName]", "The name of the branch");

            self.OnExecute(async () =>
            {
                var api = _connect(urlArg.Value, repoArg.Value);
                WriteOutputToConsole(await api.GetBranch(branchNameArg.Value));
                return 0;
            });
        }

        private static void WriteOutputToConsole(string result)
        {
            using (var writer = new StreamWriter(Console.OpenStandardOutput()))
            {
                writer.Write(result);
            }
        }
    }
}