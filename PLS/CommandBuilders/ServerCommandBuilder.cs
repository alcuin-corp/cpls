using System;
using System.Threading.Tasks;
using Microsoft.Extensions.CommandLineUtils;

namespace PLS.CommandBuilders
{
    public static class CliExt
    {
        public static void Use(this CommandLineApplication app, ICommandRunner runner)
        {
            app.OnExecute(() => runner.Run());
        }
    }

    public class ServerCommandRunner : ICommandRunner
    {
        private readonly bool _showList;
        private readonly bool _showAll;
        private readonly PlsDbContext _db;
        private readonly ServerServiceFactory _s;

        public ServerCommandRunner(bool showList, bool showAll, PlsDbContext db, ServerServiceFactory s)
        {
            _showList = showList;
            _showAll = showAll;
            _db = db;
            _s = s;
        }

        public Task<int> Run()
        {
            if (!_showList) return Task.FromResult(0);

            foreach (var server in _db.Servers)
            {
                Console.WriteLine(server.Id);
                if (!_showAll) continue;

                Console.WriteLine($"\tHostname: {server.Hostname}");
                Console.WriteLine($"\tBackupDirectory: {_s(server).BackupDirectory}");
                Console.WriteLine($"\tDataDirectory: {_s(server).DataDirectory}");
            }
            return Task.FromResult(0);
        }
    }

    public interface ICommandRunner
    {
        Task<int> Run();
    }

    public class ServerCommandBuilder : ICommandBuilder
    {
        private readonly AddServerCommandBuilder _add;
        private readonly DbServerCommandBuilder _dbBuilder;
        private readonly PlsDbContext _db;
        private readonly ServerServiceFactory _s;

        public ServerCommandBuilder(AddServerCommandBuilder add, DbServerCommandBuilder dbBuilder, PlsDbContext db, ServerServiceFactory s)
        {
            _add = add;
            _dbBuilder = dbBuilder;
            _db = db;
            _s = s;
        }
        public string Name => "server";

        public void Configure(CommandLineApplication command)
        {
            command.AddHelp();
            command.Command(_add.Name, _add.Configure);
            command.Command(_dbBuilder.Name, _dbBuilder.Configure);

            var maybeList = command.Option("-l|--list", "List the available servers", CommandOptionType.NoValue);
            var maybeAll = command.Option("-a|--all", "List all the reachable servers", CommandOptionType.NoValue);

            command.Use(new ServerCommandRunner(maybeList.HasValue(), maybeAll.HasValue(), _db, _s));
        }
    }
}