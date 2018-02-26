using Microsoft.Extensions.CommandLineUtils;

namespace PLS.CommandBuilders
{
    public class BackupTenantCommandBuilder : ICommandBuilder
    {
        private readonly PlsDbContext _db;
        private readonly ServerTasksFactory _s;

        public BackupTenantCommandBuilder(PlsDbContext db, ServerTasksFactory s)
        {
            _db = db;
            _s = s;
        }

        public string Name => "backup-tenant";

        public void Configure(CommandLineApplication target)
        {
            target.Description = "Backup the tenant's databases";

            var nameArg = target.Argument("name", "The tenant name");

            target.OnExecute(async () =>
            {

                var tenant = _db.Tenants.Find(nameArg.Value);
                _db.Entry(tenant).Reference(_ => _.Server).Load();
                var hserver = _s(tenant.Server);

                await hserver.BackupAsync(tenant.ConfigDb, tenant.ConfigDb + ".bak");
                await hserver.BackupAsync(tenant.PublicDb, tenant.PublicDb + ".bak");

                return 0;
            });
        }
    }
}