using System.Collections.Generic;

namespace PLS.Agit
{
    public interface IAgitRepository
    {
        string Head { get; set; }
        bool IsReady { get; }
        void Tag(string name, string hash = null);
        IEnumerable<string> Tags { get; }
        void Commit(string content, string authorOpt = "", string messageOpt = "", string[] parents = null);
        void Branch(string name, string hash = null);
        void Initialize();
    }
}