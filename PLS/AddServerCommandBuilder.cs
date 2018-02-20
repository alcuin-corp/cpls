using System;
using Microsoft.Extensions.CommandLineUtils;

namespace PLS
{
    public class AddServerCommandBuilder : ICommandBuilder
    {
        private readonly PlsDbContext _db;

        public AddServerCommandBuilder(PlsDbContext db)
        {
            _db = db;
        }

        public void Apply(CommandLineApplication self)
        {
            self.Command("add", command =>
            {
                command.AddHelp();

                var nameArg = command.Argument("[name]", "The server name");
                var hostnameArg = command.Argument("[hostname]", "The server hostname");
                var loginArg = command.Argument("[login]", "The server login");
                var passwordArg = command.Argument("[password]", "The server password");

                command.OnExecute(() =>
                {
                    Console.WriteLine($"Creation of server {nameArg.Value} in progress ...");
                    _db.Servers.Add(new Server
                    {
                        Id = nameArg.Value,
                        Hostname = hostnameArg.Value,
                        Login = loginArg.Value,
                        Password = passwordArg.Value
                    });
                    _db.SaveChanges();
                    Console.WriteLine($"Server {nameArg.Value} has been created.");
                    return 0;
                });
            });
        }
    }
}