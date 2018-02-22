using Microsoft.Extensions.CommandLineUtils;

namespace PLS.CommandBuilders
{
    public interface ICommandBuilder
    {
        string Name { get; }
        void Configure(CommandLineApplication command);
    }
}