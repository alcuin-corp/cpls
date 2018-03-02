using Microsoft.Web.Administration;

namespace PLS.Services
{
    public interface IIisService
    {
        void CreateApplication(string pool, string path, string physicalPath, Site site = null);
        void DropApplication(string name);
    }
}