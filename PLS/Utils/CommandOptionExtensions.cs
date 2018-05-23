using LanguageExt;
using Microsoft.Extensions.CommandLineUtils;

namespace PLS.Utils
{
    public static class CommandOptionExtensions
    {
        public static Option<string> Some(this CommandOption opt) => !opt.HasValue() ? null : opt.Value();
    }
}