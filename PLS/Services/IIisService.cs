using Microsoft.Web.Administration;
using Optional;

namespace PLS.Services
{
    public interface IIisService
    {
        void CreateApplication(string pool, string path, string physicalPath, Site site = null);
        void DropApplication(string name);
        Option<ApplicationPool> GetPool(string name);
        ApplicationPool GetPoolOrCreate(string pool);
    }
}