using Microsoft.Extensions.CommandLineUtils;
using PLS.Utils;

namespace PLS.CommandBuilders.Dev
{
    public class CopyDbCommandBuilder : ICommandBuilder
    {
        private readonly PlsDbContext _db;

        public CopyDbCommandBuilder(PlsDbContext db)
        {
            _db = db;
        }

        public string Name => "copy-db";

        public void Configure(CommandLineApplication command)
        {
            command.Description = "copies a database from a server to another server";

            var fromArg = command.Argument("source", "the source server id");
            var toArg = command.Argument("target", "the target server id");
            var dbListArg = command.Argument("database", "the databases we want to copy to the target", true);

            var maybeBackupOnly = command.Option("--backup-only", "do not restore the databases after fetching backups",
                CommandOptionType.NoValue);

            command.OnExecute(async () =>
            {
                var src = _db.Servers.Find(fromArg.Value);
                var target = _db.Servers.Find(toArg.Value);
                if (maybeBackupOnly.HasValue())
                {
                    foreach (var value in dbListArg.Values)
                    {
                        await target.FetchBackupAsync(src, value);
                    }
                }
                else
                {
                    foreach (var value in dbListArg.Values)
                    {
                        await target.CopyDatabaseAsync(src, value);
                    }
                }
                return 0;
            });
        }
    }
}