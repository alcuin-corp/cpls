using System;
using System.IO;
using System.Text;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using PLS.CommandBuilders;

namespace PLS
{
    public static class Application
    {
        public static void Parse(params string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            var app = BuildContainer();
            var cmdLine = new CommandLineApplication {Name = "PLS"};
            cmdLine.AddHelp();
            var builders = app.GetRequiredService<ICommandBuilder[]>();
            foreach (var builder in builders)
            {
                cmdLine.Command(builder.Name, builder.Configure);
            }
            cmdLine.Execute(args);
        }

        public static void AddCommandBuilders(this IServiceCollection services)
        {
            services.AddScoped<TenantServiceFactory>(provider => tenant => new TenantService(tenant,
                provider.GetRequiredService<PlsDbContext>(),
                provider.GetRequiredService<ServerTasksFactory>()));
            services.AddScoped<ServerTasksFactory>(provider => server => new ServerTasks(server));

            services.AddScoped<ConfigCommandBuilder>();
            services.AddScoped<DbListServerCommandBuilder>();
            services.AddScoped<DbCopyCommandBuilder>();
            services.AddScoped<ServerListCommandBuilder>();
            services.AddScoped<TenantListCommandBuilder>();
            services.AddScoped<AddTenantCommandBuilder>();
            services.AddScoped<RestoreTenantCommandBuilder>();
            services.AddScoped<AddServerCommandBuilder>();

            services.AddScoped(provider => new ICommandBuilder[]
            {
                provider.GetRequiredService<ConfigCommandBuilder>(),
                provider.GetRequiredService<DbListServerCommandBuilder>(),
                provider.GetRequiredService<DbCopyCommandBuilder>(),
                provider.GetRequiredService<ServerListCommandBuilder>(),
                provider.GetRequiredService<TenantListCommandBuilder>(),
                provider.GetRequiredService<AddTenantCommandBuilder>(),
                provider.GetRequiredService<RestoreTenantCommandBuilder>(),
                provider.GetRequiredService<AddServerCommandBuilder>(),
            });
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

            services.AddCommandBuilders();
            services.AddScoped(provider =>
            {
                var builder = new MapperConfiguration(expr =>
                {
                    expr.CreateMap<Tenant, Tenant>();
                });
                return builder.CreateMapper();
            });
            services.AddDbContext<PlsDbContext>(builder =>
            {
                var dbPath = Path.Combine(GetOrCreateConfigFolderPath(), "pls.db");
                builder.UseSqlite($"Data Source={dbPath};");
            });
            var container = services.BuildServiceProvider();

            var db = container.GetRequiredService<PlsDbContext>();
            db.Database.EnsureCreated();
            return container;
        }
    }
}