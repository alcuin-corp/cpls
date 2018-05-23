using Microsoft.Extensions.CommandLineUtils;
using PLS.Dtos;
using PLS.Utils;

namespace PLS.CommandBuilders.Dev
{
    public class AddServerCommandBuilder : ICommandBuilder
    {
        private readonly PlsDbContext _db;

        public AddServerCommandBuilder(PlsDbContext db)
        {
            _db = db;
        }

        public string Name => "add-server";

        public void Configure(CommandLineApplication command)
        {
            command.Description = "add a server to the local database";

            var nameArg = command.Argument("[name]", "The server name");
            var hostnameArg = command.Argument("[hostname]", "The server hostname");
            var loginArg = command.Argument("[login]", "The server login");
            var passwordArg = command.Argument("[password]", "The server password");

            command.OnExecute(() =>
            {
                _db.Upsert(new Server
                {
                    Id = nameArg.Value,
                    Hostname = hostnameArg.Value,
                    Login = loginArg.Value,
                    Password = passwordArg.Value,
                }, _ => _.Id);
                _db.SaveChanges();
                return 0;
            });
        }
    }
}