using System;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using PLS.CommandBuilders;

namespace PLS
{
    public static class ProviderExtensions
    {
        public static void Apply<T>(this IServiceProvider self, CommandLineApplication app) where T : class, ICommandBuilder
        {
            ICommandBuilder builder = new HelpDecoratorCommandBuilder(self.GetRequiredService<T>());
            app.Command(builder.Name, builder.Configure);
        }

        public static void AddCommandBuilders(this IServiceCollection services)
        {
            services.AddScoped<ConfigCommandBuilder>();
            services.AddScoped<DbListServerCommandBuilder>();
            services.AddScoped<DbCopyCommandBuilder>();
            services.AddScoped<DbRestoreCommandBuilder>();
            services.AddScoped<ServerListCommandBuilder>();
            services.AddScoped<TenantListCommandBuilder>();
            services.AddScoped<AddTenantCommandBuilder>();
            services.AddScoped<RestoreTenantCommandBuilder>();
            services.AddScoped<AddServerCommandBuilder>();
            services.AddScoped<MigrateTenantCommandBuilder>();
            services.AddScoped<BackupTenantCommandBuilder>();
            services.AddScoped<TenantCopyCommandBuilder>();
        }
    }
}