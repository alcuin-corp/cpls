using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dapper;

namespace PLS.Utils
{
    public static class DbConnectionExtensions
    {
        public static int GetProcessId(this IDbConnection db)
        {
            return db.ExecuteScalar<int>($"SELECT @@SPID");
        }

        // This can be used to fetch information about a task completion.
        // I deactivated it because we don't need it for the moment.
        //
        //public Option<(string, string)> GetCompletion(int sessionId)
        //{
        //    using (var conn = OpenConnection())
        //    {
        //        var sql = "SELECT " + 
        //                    " r.percent_complete AS PercentComplete, " +
        //                    " r.total_elapsed_time AS TotalElapsedTime " +
        //                  "FROM sys.dm_exec_requests r " +
        //                  "WHERE command IN ('RESTORE DATABASE', 'BACKUP DATABASE') " +
        //                  $"AND r.session_id = {sessionId}";
        //        var result = conn.QuerySingleOrDefault(sql);
        //        if (result == null) return Option.None<(string, string)>();
        //        return Option.Some<(string, string)>((result.PercentComplete.ToString(), result.TotalElapsedTime.ToString()));
        //    }
        //}

        public static DatabaseTasks Use(this IDbConnection self, string database) => new DatabaseTasks(self, database);

        public static string BackupDirectory(this IDbConnection self)
        {
            var rows = self.Query("EXEC master.dbo.xp_instance_regread " +
                                  "N'HKEY_LOCAL_MACHINE', N'Software\\Microsoft\\MSSQLServer\\MSSQLServer',N'BackupDirectory'").ToList();
            return rows[0].Data;
        }

        public static string DataDirectory(this IDbConnection self)
        {
            return self.QuerySingle<string>("SELECT CONVERT(SYSNAME, SERVERPROPERTY('InstanceDefaultDataPath'))");
        }

        public static IEnumerable<string> GetDatabaseNames(this IDbConnection self)
        {
            var rows = self.Query("SELECT name as Name " +
                                    "FROM sys.databases " +
                                    "WHERE name NOT IN ('master', 'tempdb', 'model', 'msdb') " +
                                    "AND state != 6 " +
                                    "ORDER BY name");
            foreach (var row in rows)
            {
                yield return row.Name;
            }
        }

        /// <summary>
        /// (Logical, Physical)
        /// </summary>
        public static IEnumerable<(string, string)> ReadFilelistFromBackup(this IDbConnection self, string backupFilename)
        {
            var results = self.Query($"RESTORE FILELISTONLY " +
                                        $"FROM DISK = N'{backupFilename}'");
            foreach (var row in results)
            {
                yield return (row.LogicalName, row.PhysicalName);
            }
        }

        /// <summary>
        /// (Logical, Physical)
        /// </summary>
        public static IEnumerable<(string, string)> RelocateLogicalFiles(this IDbConnection self, string newName, IEnumerable<(string, string)> fileList)
        {
            foreach (var (logical, physical) in fileList)
            {
                var fileName = Path.GetFileName(physical);
                var ext = Path.GetExtension(fileName);
                yield return (logical, Path.Combine(self.DataDirectory(), newName + ext));
            }
        }

        public static async Task CopyDatabaseAsync(this IDbConnection self, IDbConnection from, string sharedBackupDirectory, string db)
        {
            var backup = await self.FetchBackupAsync(from, sharedBackupDirectory, db);
            self.Use(db).RestoreDatabase(backup);
        }

        public static async Task<string> FetchBackupAsync(this IDbConnection self, IDbConnection from, string sharedBackupDirectory, string db)
        {
            var backupName = DateTime.Now.PostfixBackup(db + ".bak");
            var fullPath = Path.Combine(from.BackupDirectory(), backupName);
            var remotePath = Path.Combine(sharedBackupDirectory, backupName);
            var localPath = Path.Combine(self.BackupDirectory(), backupName);
            await from.Use(db).BackupDatabaseAsync(fullPath);
            File.Copy(remotePath, localPath);
            return localPath;
        }
    }
}