using Microsoft.Extensions.CommandLineUtils;

namespace PLS.CommandBuilders
{
    public interface ICommandBuilder
    {
        void Apply(CommandLineApplication target);
    }
}