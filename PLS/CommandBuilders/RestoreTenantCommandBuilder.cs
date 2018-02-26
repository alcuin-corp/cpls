using System.IO;
using System.Linq;
using Microsoft.Extensions.CommandLineUtils;

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
            var maybeKeepAppName = target.Option("-k|--keep-appname", "Keep the original application's name", CommandOptionType.NoValue);
            var maybeConfigBackup =
                target.Option("-c|--config-backup", "Force using the given backup as config backup", CommandOptionType.SingleValue);
            var maybePublicBackup =
                target.Option("-p|--public-backup", "Force using the given backup as public backup", CommandOptionType.SingleValue);

            target.OnExecute(() =>
            {
                var tenant = _db.Tenants.Find(nameArg.Value);
                _db.Entry(tenant).Reference(_ => _.Server).Load();
                var hserver = _s(tenant.Server);
                hserver.Restore(maybeConfigBackup.Values.FirstOrDefault() ?? Path.Combine(hserver.BackupDirectory, tenant.ConfigDb + ".bak"),
                    tenant.ConfigDb);
                hserver.Restore(maybePublicBackup.Values.FirstOrDefault() ?? Path.Combine(hserver.BackupDirectory, tenant.PublicDb + ".bak"),
                    tenant.PublicDb);
                if (!maybeKeepAppName.HasValue())
                {
                    _t(tenant).AppName = nameArg.Value;
                }
                return 0;
            });
        }
    }
}