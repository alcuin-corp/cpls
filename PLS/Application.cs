using System;
using System.Data;
using System.IO;
using System.Reflection;
using System.Text;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;

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
                builder.Apply(cmdLine);
            }
            cmdLine.Execute(args);
        }

        public static void AddCommandBuilders(this IServiceCollection services)
        {
            services.AddScoped<ConfigCommandBuilder>();
            services.AddScoped<AddTenantCommandBuilder>();
            services.AddScoped<AddServerCommandBuilder>();
            services.AddScoped<ListServerCommandBuilder>();

            services.AddScoped(provider => new ICommandBuilder[]
            {
                //new GroupCommandBuilder("server",
                //    provider.GetRequiredService<AddServerCommandBuilder>(),
                //    provider.GetRequiredService<ListServerCommandBuilder>()
                //),
                //new GroupCommandBuilder("tenant", provider.GetRequiredService<AddTenantCommandBuilder>()),
                provider.GetRequiredService<ConfigCommandBuilder>(),
            });
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
                var rootPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var dbPath = Path.Combine(rootPath, "pls.db");
                builder.UseSqlite($"Data Source={dbPath};");
            });
            var container = services.BuildServiceProvider();

            var db = container.GetRequiredService<PlsDbContext>();
            db.Database.EnsureCreated();
            return container;
        }
    }
}