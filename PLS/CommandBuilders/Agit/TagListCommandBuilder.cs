using System;
using Microsoft.Extensions.CommandLineUtils;
using PLS.Agit;

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

            var directoryOpt = command.AddDirectoryOption();

            command.OnExecute(() =>
            {
                var repository = _agit.LoadFromDirectory(directoryOpt.ValueOrCurrentDirectory());

                if (!repository.IsReady)
                    throw new Exception($"{directoryOpt.ValueOrCurrentDirectory()} is not a valid repository");

                Console.Write(string.Join("\n", repository.Tags));

                return 0;
            });

        }

    }
}