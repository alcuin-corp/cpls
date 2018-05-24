using System.IO;
using Microsoft.Extensions.CommandLineUtils;
using PLS.Agit;

namespace PLS.CommandBuilders.Agit
{
    public class InitRepositoryBuilder : ICommandBuilder
    {
        public string Name => "init";

        private readonly IAgitServices _agit;

        public InitRepositoryBuilder(IAgitServices agit)
        {
            _agit = agit;
        }

        public void Configure(CommandLineApplication command)
        {
            command.Description = "initialize a repository";

            var directoryOpt =
                command.Option("--directory | -d", "the folder we want to setup for storing application patches", CommandOptionType.SingleValue);

            command.OnExecute(() =>
            {
                _agit.LoadFromDirectory(directoryOpt.Value() ?? Directory.GetCurrentDirectory()).Initialize();
                return 0;
            });
        }
    }
}