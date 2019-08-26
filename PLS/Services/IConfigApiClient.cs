using System.Threading.Tasks;

namespace PLS.Services
{
    public interface IConfigApiClient
    {
        Task<string> GetToken();
        Task<string> GetConfig();
        Task<string> PostConfig(string filename);
        Task<string> GetInstanceInfo();
    }
}