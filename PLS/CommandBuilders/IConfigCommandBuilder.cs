using Microsoft.Extensions.CommandLineUtils;

namespace PLS.CommandBuilders
{
    public interface IConfigCommandBuilder : ICommandBuilder
    {
        void AddConfigExportCommand(CommandLineApplication self);
        void AddConfigImportCommand(CommandLineApplication self);
    }
}