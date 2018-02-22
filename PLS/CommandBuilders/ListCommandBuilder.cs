using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Extensions.CommandLineUtils;

namespace PLS.CommandBuilders
{
    public abstract class ListCommandBuilder<T> : ICommandBuilder
        where T : class
    {
        private readonly PlsDbContext _db;

        protected ListCommandBuilder(PlsDbContext db)
        {
            _db = db;
        }

        public abstract Expression<Func<T, string>> NameSelector { get; }

        public string Name => "list";

        public void Configure(CommandLineApplication command)
        {
            command.AddHelp();

            command.OnExecute(() =>
            {
                foreach (var id in _db.Set<T>().Select(NameSelector).ToArray())
                {
                    Console.WriteLine(id);
                }
                return 0;
            });
        }
    }
}