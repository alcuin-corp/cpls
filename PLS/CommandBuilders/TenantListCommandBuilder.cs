using System;
using System.Linq;
using Microsoft.Extensions.CommandLineUtils;
using PLS.Gui;
using PLS.Services;

namespace PLS.CommandBuilders
{
    public class TenantListCommandBuilder : ICommandBuilder
    {
        private readonly PlsDbContext _db;
        private readonly TenantTasksFactory _tt;

        public TenantListCommandBuilder(PlsDbContext db, TenantTasksFactory tt)
        {
            _db = db;
            _tt = tt;
        }

        public string Name => "tenant-list";
        public void Configure(CommandLineApplication command)
        {
            command.Description = "displays a list of all tenants stored in local database";

            var maybeAll = command.Option("-a|--all", "display all information", CommandOptionType.NoValue);

            command.OnExecute(() =>
            {
                if (maybeAll.HasValue())
                {
                    var table = Console.OpenStandardOutput().StartTable();
                    table.AddRow(new[] { "Id", "ServerId", "AppName", "Version", "ConfigDb", "PublicDb" });

                    var list = _db.Tenants.ToList().Select(t => new[]
                    {
                        t.Id,
                        t.ServerId,
                        _tt(t).AppName,
                        _tt(t).LastVersion,
                        t.ConfigDb,
                        t.PublicDb,
                    }).ToArray();

                    table.AddRows(list);
                    table.Write(true);
                }
                else
                {
                    foreach (var tenant in _db.Tenants)
                    {
                        Console.WriteLine(tenant.Id);
                    }
                }
                return 0;
            });
        }
    }
}