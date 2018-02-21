using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.CommandLineUtils;

namespace PLS
{
    public class ListServerCommandBuilder : ICommandBuilder
    {
        private readonly PlsDbContext _db;

        public ListServerCommandBuilder(PlsDbContext db)
        {
            _db = db;
        }

        public void Apply(CommandLineApplication target)
        {
            target.Command("list", command =>
            {
                command.AddHelp();

                command.OnExecute(() =>
                {
                    foreach (var id in _db.Servers.Select(_ => _.Id).ToArray())
                    {
                        Console.WriteLine(id);
                    }
                    return 0;
                });
            });
        }
    }
}