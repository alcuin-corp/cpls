﻿using System;
using System.IO;
using Dapper;
using Microsoft.Extensions.Options;
using PLS.Dtos;

namespace PLS.Services
{
    public class TenantTasks : ITenantTasks
    {
        private readonly IOptions<AlcuinOptions> _opt;
        private readonly IIisService _iis;
        private readonly IServerTasks _server;

        public Tenant Tenant { get; }

        public TenantTasks(Tenant tenant, PlsDbContext db, IOptions<AlcuinOptions> opt, ServerTasksFactory serverEnhancer, IIisService iis)
        {
            _opt = opt;
            _iis = iis;
            Tenant = tenant ?? throw new ArgumentNullException(nameof(tenant));
            var server = db.Servers.Find(tenant.ServerId);
            _server = serverEnhancer(server);
        }

        public void CreateAdminWebApp()
        {
            _iis.CreateApplication("Admin", $"/{AppName}_ADM", Path.Combine(_opt.Value.AlcuinRootPath, @"Web\Admin\Web\Alcuin.Admin.Web"));
        }
        public void CreatePublicWebApp()
        {
            _iis.CreateApplication("Public", $"/{AppName}", Path.Combine(_opt.Value.AlcuinRootPath, @"Web\Public\WebMvc\Alcuin.Public.Web"));
        }
        public string LastVersion
        {
            get
            {
                using (var conn = _server.OpenConnection())
                {
                    return conn.ExecuteScalar<string>($"SELECT TOP 1 Version FROM [{Tenant.ConfigDb}].dbo.Versions ORDER BY Date DESC;");
                }
            }
        }

        public string AppName
        {
            get
            {
                using (var conn = _server.OpenConnection())
                {
                    return conn.ExecuteScalar<string>($"SELECT name FROM [{Tenant.ConfigDb}].dbo.Application;");
                }
            }
            set
            {
                using (var conn = _server.OpenConnection())
                {
                    conn.Execute($"UPDATE [{Tenant.ConfigDb}].dbo.Application SET name='{value}'");
                }
            }
        }

        public void Restore(string configBackup, string publicBackup, string backupDirectory = null)
        {
            backupDirectory = backupDirectory ?? _server.BackupDirectory;
            _server.Restore(Path.Combine(backupDirectory, configBackup), Tenant.ConfigDb);
            _server.Restore(Path.Combine(backupDirectory, publicBackup), Tenant.PublicDb);
        }
    }
}