using Microsoft.Extensions.CommandLineUtils;

namespace PLS
{
    public interface ICommandBuilder
    {
        void Apply(CommandLineApplication target);
    }
}