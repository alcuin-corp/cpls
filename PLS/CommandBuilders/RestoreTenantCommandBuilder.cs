using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.CommandLineUtils;
using PLS.Services;

namespace PLS.CommandBuilders
{
    public class RestoreTenantCommandBuilder : ICommandBuilder
    {
        private readonly PlsDbContext _db;
        private readonly TenantTasksFactory _t;
        private readonly ServerTasksFactory _s;

        public string Name => "restore-tenant";

        public RestoreTenantCommandBuilder(PlsDbContext db, TenantTasksFactory t, ServerTasksFactory s)
        {
            _db = db;
            _t = t;
            _s = s;
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
                var tenant = _db.Tenants.Find(nameArg.Value) ??
                    throw new Exception($"Tenant {nameArg.Value} does not exist, use add-tenant command to create a new tenant before restoring it.");

                _db.Entry(tenant).Reference(_ => _.Server).Load();
                var hserver = _s(tenant.Server);
                hserver.Restore(maybeConfigBackup.Values.FirstOrDefault() ?? Path.Combine(hserver.BackupDirectory, tenant.ConfigDb + ".bak"), tenant.ConfigDb);
                hserver.Restore(maybePublicBackup.Values.FirstOrDefault() ?? Path.Combine(hserver.BackupDirectory, tenant.PublicDb + ".bak"), tenant.PublicDb);
                if (maybeAppName.HasValue())
                {
                    _t(tenant).AppName = maybeAppName.Value();
                }
                return 0;
            });
        }
    }
}