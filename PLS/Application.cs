using System;
using System.IO;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Web.Administration;
using Newtonsoft.Json;
using Omu.ValueInjecter;
using PLS.CommandBuilders.Config;
using PLS.CommandBuilders.Dev;
using PLS.Services;
using PLS.Utils;

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

            services.AddSingleton(new ServerManager());
            services.AddScoped<IIisService, IisService>();

            services.AddSingleton(ConfigApiClient.Factory);
            services.AddSingleton(ConfigDatabaseService.Factory);

            services.AddScoped<TenantTasksFactory>(provider => tenant => new TenantTasks(tenant,
                provider.GetRequiredService<PlsDbContext>(),
                provider.GetRequiredService<IOptions<AlcuinOptions>>(),
                provider.GetRequiredService<IIisService>(),
                provider.GetRequiredService<ConfigDatabaseServiceFactory>()));

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

                cmd.Command("dev", devCmd =>
                {
                    devCmd.HelpOption("--help|-h");
                    devCmd.Description = "all dev team realted commands (mainly debug and maintenance commands)";
                    provider.Apply<DbListServerCommandBuilder>(devCmd);
                    provider.Apply<RestoreDbCommandBuilder>(devCmd);
                    provider.Apply<ServerListCommandBuilder>(devCmd);
                    provider.Apply<TenantListCommandBuilder>(devCmd);
                    provider.Apply<AddTenantCommandBuilder>(devCmd);
                    provider.Apply<RestoreTenantCommandBuilder>(devCmd);
                    provider.Apply<AddServerCommandBuilder>(devCmd);
                    provider.Apply<MigrateTenantCommandBuilder>(devCmd);
                    provider.Apply<BackupTenantCommandBuilder>(devCmd);
                    provider.Apply<CopyTenantCommandBuilder>(devCmd);
                    provider.Apply<DropTenantCommandBuilder>(devCmd);
                    provider.Apply<CreateWebAppCommandBuilder>(devCmd);
                    provider.Apply<DropWebAppCommandBuilder>(devCmd);
                    provider.Apply<RecyclePoolCommandBuilder>(devCmd);
                    provider.Apply<CopyDbCommandBuilder>(devCmd);
                });

                cmd.Command("config", configCmd =>
                {
                    configCmd.HelpOption("--help|-h");
                    configCmd.Description = "commands related to the configuration import/export";
                    provider.Apply<ImportConfigCommandBuilder>(configCmd);
                    provider.Apply<ExportConfigCommandBuilder>(configCmd);
                    provider.Apply<InstanceInfoCommandBuilder>(configCmd);
                });

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