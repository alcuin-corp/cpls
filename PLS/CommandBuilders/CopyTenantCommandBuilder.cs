using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.CommandLineUtils;
using PLS.Dtos;
using PLS.Services;
using PLS.Utils;

namespace PLS.CommandBuilders
{
    public class CopyTenantCommandBuilder : ICommandBuilder
    {
        private readonly PlsDbContext _db;
        private readonly TenantTasksFactory _tn;

        public CopyTenantCommandBuilder(PlsDbContext db, TenantTasksFactory tn)
        {
            _db = db;
            _tn = tn;
        }

        public string Name => "copy-tenant";

        public void Configure(CommandLineApplication command)
        {
            command.Description = "copies a tenant from a server to another server";

            var serverNameArg = command.Argument("server", "the source server name");
            var configDbArg = command.Argument("config-db", "the target server's config db");
            var publicDbArg = command.Argument("public-db", "the target server's public db");

            var maybeTargetServer = command.Option("-s|--target-server-id", "copy to a defined server (default is 'localhost')", CommandOptionType.SingleValue);
            var maybeAppName = command.Option("-a|--set-appname", "set the tenant's appname", CommandOptionType.SingleValue);
            var maybeTenantId = command.Option("-i|--set-tenant-id", "set the tenant's local id", CommandOptionType.SingleValue);

            var maybeNoIisSetup = command.Option("-x|--no-iis-setup", "prevent apps creation in IIS (allow running without admin role)", CommandOptionType.SingleValue);

            command.OnExecute(async () =>
            {
                var target = maybeTargetServer.HasValue() ?
                    _db.Servers.Find(maybeTargetServer.Value()) :
                    _db.Servers.First(_ => _.Hostname == "localhost");
                var source = _db.Servers.Find(serverNameArg.Value);

                await Task.WhenAll(
                    target.CopyDatabaseAsync(source, configDbArg.Value),
                    target.CopyDatabaseAsync(source, publicDbArg.Value));

                var newTenant = _tn(new Tenant
                {
                    ConfigDb = configDbArg.Value,
                    PublicDb = publicDbArg.Value,
                    ServerId = target.Id,
                });

                var tenantName = maybeAppName.HasValue() ? maybeAppName.Value() : newTenant.ApplicationName;
                var tenantId = maybeTenantId.HasValue() ? maybeTenantId.Value() : tenantName;

                newTenant.ApplicationName = tenantName;
                newTenant.Dto.Id = tenantId;

                _db.Upsert(newTenant.Dto, _ => _.Id);
                _db.SaveChanges();

                if (!maybeNoIisSetup.HasValue())
                {
                    newTenant.CreateAdminWebApp();
                    newTenant.CreatePublicWebApp();
                }

                return 0;
            });
        }
    }
}