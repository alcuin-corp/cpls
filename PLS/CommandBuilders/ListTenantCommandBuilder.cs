using System;
using System.Linq.Expressions;

namespace PLS.CommandBuilders
{
    public class ListTenantCommandBuilder : ListCommandBuilder<Tenant>
    {
        public ListTenantCommandBuilder(PlsDbContext db) : base(db)
        {
        }
        public override Expression<Func<Tenant, string>> NameSelector => _ => $"{_.Id}\t{_.ServerId}";
    }
}