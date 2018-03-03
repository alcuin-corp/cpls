using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.CommandLineUtils;
using PLS.Services;
using PLS.Utils;

namespace PLS.CommandBuilders
{
    public class RestoreTenantCommandBuilder : ICommandBuilder
    {
        private readonly PlsDbContext _db;
        private readonly TenantTasksFactory _t;

        public string Name => "restore-tenant";

        public RestoreTenantCommandBuilder(PlsDbContext db, TenantTasksFactory t)
        {
            _db = db;
            _t = t;
        }

        public void Configure(CommandLineApplication target)
        {
            target.Description = "restores the tenant's database to a designated backup";

            var nameArg = target.Argument("name", "The tenant name");
            var maybeAppName = target.Option("-a|--set-appname", "Set the appname to a specific value", CommandOptionType.SingleValue);
            var maybeConfigBackup = target.Option("-c|--config-backup", "Force using the given backup as config backup", CommandOptionType.SingleValue);
            var maybePublicBackup = target.Option("-p|--public-backup", "Force using the given backup as public backup", CommandOptionType.SingleValue);

            target.OnExecute(() =>
            {
                var tenant = _t(_db.Tenants.Find(nameArg.Value) ??
                    throw new Exception($"Tenant {nameArg.Value} does not exist, use add-tenant command to create a new tenant before restoring it."));

                _db.Entry(tenant).Reference(_ => _.Dto.Server).Load();
                var hserver = tenant.Dto.Server;
                hserver.RestoreDatabase(maybeConfigBackup.Values.FirstOrDefault() ?? Path.Combine(hserver.BackupDirectory(), tenant.Dto.ConfigDb + ".bak"), tenant.Dto.ConfigDb);
                hserver.RestoreDatabase(maybePublicBackup.Values.FirstOrDefault() ?? Path.Combine(hserver.BackupDirectory(), tenant.Dto.PublicDb + ".bak"), tenant.Dto.PublicDb);
                if (maybeAppName.HasValue())
                {
                    tenant.ApplicationName = maybeAppName.Value();
                }
                return 0;
            });
        }
    }
}