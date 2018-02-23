using System;
using AutoMapper;
using Microsoft.Extensions.CommandLineUtils;

namespace PLS.CommandBuilders
{
    public class AddServerCommandBuilder : ICommandBuilder
    {
        private readonly PlsDbContext _db;
        private readonly IMapper _mapper;

        public AddServerCommandBuilder(PlsDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public string Name => "add-server";

        public void Configure(CommandLineApplication command)
        {
            command.AddHelp();

            var nameArg = command.Argument("[name]", "The server name");
            var hostnameArg = command.Argument("[hostname]", "The server hostname");
            var loginArg = command.Argument("[login]", "The server login");
            var passwordArg = command.Argument("[password]", "The server password");

            command.OnExecute(() =>
            {
                _db.Upsert(_mapper, nameArg.Value, new Server
                {
                    Id = nameArg.Value,
                    Hostname = hostnameArg.Value,
                    Login = loginArg.Value,
                    Password = passwordArg.Value,
                });
                return 0;
            });
        }
    }
}