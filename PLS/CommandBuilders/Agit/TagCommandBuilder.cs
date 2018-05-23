using System;
using Microsoft.Extensions.CommandLineUtils;

namespace PLS.CommandBuilders.Agit
{
    public class TagCommandBuilder : ICommandBuilder
    {
        private readonly IAgitServices _agit;
        public string Name => "tag";


        public TagCommandBuilder(IAgitServices agit)
        {
            _agit = agit;
        }

        public void Configure(CommandLineApplication command)
        {
            command.Description =
                "create a copy of this patch with its hash as filename and references to others patches";

            var nameArg = command.Argument("name", "tag name");
            var repoFolderOpt =
                command.Option("--repo-folder | -r", "target repository", CommandOptionType.SingleValue);

            command.OnExecute(() =>
            {
                var repository = _agit.LoadFromDirectory(repoFolderOpt.Value());

                if (!repository.IsReady)
                    throw new Exception($"{repoFolderOpt.Value()} is not a valid repository");

                repository.Tag(nameArg.Value);

                return 0;
            });

        }

    }
}