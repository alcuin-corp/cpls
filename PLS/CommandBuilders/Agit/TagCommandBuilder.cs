using System;
using System.IO;
using Microsoft.Extensions.CommandLineUtils;
using PLS.Agit;
using PLS.Utils;

namespace PLS.CommandBuilders.Agit
{
    public static class CommandBuilderExtensions
    {
        public static CommandOption AddDirectoryOption(this CommandLineApplication self)
        {
            return self.Option("--directory | -d", "the repository's directory", CommandOptionType.SingleValue);
        }

        public static string ValueOrCurrentDirectory(this CommandOption opt)
        {
            return opt.Some().IfNone(Directory.GetCurrentDirectory);
        }
    }

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
            command.Description = "create a copy of this patch with its hash as filename and references to others patches";
            var nameArg = command.Argument("name", "tag name");
            var directoryOpt = command.AddDirectoryOption();
            command.OnExecute(() =>
            {
                var repository = _agit.LoadFromDirectory(directoryOpt.ValueOrCurrentDirectory());
                if (!repository.IsReady)
                    throw new Exception($"{Directory.GetCurrentDirectory()} is not a valid repository");
                repository.Tag(nameArg.Value);
                return 0;
            });
        }
    }
}