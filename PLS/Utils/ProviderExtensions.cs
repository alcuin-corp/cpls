using System;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using PLS.CommandBuilders;
using PLS.CommandBuilders.Agit;
using PLS.CommandBuilders.Config;
using PLS.CommandBuilders.Dev;

namespace PLS.Utils
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
            services.AddScoped<DbListServerCommandBuilder>();
            services.AddScoped<CopyDbCommandBuilder>();
            services.AddScoped<RestoreDbCommandBuilder>();
            services.AddScoped<ServerListCommandBuilder>();
            services.AddScoped<TenantListCommandBuilder>();
            services.AddScoped<AddTenantCommandBuilder>();
            services.AddScoped<RestoreTenantCommandBuilder>();
            services.AddScoped<AddServerCommandBuilder>();
            services.AddScoped<MigrateTenantCommandBuilder>();
            services.AddScoped<BackupTenantCommandBuilder>();
            services.AddScoped<CopyTenantCommandBuilder>();
            services.AddScoped<DropTenantCommandBuilder>();
            services.AddScoped<CreateWebAppCommandBuilder>();
            services.AddScoped<DropWebAppCommandBuilder>();
            services.AddScoped<RecyclePoolCommandBuilder>();
            services.AddScoped<FormatJsonBuilder>();
            services.AddScoped<ImportConfigCommandBuilder>();
            services.AddScoped<ExportConfigCommandBuilder>();
            services.AddScoped<CommitCommandBuilder>();
            services.AddScoped<InitRepositoryBuilder>();
            services.AddScoped<TagListCommandBuilder>();
            services.AddScoped<TagCommandBuilder>();
        }
    }
}