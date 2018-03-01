using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.CommandLineUtils;
using PLS.Dtos;
using PLS.Services;
using PLS.Utils;

namespace PLS.CommandBuilders
{
    public class CreateWebAppCommandBuilder : ICommandBuilder
    {
        private readonly IIisService _iis;
        private readonly PlsDbContext _db;
        private readonly TenantTasksFactory _t;

        public CreateWebAppCommandBuilder(IIisService iis, PlsDbContext db, TenantTasksFactory t)
        {
            _iis = iis;
            _db = db;
            _t = t;
        }

        public string Name => "create-webapp";
        public void Configure(CommandLineApplication command)
        {
            command.Description = "creates a IIS webapp for the given tenant id (overrides existing app if necessary)";

            var tenantNameArg = command.Argument("tenant", "the tenant for which we want to create a webapp");

            var adminOption = command.Option("-a|--admin", "creates an admin instance in IIS only", CommandOptionType.NoValue);
            var publicOption = command.Option("-p|--public", "creates a public instance in IIS only", CommandOptionType.NoValue);

            command.OnExecute(() =>
            {
                var tenant = _t(_db.Tenants.Find(tenantNameArg.Value));
                var all = !adminOption.HasValue() && !publicOption.HasValue();
                if (adminOption.HasValue() || all)
                    tenant.CreateAdminWebApp();
                if (publicOption.HasValue() || all)
                    tenant.CreatePublicWebApp();
                return 0;
            });
        }
    }

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
            var maybeAppName = command.Option("-a|--set-appname", "set the tenant's appname", CommandOptionType.SingleValue);
            var maybeTenantId = command.Option("-i|--set-tenant-id", "set the tenant's local id", CommandOptionType.SingleValue);

            var maybeNoIisSetup = command.Option("-x|--no-iis-setup", "prevent apps creation in IIS (allow running without admin role)", CommandOptionType.SingleValue);

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