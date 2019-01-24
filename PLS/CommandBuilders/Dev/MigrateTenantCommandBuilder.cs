using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Options;
using PLS.Dtos;

namespace PLS.CommandBuilders.Dev
{
    public class MigrateTenantCommandBuilder : ICommandBuilder
    {
        private readonly PlsDbContext _db;
        private readonly AlcuinOptions _options;

        public MigrateTenantCommandBuilder(PlsDbContext db, IOptions<AlcuinOptions> options)
        {
            _db = db;
            _options = options.Value;
        }

        private void PerformMigration(string mainDb, string secondDb, string dll, Server server)
        {
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    FileName = _options.MigratorExe,
                    Arguments = $" SqlServer2005Dialect \"" +
                                $"Database={mainDb}; Data Source={server.Hostname}; " +
                                $"User Id={server.Login};Password={server.Password};" +
                                $"\" {dll} -additionalConnString \"" +
                                $"Database={secondDb}; Data Source={server.Hostname}; " +
                                $"User Id={server.Login};Password={server.Password};\""
                }
            };
            Console.WriteLine($"Migration of {mainDb} on {server.Hostname}");
            Console.WriteLine($"Using {_options.MigratorExe}");
            proc.Start();
            while (!proc.StandardOutput.EndOfStream)
            {
                Console.WriteLine("\t | " + proc.StandardOutput.ReadLine());
            }
        }

        public string Name => "migrate-tenant";
        public void Configure(CommandLineApplication command)
        {
            command.Description = "migrates a tenant to the last compiled migrations";
            var tenantNameArg = command.Argument("name", "The tenant name");

            command.OnExecute(() =>
            {
                var tenant = _db.Tenants.Include(_ => _.Server).First(_ => _.Id == tenantNameArg.Value);
                PerformMigration(tenant.ConfigDb, tenant.PublicDb, _options.ConfigMigrationDll, tenant.Server);
                PerformMigration(tenant.PublicDb, tenant.ConfigDb, _options.PublicMigrationDll, tenant.Server);
                return 0;
            });

        }
    }
}