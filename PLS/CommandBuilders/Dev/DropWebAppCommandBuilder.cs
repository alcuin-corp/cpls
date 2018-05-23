using Microsoft.Extensions.CommandLineUtils;
using PLS.Services;

namespace PLS.CommandBuilders.Dev
{
    public class DropWebAppCommandBuilder : ICommandBuilder
    {
        private readonly PlsDbContext _db;
        private readonly TenantTasksFactory _t;
        public DropWebAppCommandBuilder(PlsDbContext db, TenantTasksFactory t)
        {
            _db = db;
            _t = t;
        }
        public string Name => "drop-webapp";
        public void Configure(CommandLineApplication command)
        {
            command.Description = "drops a IIS webapp if found (use public name and it will try to remove the admin too)";

            var tenantNameArg =
                command.Argument("tenant", "the tenant for which we want to remove the web applications");

            command.OnExecute(() =>
            {
                var tenant = _t(_db.Tenants.Find(tenantNameArg.Value));
                tenant.DropAdminWebApp();
                tenant.DropPublicWebApp();
                return 0;
            });
        }
    }
}