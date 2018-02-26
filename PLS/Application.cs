using System;
using System.IO;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Omu.ValueInjecter;
using PLS.CommandBuilders;

namespace PLS
{
    public static class Application
    {
        public static void Start(params string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            var container = BuildContainer();
            var app = container.GetRequiredService<CommandLineApplication>();
            app.Execute(args);
        }

        public static string GetOrCreateConfigFolderPath()
        {
            var homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var configPath = Path.Combine(homePath, ".pls");
            if (!Directory.Exists(configPath))
            {
                Console.WriteLine("Create config folder into your home directory.");
                Directory.CreateDirectory(configPath);
            }
            return configPath;
        }

        public static IServiceProvider BuildContainer()
        {
            var services = new ServiceCollection();

            services.AddSingleton<ApiClientFactory>(ApiClient.Factory);

            services.AddScoped<TenantTasksFactory>(provider => tenant => new TenantTasks(tenant,
                provider.GetRequiredService<PlsDbContext>(),
                provider.GetRequiredService<ServerTasksFactory>()));
            services.AddScoped<ServerTasksFactory>(provider => server => new ServerTasks(server));

            services.AddCommandBuilders();

            services.AddDbContext<PlsDbContext>(builder =>
            {
                var dbPath = Path.Combine(GetOrCreateConfigFolderPath(), "pls.db");
                builder.UseSqlite($"Data Source={dbPath};");
            });

            services.Configure<AlcuinOptions>(Configuration);

            services.AddScoped(provider =>
            {
                var cmd = new CommandLineApplication { Name = "PLS" };
                cmd.HelpOption("-h|--help");
                cmd.Description = "Powerfull Lannister CLI";

                provider.Apply<ConfigCommandBuilder>(cmd);
                provider.Apply<DbListServerCommandBuilder>(cmd);
                provider.Apply<DbCopyCommandBuilder>(cmd);
                provider.Apply<DbRestoreCommandBuilder>(cmd);
                provider.Apply<ServerListCommandBuilder>(cmd);
                provider.Apply<TenantListCommandBuilder>(cmd);
                provider.Apply<AddTenantCommandBuilder>(cmd);
                provider.Apply<RestoreTenantCommandBuilder>(cmd);
                provider.Apply<AddServerCommandBuilder>(cmd);
                provider.Apply<MigrateTenantCommandBuilder>(cmd);
                provider.Apply<BackupTenantCommandBuilder>(cmd);
                provider.Apply<TenantCopyCommandBuilder>(cmd);

                return cmd;
            });

            var container = services.BuildServiceProvider();

            var db = container.GetRequiredService<PlsDbContext>();
            db.Database.EnsureCreated();
            return container;
        }

        private static void Configuration(AlcuinOptions obj)
        {
            var jsonSettingsPath = Path.Combine(GetOrCreateConfigFolderPath(), "settings.json");
            if (!File.Exists(jsonSettingsPath))
            {
                var options = JsonConvert.SerializeObject(new AlcuinOptions
                {
                    AlcuinRootPath = @"C:\dev\alcuin\alcuin",
                    MsbuildExe = @"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe",
                    BackupDirectory = @"C:\Program Files\Microsoft SQL Server\MSSQL14.MSSQLSERVER\MSSQL\Backup",
                    ConfigMigrationDll = @"C:\dev\alcuin\alcuin\Migration\output\Alcuin.Migration.Configuration.dll",
                    PublicMigrationDll = @"C:\dev\alcuin\alcuin\Migration\output\Alcuin.Migration.Application.dll",
                    MigratorExe = @"C:\dev\alcuin\alcuin\Tools\Migrator\Alcuin.Migration.Migrator.Console.exe",
                }, Formatting.Indented);
                File.WriteAllText(jsonSettingsPath, options);
            }
            var jsonSettings = JsonConvert.DeserializeObject<AlcuinOptions>(File.ReadAllText(jsonSettingsPath));
            obj.InjectFrom(jsonSettings);
        }
    }
}