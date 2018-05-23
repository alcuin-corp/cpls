using System;
using System.Linq;
using Microsoft.Extensions.CommandLineUtils;
using PLS.Utils;

namespace PLS.CommandBuilders.Dev
{
    public class DbListServerCommandBuilder : ICommandBuilder
    {
        private readonly PlsDbContext _db;

        public DbListServerCommandBuilder(PlsDbContext db)
        {
            _db = db;
        }

        public string Name => "db-list";

        public void Configure(CommandLineApplication command)
        {
            command.Description = "displays a list of all reachable databases in locally stored db server";

            var serverIdArg = command.Option("-i|--server-id", "limits the list to one or several specific server(s)", CommandOptionType.MultipleValue);
            
            command.OnExecute(() =>
            {
                var servers = (serverIdArg.HasValue()
                    ? serverIdArg.Values.Select(_ => _db.Servers.Find(_)).Where(_ => _ != null)
                    : _db.Servers).ToList();

                if (servers.Any())
                {
                    foreach (var server in servers)
                    {
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine($"{server.Id}@{server.Hostname}");
                        Console.ResetColor();

                        var dbnames = server.GetDatabaseNames().ToList();
                        if (dbnames.Any())
                        {
                            foreach (var dbname in dbnames)
                            {
                                Console.WriteLine(dbname);
                            }
                        }
                        else
                        {
                            Console.WriteLine("No db found.");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("No servers found.");
                }


                return 0;
            });
        }
    }
}