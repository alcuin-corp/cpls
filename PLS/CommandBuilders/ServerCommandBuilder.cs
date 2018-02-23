using System;
using Microsoft.Extensions.CommandLineUtils;

namespace PLS.CommandBuilders
{
    public class ServerListCommandBuilder : ICommandBuilder
    {
        private readonly PlsDbContext _db;
        private readonly ServerTasksFactory _s;

        public ServerListCommandBuilder(
            PlsDbContext db, ServerTasksFactory s)
        {
            _db = db;
            _s = s;
        }
        public string Name => "server-list";

        public void Configure(CommandLineApplication command)
        {
            command.AddHelp();

            var maybeAll = command.Option("-a|--all", "List all the reachable servers", CommandOptionType.NoValue);

            command.OnExecute(() =>
            {
                foreach (var server in _db.Servers)
                {
                    Console.WriteLine(server.Id);
                    if (!maybeAll.HasValue()) continue;

                    Console.WriteLine($"\tHostname: {server.Hostname}");
                    Console.WriteLine($"\tBackupDirectory: {_s(server).BackupDirectory}");
                    Console.WriteLine($"\tDataDirectory: {_s(server).DataDirectory}");
                    Console.WriteLine($"\tSharedBackupDirectory: {_s(server).SharedBackupDirectory}");
                }
                return 0;
            });
        }
    }
}