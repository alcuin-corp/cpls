using Microsoft.Extensions.CommandLineUtils;

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

            var directoryArg =
                command.Argument("folder", "the folder we want to setup for storing application patches");

            command.OnExecute(() =>
            {
                _agit.LoadFromDirectory(directoryArg.Value).Initialize();
                return 0;
            });
        }
    }
}