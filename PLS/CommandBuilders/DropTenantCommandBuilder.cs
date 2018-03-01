using Microsoft.Extensions.CommandLineUtils;
using PLS.Dtos;
using PLS.Services;

namespace PLS.CommandBuilders
{
    public class DropTenantCommandBuilder : ICommandBuilder
    {
        private readonly PlsDbContext _db;
        private readonly ServerTasksFactory _st;
        private readonly TenantTasksFactory _tt;

        public DropTenantCommandBuilder(PlsDbContext db, ServerTasksFactory st, TenantTasksFactory tt)
        {
            _db = db;
            _st = st;
            _tt = tt;
        }

        public string Name => "drop-tenant";
        public void Configure(CommandLineApplication command)
        {
            command.Description = "deletes a tenant (optionally with its databases and found backups)";

            var tenantNamesArg = command.Argument("tenant", "the tenant(s) id", true);
            var withDbOption = command.Option("-d|--with-database", "deletes the related databases (use with caution)", CommandOptionType.NoValue);

            command.OnExecute(() =>
            {
                foreach (var tenantName in tenantNamesArg.Values)
                {
                    var tenant = _tt(_db.Find<Tenant>(tenantName));
                    var server = _st(_db.Find<Server>(tenant.Tenant.ServerId));
                    if (withDbOption.HasValue())
                    {
                        server.Drop(tenant.Tenant.ConfigDb);
                        server.Drop(tenant.Tenant.PublicDb);
                    }
                    _db.Tenants.Remove(tenant.Tenant);
                    _db.SaveChanges();
                }

                return 0;
            });

        }
    }
}