using Microsoft.Extensions.CommandLineUtils;
using PLS.Services;
using PLS.Utils;

namespace PLS.CommandBuilders
{
    public class BackupTenantCommandBuilder : ICommandBuilder
    {
        private readonly PlsDbContext _db;

        public BackupTenantCommandBuilder(PlsDbContext db)
        {
            _db = db;
        }

        public string Name => "backup-tenant";

        public void Configure(CommandLineApplication target)
        {
            target.Description = "create a backup of the selected tenant's databases";

            var nameArg = target.Argument("name", "The tenant name");

            target.OnExecute(async () =>
            {
                var tenant = _db.Tenants.Find(nameArg.Value);
                _db.Entry(tenant).Reference(_ => _.Server).Load();

                await tenant.Server.BackupDatabaseAsync(tenant.ConfigDb, tenant.ConfigDb + ".bak");
                await tenant.Server.BackupDatabaseAsync(tenant.PublicDb, tenant.PublicDb + ".bak");

                return 0;
            });
        }
    }
}