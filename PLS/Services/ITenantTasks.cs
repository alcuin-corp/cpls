using PLS.Dtos;

namespace PLS.Services
{
    public interface ITenantTasks
    {
        void Restore(string configBackup, string publicBackup, string backupDirectory = null);
        Tenant Dto { get; }
        string ApplicationName { get; set; }
        void CreateAdminWebApp();
        void DropAdminWebApp();
        void CreatePublicWebApp();
        void DropPublicWebApp();
        string LastVersion { get; }
    }
}