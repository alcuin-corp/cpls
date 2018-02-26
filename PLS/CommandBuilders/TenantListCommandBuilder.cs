using System;
using Microsoft.Extensions.CommandLineUtils;

namespace PLS.CommandBuilders
{
    public class TenantListCommandBuilder : ICommandBuilder
    {
        private readonly PlsDbContext _db;

        public TenantListCommandBuilder(PlsDbContext db)
        {
            _db = db;
        }

        public string Name => "tenant-list";
        public void Configure(CommandLineApplication command)
        {
            command.Description = "displays a list of all tenants stored in local database";
            command.OnExecute(() =>
            {
                foreach (var tenant in _db.Tenants)
                {
                    Console.WriteLine(tenant.Id + " " + tenant.ServerId + " " + tenant.ConfigDb + " " + tenant.PublicDb);
                }
                return 0;
            });
        }
    }
}