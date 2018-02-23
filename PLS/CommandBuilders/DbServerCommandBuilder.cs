using System;
using System.Linq;
using Microsoft.Extensions.CommandLineUtils;

namespace PLS.CommandBuilders
{
    public class DbListServerCommandBuilder : ICommandBuilder
    {
        private readonly PlsDbContext _db;
        private readonly ServerTasksFactory _s;

        public DbListServerCommandBuilder(PlsDbContext db, ServerTasksFactory s)
        {
            _db = db;
            _s = s;
        }

        public string Name => "db-list";

        public void Configure(CommandLineApplication command)
        {
            command.AddHelp();

            var serverIdArg = command.Option("-i|--server-id", "Select one or several specific server(s)", CommandOptionType.MultipleValue);
            
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

                        var dbnames = _s(server).GetDatabaseNames().ToList();
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