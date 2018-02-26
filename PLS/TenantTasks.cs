using System;
using System.IO;
using Dapper;

namespace PLS
{
    public delegate ITenantTasks TenantTasksFactory(Tenant tenant);

    public interface ITenantTasks
    {
        string AppName { get; set; }
        void Restore(string configBackup, string publicBackup, string backupDirectory = null);
        Tenant Tenant { get; }
    }

    public class TenantTasks : ITenantTasks
    {
        private readonly IServerTasks _server;

        public Tenant Tenant { get; }

        public TenantTasks(Tenant tenant, PlsDbContext db, ServerTasksFactory serverEnhancer)
        {
            Tenant = tenant ?? throw new ArgumentNullException(nameof(tenant));
            var server = db.Servers.Find(tenant.ServerId);
            _server = serverEnhancer(server);
        }

        public string AppName
        {
            get
            {
                using (var conn = _server.OpenConnection())
                {
                    return conn.ExecuteScalar<string>($"SELECT name FROM [{Tenant.ConfigDb}].dbo.Application;");
                }
            }
            set
            {
                using (var conn = _server.OpenConnection())
                {
                    conn.Execute($"UPDATE [{Tenant.ConfigDb}].dbo.Application SET name='{value}'");
                }
            }
        }

        public void Restore(string configBackup, string publicBackup, string backupDirectory = null)
        {
            backupDirectory = backupDirectory ?? _server.BackupDirectory;
            _server.Restore(Path.Combine(backupDirectory, configBackup), Tenant.ConfigDb);
            _server.Restore(Path.Combine(backupDirectory, publicBackup), Tenant.PublicDb);
        }
    }
}