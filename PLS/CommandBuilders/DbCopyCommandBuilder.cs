using Microsoft.Extensions.CommandLineUtils;

namespace PLS.CommandBuilders
{
    public class DbCopyCommandBuilder : ICommandBuilder
    {
        private readonly PlsDbContext _db;
        private readonly ServerTasksFactory _st;

        public DbCopyCommandBuilder(PlsDbContext db, ServerTasksFactory st)
        {
            _db = db;
            _st = st;
        }

        public string Name => "db-copy";

        public void Configure(CommandLineApplication command)
        {
            command.AddHelp();

            var fromArg = command.Argument("source", "the source server id");
            var toArg = command.Argument("target", "the target server id");
            var dbListArg = command.Argument("database", "the databases we want to copy to the target", true);

            var maybeBackupOnly = command.Option("--backup-only", "do not restore the databases after fetching backups",
                CommandOptionType.NoValue);

            command.OnExecute(() =>
            {
                var src = _st(_db.Servers.Find(fromArg.Value));
                var target = _st(_db.Servers.Find(toArg.Value));
                if (maybeBackupOnly.HasValue())
                {
                    foreach (var value in dbListArg.Values)
                    {
                        target.FetchBackup(src, value);
                    }
                }
                else
                {
                    target.Copy(src, dbListArg.Values.ToArray());
                }
                return 0;
            });
        }
    }
}