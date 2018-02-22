using System;
using System.Linq.Expressions;

namespace PLS.CommandBuilders
{
    public class ListServerCommandBuilder : ListCommandBuilder<Server>
    {
        public ListServerCommandBuilder(PlsDbContext db) : base(db)
        {
        }
        public override Expression<Func<Server, string>> NameSelector => _ => _.Id;
    }
}