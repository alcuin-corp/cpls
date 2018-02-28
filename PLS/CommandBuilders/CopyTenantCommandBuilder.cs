using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.CommandLineUtils;
using PLS.Dtos;
using PLS.Services;
using PLS.Utils;

namespace PLS.CommandBuilders
{
    public class CopyTenantCommandBuilder : ICommandBuilder
    {
        private readonly PlsDbContext _db;
        private readonly ServerTasksFactory _st;
        private readonly TenantTasksFactory _tn;

        public CopyTenantCommandBuilder(PlsDbContext db, ServerTasksFactory st, TenantTasksFactory tn)
        {
            _db = db;
            _st = st;
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
            var maybeAppName = command.Option("-n|--set-appname", "set the tenant's appname", CommandOptionType.SingleValue);
            var maybeTenantId = command.Option("-i|--set-tenant-id", "set the tenant's local id", CommandOptionType.SingleValue);

            command.OnExecute(async () =>
            {
                var target = _st(maybeTargetServer.HasValue() ?
                    _db.Servers.Find(maybeTargetServer.Value()) :
                    _db.Servers.First(_ => _.Hostname == "localhost"));
                var source = _st(_db.Servers.Find(serverNameArg.Value));

                await target.CopyAsync(source, configDbArg.Value, publicDbArg.Value);

                var newTenant = _tn(new Tenant
                {
                    ConfigDb = configDbArg.Value,
                    PublicDb = publicDbArg.Value,
                    ServerId = target.Server.Id,
                });

                var tenantName = maybeAppName.HasValue() ? maybeAppName.Value() : newTenant.AppName;
                var tenantId = maybeTenantId.HasValue() ? maybeTenantId.Value() : tenantName;

                newTenant.AppName = tenantName;
                newTenant.Tenant.Id = tenantId;

                _db.Upsert(newTenant.Tenant, _ => _.Id);
                _db.SaveChanges();

                return 0;
            });
        }
    }
}