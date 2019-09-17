using System;
using System.IO;
using Microsoft.Extensions.CommandLineUtils;
using PLS.Services;

namespace PLS.CommandBuilders.Agit
{
    public class AgitPostCommitCommandBuilder : ICommandBuilder
    {
        private readonly AgitApiClientFactory _connect;

        public AgitPostCommitCommandBuilder(AgitApiClientFactory connect)
        {
            _connect = connect;
        }

        public string Name => "post-commit";

        public void Configure(CommandLineApplication self)
        {
            var urlArg = self.Argument("[url]", "The agit api url");
            var repoArg = self.Argument("[repository]", "The agit repository to use");
            var filenameArg = self.Argument("[filename]", "The name of the file to commit");
            var authorArg = self.Argument("[author]", "The author");
            var messageArg = self.Argument("[message]", "The commit message");
            var branchNameArg = self.Argument("[branchName]", "The branch name");

            self.OnExecute(async () =>
            {
                var api = _connect(urlArg.Value, repoArg.Value);
                WriteOutputToConsole(await api.PostCommit(filenameArg.Value, authorArg.Value, messageArg.Value, branchNameArg.Value));
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