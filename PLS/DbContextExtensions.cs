using System;
using System.Linq.Expressions;
using Omu.ValueInjecter;

namespace PLS
{
    public static class DbContextExtensions
    {
        public static void Upsert<T, TKey>(this PlsDbContext db, T newItem, Expression<Func<T, TKey>> on)
            where T : class
        {
            if (newItem == null) throw new ArgumentNullException(nameof(newItem));
            if (@on == null) throw new ArgumentNullException(nameof(@on));
            var id = on.Compile()(newItem);
            T item;
            if ((item = db.Find<T>(id)) != null)
            {
                item.InjectFrom(newItem);
            }
            else
            {
                db.Add(newItem);
            }
        }
    }
}