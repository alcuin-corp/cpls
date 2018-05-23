using System;
using System.Linq;
using Microsoft.Extensions.CommandLineUtils;

namespace PLS.CommandBuilders.Agit
{
    public class CommitCommandBuilder : ICommandBuilder
    {
        private readonly IAgitServices _agit;
        public string Name => "commit";


        public CommitCommandBuilder(IAgitServices agit)
        {
            _agit = agit;
        }

        public void Configure(CommandLineApplication command)
        {
            command.Description = "create a copy of this patch with its hash as filename and references to others patches";

            var patchFileNameArg = command.Argument("source", "the patch we want to work with");
            var repoFolderOpt = command.Option("--repo-folder | -r", "target repository", CommandOptionType.SingleValue);
            var authorOpt = command.Option("--author | -a", "the email of the patch's author",
                CommandOptionType.SingleValue);
            var messageOpt = command.Option("--message | -m", "a description of the patch's content", CommandOptionType.SingleValue);
            var saveIndentedOpt = command.Option("--save-indented | -i", "save the new patch as indented json", CommandOptionType.NoValue);

            command.OnExecute(() =>
            {
                var repository = _agit.LoadFromDirectory(repoFolderOpt.Value());

                if (!repository.IsReady)
                    throw new Exception($"{repoFolderOpt.Value()} is not a valid repository");

                if (!authorOpt.HasValue())
                    Console.WriteLine("Warning => you didn't specify any author for this patch");
                if (!messageOpt.HasValue())
                    Console.WriteLine("Warning => you didn't specify a descriptive message for this patch");

                repository.Commit(patchFileNameArg.Value, authorOpt.Value(), messageOpt.Value());

                return 0;
            });

        }


    }
}