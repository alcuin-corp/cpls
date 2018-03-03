using Microsoft.Extensions.CommandLineUtils;
using PLS.Dtos;
using PLS.Services;
using PLS.Utils;

namespace PLS.CommandBuilders
{
    public class DropTenantCommandBuilder : ICommandBuilder
    {
        private readonly PlsDbContext _db;
        private readonly TenantTasksFactory _t;

        public DropTenantCommandBuilder(PlsDbContext db, TenantTasksFactory t)
        {
            _db = db;
            _t = t;
        }

        public string Name => "drop-tenant";
        public void Configure(CommandLineApplication command)
        {
            command.Description = "deletes a tenant (optionally with its databases and found backups)";

            var tenantNamesArg = command.Argument("tenant", "the tenant(s) id", true);
            var hardOption = command.Option("--hard", "deletes the related databases and webapps (use with caution)", CommandOptionType.NoValue);

            command.OnExecute(() =>
            {
                foreach (var tenantName in tenantNamesArg.Values)
                {
                    var tenant = _t(_db.Find<Tenant>(tenantName));
                    var server = _db.Find<Server>(tenant.Dto.ServerId);
                    if (hardOption.HasValue())
                    {
                        tenant.DropAdminWebApp();
                        tenant.DropPublicWebApp();
                        server.DropDatabase(tenant.Dto.ConfigDb);
                        server.DropDatabase(tenant.Dto.PublicDb);
                    }
                    _db.Tenants.Remove(tenant.Dto);
                    _db.SaveChanges();
                }

                return 0;
            });

        }
    }
}