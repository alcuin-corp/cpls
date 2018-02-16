using System;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace PLS
{
    public static class Application
    {
        public static void AddCommandBuilders(this IServiceCollection services)
        {
            services.AddScoped<AddTenantCommandBuilder>();
            services.AddScoped<AddServerCommandBuilder>();
            services.AddScoped<ConfigCommandBuilder>();

            services.AddScoped(provider =>
            {
                var add = provider.GetRequiredService<AddTenantCommandBuilder>();
                return new TenantCommandBuilder(add);
            });

            services.AddScoped(provider =>
            {
                var add = provider.GetRequiredService<AddServerCommandBuilder>();
                return new ServerCommandBuilder(add);
            });

            services.AddScoped(provider => new ICommandBuilder[]
            {
                provider.GetRequiredService<ServerCommandBuilder>(),
                provider.GetRequiredService<TenantCommandBuilder>(),
                provider.GetRequiredService<ConfigCommandBuilder>(),
            });
        }

        public static IServiceProvider BuildContainer()
        {
            var services = new ServiceCollection();
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
                builder.UseSqlite("Data Source=.\\pls.db;");
            });
            var container = services.BuildServiceProvider();

            var db = container.GetRequiredService<PlsDbContext>();
            db.Database.EnsureCreated();
            return container;
        }
    }
}