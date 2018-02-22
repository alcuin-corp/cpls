using AutoMapper;
using Microsoft.Extensions.CommandLineUtils;

namespace PLS.CommandBuilders
{
    public class AddTenantCommandBuilder : ICommandBuilder
    {
        private readonly PlsDbContext _db;
        private readonly IMapper _mapper;

        public AddTenantCommandBuilder(PlsDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public string Name => "add";

        public void Configure(CommandLineApplication command)
        {
            command.AddHelp();

            var nameArg = command.Argument("[name]", "The tenant name");
            var serverIdArg = command.Argument("[server]", "The server hosting this tenant");
            var publicDbArg = command.Argument("[public-db]", "The tenant's public db");
            var configDbArg = command.Argument("[config-db]", "The tenant's config db");

            command.OnExecute(() =>
            {
                _db.Upsert(_mapper, nameArg.Value, new Tenant
                {
                    Id = nameArg.Value,
                    ServerId = serverIdArg.Value,
                    ConfigDb = configDbArg.Value,
                    PublicDb = publicDbArg.Value
                });
                return 0;
            });
        }
    }
}