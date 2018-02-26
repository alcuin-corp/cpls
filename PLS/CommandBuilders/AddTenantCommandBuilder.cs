using Microsoft.Extensions.CommandLineUtils;

namespace PLS.CommandBuilders
{
    public class AddTenantCommandBuilder : ICommandBuilder
    {
        private readonly PlsDbContext _db;

        public AddTenantCommandBuilder(PlsDbContext db)
        {
            _db = db;
        }

        public string Name => "add-tenant";

        public void Configure(CommandLineApplication command)
        {
            var nameArg = command.Argument("[name]", "The tenant name");
            var serverIdArg = command.Argument("[server]", "The server hosting this tenant");
            var publicDbArg = command.Argument("[public-db]", "The tenant's public db");
            var configDbArg = command.Argument("[config-db]", "The tenant's config db");

            command.OnExecute(() =>
            {
                _db.Upsert(new Tenant
                {
                    Id = nameArg.Value,
                    ServerId = serverIdArg.Value,
                    ConfigDb = configDbArg.Value,
                    PublicDb = publicDbArg.Value
                }, _ => _.Id);
                _db.SaveChanges();
                return 0;
            });
        }
    }
}