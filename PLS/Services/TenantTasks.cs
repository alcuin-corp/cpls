using System;
using System.Data;
using System.IO;
using Microsoft.Extensions.Options;
using PLS.Dtos;
using PLS.Utils;

namespace PLS.Services
{
    public class TenantTasks : ITenantTasks
    {
        private readonly IOptions<AlcuinOptions> _opt;
        private readonly IIisService _iis;
        private readonly IConfigDatabaseService _cfgDb;
        private readonly Server _server;

        public Tenant Dto { get; }

        public TenantTasks(Tenant tenant, PlsDbContext db, IOptions<AlcuinOptions> opt, IIisService iis, ConfigDatabaseServiceFactory cfgDbFactory)
        {
            _opt = opt;
            _iis = iis;
            Dto = tenant ?? throw new ArgumentNullException(nameof(tenant));
            _server = db.Servers.Find(tenant.ServerId);
            _cfgDb = cfgDbFactory(_server.ConnectionString(), tenant.ConfigDb);
        }

        public void DropAdminWebApp()
        {
            _iis.DropApplication($"/{ApplicationName}_ADM");
        }

        public void DropPublicWebApp()
        {
            _iis.CreateApplication("Public", $"/{ApplicationName}", Path.Combine(_opt.Value.AlcuinRootPath, @"Web\Public\WebMvc\Alcuin.Public.Web"));
        }

        public void CreateAdminWebApp()
        {
            _iis.CreateApplication("Admin", $"/{ApplicationName}_ADM", Path.Combine(_opt.Value.AlcuinRootPath, @"Web\Admin\Web\Alcuin.Admin.Web"));
        }

        public void CreatePublicWebApp()
        {
            _iis.CreateApplication("Public", $"/{ApplicationName}", Path.Combine(_opt.Value.AlcuinRootPath, @"Web\Public\WebMvc\Alcuin.Public.Web"));
        }

        public string LastVersion => _cfgDb.LastVersion;

        public string ApplicationName
        {
            get => _cfgDb.ApplicationName;
            set => _cfgDb.ApplicationName = value;
        }

        public void Restore(string configBackup, string publicBackup, string backupDirectory = null)
        {
            backupDirectory = backupDirectory ?? _server.BackupDirectory();
            _server.RestoreDatabase(Path.Combine(backupDirectory, configBackup), Dto.ConfigDb);
            _server.RestoreDatabase(Path.Combine(backupDirectory, publicBackup), Dto.PublicDb);
        }
    }
}