﻿using Microsoft.Extensions.CommandLineUtils;
using PLS.Services;

namespace PLS.CommandBuilders.Agit
{
    public class AgitPostBranchCommandBuilder : ICommandBuilder
    {
        private readonly AgitApiClientFactory _connect;

        public AgitPostBranchCommandBuilder(AgitApiClientFactory connect)
        {
            _connect = connect;
        }

        public string Name => "post-branch";

        public void Configure(CommandLineApplication self)
        {
            var urlArg = self.Argument("[url]", "The agit api url");
            var repoArg = self.Argument("[repository]", "The agit repository to use");
            var branchNameArg = self.Argument("[branchName]", "The name of the new branch");
            var revisionArg = self.Argument("[revision]", "The revision to create the branch from");

            self.OnExecute(async () =>
            {
                var api = _connect(urlArg.Value, repoArg.Value);
                await api.PostBranch(branchNameArg.Value, revisionArg.Value);
                return 0;
            });
        }
    }
}