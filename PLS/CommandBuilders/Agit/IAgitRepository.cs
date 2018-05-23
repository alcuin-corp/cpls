using System.Collections.Generic;

namespace PLS.CommandBuilders.Agit
{
    public interface IAgitRepository
    {
        void Tag(string tagName);
        IEnumerable<string> Tags { get; }
        void Commit(string sourceFile, string authorOpt = "", string messageOpt = "");
        void Initialize();
        bool IsReady { get; }
    }
}