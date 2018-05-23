using System;
using Microsoft.Extensions.CommandLineUtils;

namespace PLS.CommandBuilders.Agit
{
    public class TagListCommandBuilder : ICommandBuilder
    {
        private readonly IAgitServices _agit;
        public string Name => "tag-list";


        public TagListCommandBuilder(IAgitServices agit)
        {
            _agit = agit;
        }

        public void Configure(CommandLineApplication command)
        {
            command.Description =
                "create a copy of this patch with its hash as filename and references to others patches";

            var repoFolderOpt =
                command.Option("--repo-folder | -r", "target repository", CommandOptionType.SingleValue);

            command.OnExecute(() =>
            {
                var repository = _agit.LoadFromDirectory(repoFolderOpt.Value());

                if (!repository.IsReady)
                    throw new Exception($"{repoFolderOpt.Value()} is not a valid repository");

                Console.Write(string.Join("\n", repository.Tags));

                return 0;
            });

        }

    }
}