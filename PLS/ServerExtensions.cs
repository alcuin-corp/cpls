using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using Dapper;

namespace PLS
{
    public static class TenantExtensions
    {
        public static void SetAppName(this Tenant tenant, string newName)
        {
            using (var conn = tenant.Server.OpenConnection())
            {
                conn.Execute($"UPDATE [{tenant.ConfigDb}].dbo.Application SET name='{newName}'");
            }
        }
    }

    public static class ServerExtensions
    {
        public static IDbConnection OpenConnection(this Server self)
        {
            return new SqlConnection($"Server={self.Hostname};User Id={self.Login};Password={self.Password};");
        }

        public static void Restore(this Server self, string backupFile)
        {
            if (!File.Exists(backupFile))
            {
                throw new Exception("Can't restore not existing file.");
            }
        }

        public static void DropDatabase(this Server self, string database)
        {
            using (var con = self.OpenConnection())
            {
                con.Execute($"DROP DATABASE [{database}]");
            }
        }

        public static IEnumerable<string> GetDatabaseNames(this Server self)
        {
            using (var con = self.OpenConnection())
            {
                var rows = con.Query("SELECT name as Name " +
                                     "FROM sys.databases " +
                                     "WHERE name NOT IN ('master', 'tempdb', 'model', 'msdb') " +
                                     "AND state != 6 " +
                                     "ORDER BY name");
                foreach (var row in rows)
                {
                    yield return row.Name;
                }
            }
        }
    }
}