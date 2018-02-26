﻿using Microsoft.Extensions.CommandLineUtils;

namespace PLS.CommandBuilders
{
    public class DbRestoreCommandBuilder : ICommandBuilder
    {
        private readonly PlsDbContext _db;
        private readonly ServerTasksFactory _st;

        public DbRestoreCommandBuilder(PlsDbContext db, ServerTasksFactory st)
        {
            _db = db;
            _st = st;
        }

        public string Name => "db-restore";

        public void Configure(CommandLineApplication command)
        {
            var serverIdArg = command.Argument("server", "the server");
            var dbArg = command.Argument("db", "the database");
            var backupArg = command.Argument("backup", "the backup file");

            command.OnExecute(() =>
            {
                var server = _st(_db.Servers.Find(serverIdArg.Value));
                var db = dbArg.Value;
                var backup = backupArg.Value;

                server.Restore(backup, db);

                return 0;
            });
        }
    }
}