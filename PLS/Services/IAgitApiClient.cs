using System.Threading.Tasks;

namespace PLS.Services
{
    public interface IAgitApiClient
    {
        Task<string> PostCommit(string filename, string author, string message, string branchName);
        Task<string> PostBranch(string branchName, string revision);
        Task<string> GetBranch(string branchName);
        Task<string> PostTag(string branchName, string revision);
        Task<string> Checkout(string revision);
        Task<string> Merge(string receiving, string incoming, string author, string message);
    }
}