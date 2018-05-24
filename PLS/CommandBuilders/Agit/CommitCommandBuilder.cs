using System;
using Microsoft.Extensions.CommandLineUtils;
using PLS.Agit;

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
            var directoryOpt = command.AddDirectoryOption();
            var authorOpt = command.Option("--author | -a", "the email of the patch's author",
                CommandOptionType.SingleValue);
            var messageOpt = command.Option("--message | -m", "a description of the patch's content", CommandOptionType.SingleValue);

            command.OnExecute(() =>
            {
                var directory = directoryOpt.ValueOrCurrentDirectory();
                var repository = _agit.LoadFromDirectory(directory);

                if (!repository.IsReady)
                    throw new Exception($"{directory} is not a valid repository");

                if (!authorOpt.HasValue())
                    Console.WriteLine("Warning => you didn't specify any author for this patch");
                if (!messageOpt.HasValue())
                    Console.WriteLine("Warning => you didn't specify a descriptive message for this patch");

                repository.CommitJsonFile(patchFileNameArg.Value, authorOpt.Value(), messageOpt.Value());

                return 0;
            });

        }


    }
}