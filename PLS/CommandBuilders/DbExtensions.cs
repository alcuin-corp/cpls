using System;
using AutoMapper;

namespace PLS.CommandBuilders
{
    public static class DbExtensions
    {
        public static void Upsert<T, T2>(this PlsDbContext db, IMapper mapper, T2 id, T newItem)
            where T : class
        {
            var item = db.Find<T>(id);
            if (item != null)
            {
                Console.WriteLine(
                    $"{typeof(T)} {id} already exists, we update it with the provided values ...");
                mapper.Map(newItem, item);
            }
            else
            {
                Console.WriteLine($"Creation of {typeof(T)} {id} in progress ...");
                db.Add(newItem);
            }

            db.SaveChanges();
            Console.WriteLine($"{typeof(T)} has been created/updated.");
        }
    }
}