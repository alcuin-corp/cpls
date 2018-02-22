using System.IO;
using System.Linq;
using Microsoft.Extensions.CommandLineUtils;

namespace PLS.CommandBuilders
{
    public class MigrateTenantCommandBuilder : ICommandBuilder
    {
        public string Name => "migrate";
        public void Configure(CommandLineApplication command)
        {
            command.AddHelp();
            command.Description = "Migrates a tenant to the last compiled version";
            var nameArg = command.Argument("name", "The tenant name");
        }
    }

    public class RestoreTenantCommandBuilder : ICommandBuilder
    {
        private readonly PlsDbContext _db;
        private readonly TenantServiceFactory _tenantEnhancer;
        private readonly ServerServiceFactory _serverEnhancer;
        public string Name => "restore";

        public RestoreTenantCommandBuilder(PlsDbContext db, TenantServiceFactory tenantEnhancer, ServerServiceFactory serverEnhancer)
        {
            _db = db;
            _tenantEnhancer = tenantEnhancer;
            _serverEnhancer = serverEnhancer;
        }

        public void Configure(CommandLineApplication target)
        {
            target.AddHelp();
            target.Description = "Restores the tenant's database to a designated backup";

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
                var hserver = _serverEnhancer(tenant.Server);
                hserver.Restore(maybeConfigBackup.Values.FirstOrDefault() ?? Path.Combine(hserver.BackupDirectory, nameArg.Value + "_ADM.bak"),
                    tenant.ConfigDb);
                hserver.Restore(maybePublicBackup.Values.FirstOrDefault() ?? Path.Combine(hserver.BackupDirectory, nameArg.Value + ".bak"),
                    tenant.PublicDb);
                if (!maybeKeepAppName.HasValue())
                {
                    _tenantEnhancer(tenant).SetAppName(nameArg.Value);
                }
                return 0;
            });
        }
    }
}