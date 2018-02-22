using System.IO;
using Dapper;

namespace PLS
{
    public delegate ITenantService TenantServiceFactory(Tenant tenant);

    public interface ITenantService
    {
        void SetAppName(string newName);
        void Restore(string configBackup, string publicBackup, string backupDirectory = null);
    }

    public class TenantService : ITenantService
    {
        private readonly Tenant _tenant;
        private readonly IServerService _server;

        public TenantService(Tenant tenant, PlsDbContext db, ServerServiceFactory serverEnhancer)
        {
            _tenant = tenant;
            var server = db.Servers.Find(tenant.ServerId);
            _server = serverEnhancer(server);
        }

        public void SetAppName(string newName)
        {
            using (var conn = _server.OpenConnection())
            {
                conn.Execute($"UPDATE [{_tenant.ConfigDb}].dbo.Application SET name='{newName}'");
            }
        }

        public void Restore(string configBackup, string publicBackup, string backupDirectory = null)
        {
            backupDirectory = backupDirectory ?? _server.BackupDirectory;
            _server.Restore(Path.Combine(backupDirectory, configBackup), _tenant.ConfigDb);
            _server.Restore(Path.Combine(backupDirectory, publicBackup), _tenant.PublicDb);
        }
    }
}