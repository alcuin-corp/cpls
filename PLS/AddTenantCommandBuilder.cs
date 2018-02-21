using System;
using AutoMapper;
using Microsoft.Extensions.CommandLineUtils;

namespace PLS
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

        public void Apply(CommandLineApplication self)
        {
            self.Command("add", command =>
            {
                command.AddHelp();

                var nameArg = command.Argument("[name]", "The tenant name");
                var serverIdArg = command.Argument("[server]", "The server hosting this tenant");
                var appNameArg = command.Argument("[application-name]", "The tenant application name");
                var publicDbArg = command.Argument("[public-db]", "The tenant's public db");
                var configDbArg = command.Argument("[config-db]", "The tenant's config db");

                command.OnExecute(() =>
                {
                    var server = _db.Servers.Find(serverIdArg.Value);
                    if (server == null)
                        throw new Exception($"Server '{serverIdArg.Value}' not found ...");

                    var newTenant = new Tenant
                    {
                        Id = nameArg.Value,
                        ServerId = serverIdArg.Value,
                        AppName = appNameArg.Value,
                        ConfigDb = configDbArg.Value,
                        PublicDb = publicDbArg.Value
                    };

                    var tenant = _db.Tenants.Find(newTenant.Id);
                    if (tenant != null)
                    {
                        Console.WriteLine(
                            $"Tenant {nameArg.Value} already exists, we update it with the provided values ...");
                        _mapper.Map(newTenant, tenant);
                    }
                    else
                    {
                        Console.WriteLine($"Creation of tenant {nameArg.Value} in progress ...");
                        _db.Tenants.Add(newTenant);
                    }

                    _db.SaveChanges();
                    Console.WriteLine("Tenant has been created/updated.");
                    return 0;
                });
            });
        }
    }
}