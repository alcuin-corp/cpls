using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using Dapper;
using Microsoft.EntityFrameworkCore.Internal;

namespace PLS
{
    public static class ServerExtensions
    {
        public static IDbConnection OpenConnection(this Server self)
        {
            return new SqlConnection($"Server={self.Hostname};User Id={self.Login};Password={self.Password};");
        }

        public static void Backup(this Server self, string database, string backupFile)
        {
            using (var conn = self.OpenConnection())
            {
                conn.Execute(
                    $"BACKUP DATABASE [{database}] " +
                    $"TO DISK = N'{backupFile}' " +
                    $"WITH NOFORMAT, INIT, SKIP, NOREWIND;");
            }
        }

        public static void Restore(this Server self, string backupFile, string dataDirectory, string database)
        {
            if (!File.Exists(backupFile))
            {
                throw new Exception("Can't restore not existing file.");
            }

            self.SwitchToSingleUserMode(database);
            var conn = self.OpenConnection();

            try
            {
                var task =
                    $@"RESTORE DATABASE [{database}]
                       FROM DISK = N'{backupFile}'
                       WITH FILE = 1,
                       {self.CreateMoveStatement(database, dataDirectory, backupFile)},
                       NOUNLOAD, REPLACE, RECOVERY, STATS = 25;";
                conn.Execute(task);
            }
            finally
            {
                conn.Close();
                self.SwitchMultiUserMode(database);
            }
        }

        public static bool IsDbMultiUser(this Server self, string database)
        {
            using (var con = self.OpenConnection())
            {
                return con.Query(
                    " SELECT COUNT(*) FROM sys.databases " +
                    $"WHERE name = '{database}' " +
                    " AND state != 6 " +
                    " AND user_access_desc = 'MULTI_USER'").Any();
            }
        }

        public static void SwitchMultiUserMode(this Server self, string database)
        {
            if (self.IsDbMultiUser(database)) return;
            using (var conn = self.OpenConnection())
            {
                conn.Execute($"ALTER DATABASE [{database}] SET MULTI_USER;");
            }
        }

        public static void SwitchToSingleUserMode(this Server self, string database)
        {
            if (!self.IsDbMultiUser(database)) return;
            using (var conn = self.OpenConnection())
            {
                conn.Execute($"ALTER DATABASE [{database}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;");
            }
        }

        /// <summary>
        /// List of (LogicalName, PhysicalName)
        /// </summary>
        public static IEnumerable<(string, string)> ReadFilelistFromBackup(this Server self, string backupFilename)
        {
            using (var conn = self.OpenConnection())
            {
                var results = conn.Query(
                    $"RESTORE FILELISTONLY " +
                    $"FROM DISK = N'{backupFilename}'");
                foreach (var row in results)
                {
                    yield return (row.LogicalName, row.PhysicalName);
                }
            }
        }

        public static IEnumerable<(string, string)> RelocateLogicalFiles(this Server self, string newName,
            string dataDirectory,
            IEnumerable<(string, string)> fileList)
        {
            foreach (var (logical, physical) in fileList)
            {
                var fileName = Path.GetFileName(physical);
                var ext = Path.GetExtension(fileName);
                yield return (logical, Path.Combine(dataDirectory, newName + ext));
            }
        }

        public static string CreateMoveStatement(this Server self, string dbName, string dataDirectory, string backupFilename)
        {
            var fileList = self.ReadFilelistFromBackup(backupFilename);
            var pairs = self.RelocateLogicalFiles(dbName, dataDirectory, fileList);
            var moves = new List<string>();
            foreach (var (logical, physical) in pairs)
            {
                moves.Add($"MOVE N'{logical}' TO N'{physical}'");
            }
            return moves.Join(", ");
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