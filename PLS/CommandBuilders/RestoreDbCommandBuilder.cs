using Microsoft.Extensions.CommandLineUtils;
using PLS.Services;
using PLS.Utils;

namespace PLS.CommandBuilders
{
    public class RestoreDbCommandBuilder : ICommandBuilder
    {
        private readonly PlsDbContext _db;

        public RestoreDbCommandBuilder(PlsDbContext db)
        {
            _db = db;
        }

        public string Name => "restore-db";

        public void Configure(CommandLineApplication command)
        {
            command.Description = "restores a database with the given backup file";

            var serverIdArg = command.Argument("server", "the server");
            var dbArg = command.Argument("db", "the database");
            var backupArg = command.Argument("backup", "the backup file");

            command.OnExecute(() =>
            {
                var server = _db.Servers.Find(serverIdArg.Value);
                var db = dbArg.Value;
                var backup = backupArg.Value;

                server.RestoreDatabase(backup, db);

                return 0;
            });
        }
    }
}