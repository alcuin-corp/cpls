using System;
using Microsoft.Extensions.CommandLineUtils;
using PLS.Services;
using PLS.Utils;

namespace PLS.CommandBuilders
{
    public class ServerListCommandBuilder : ICommandBuilder
    {
        private readonly PlsDbContext _db;

        public ServerListCommandBuilder(
            PlsDbContext db)
        {
            _db = db;
        }
        public string Name => "server-list";

        public void Configure(CommandLineApplication command)
        {
            command.Description = "displays a list of all servers stored in local database";

            var maybeAll = command.Option("-a|--all", "List all the reachable servers", CommandOptionType.NoValue);

            command.OnExecute(() =>
            {
                foreach (var server in _db.Servers)
                {
                    Console.WriteLine(server.Id);
                    if (!maybeAll.HasValue()) continue;

                    Console.WriteLine($"\tHostname: {server.Hostname}");
                    Console.WriteLine($"\tBackupDirectory: {server.BackupDirectory()}");
                    Console.WriteLine($"\tDataDirectory: {server.DataDirectory()}");
                    Console.WriteLine($"\tSharedBackupDirectory: {server.SharedBackupDirectory()}");
                }
                return 0;
            });
        }
    }
}