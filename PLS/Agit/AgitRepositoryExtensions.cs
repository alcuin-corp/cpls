using System.IO;
using Newtonsoft.Json.Linq;

namespace PLS.Agit
{
    public static class AgitRepositoryExtensions
    {
        public static void CommitJsonFile(this IAgitRepository self, string jsonFile, string authorOpt = "", string messageOpt = "", string[] parents = null)
        {
            var obj = JToken.Parse(File.ReadAllText(jsonFile));
            self.Commit(obj.ToString(), authorOpt, messageOpt, parents);
        }
    }
}