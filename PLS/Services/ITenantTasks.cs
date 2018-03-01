using PLS.Dtos;

namespace PLS.Services
{
    public interface ITenantTasks
    {
        string AppName { get; set; }
        void Restore(string configBackup, string publicBackup, string backupDirectory = null);
        Tenant Tenant { get; }
        string LastVersion { get; }
        void CreateAdminWebApp();
        void CreatePublicWebApp();
    }
}