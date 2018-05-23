using Microsoft.Extensions.CommandLineUtils;
using PLS.Services;

namespace PLS.CommandBuilders.Dev
{
    public class CreateWebAppCommandBuilder : ICommandBuilder
    {
        private readonly PlsDbContext _db;
        private readonly TenantTasksFactory _t;

        public CreateWebAppCommandBuilder(PlsDbContext db, TenantTasksFactory t)
        {
            _db = db;
            _t = t;
        }

        public string Name => "create-webapp";
        public void Configure(CommandLineApplication command)
        {
            command.Description = "creates a IIS webapp for the given tenant id (overrides existing app if necessary)";
            var tenantNameArg = command.Argument("tenant", "the tenant for which we want to create a webapp");

            command.OnExecute(() =>
            {
                var tenant = _t(_db.Tenants.Find(tenantNameArg.Value));
                tenant.CreateAdminWebApp();
                tenant.CreatePublicWebApp();
                return 0;
            });
        }
    }
}