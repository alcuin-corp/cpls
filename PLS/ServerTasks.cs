using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.EntityFrameworkCore.Internal;

namespace PLS
{
    public class ServerTasks : IServerTasks
    {
        private readonly Server _server;

        public ServerTasks(Server server)
        {
            _server = server ?? throw new ArgumentNullException(nameof(server));
        }

        public IDbConnection OpenConnection()
        {
            return new SqlConnection($"Server={_server.Hostname};User Id={_server.Login};Password={_server.Password};");
        }

        public string SharedBackupDirectory
        {
            get
            {
                var path = Path.Combine($"\\\\{_server.Hostname}", "Backup");
                return !Directory.Exists(path) ? $"[KO] {path}" : path;
            }
        }

        public string BackupDirectory
        {
            get
            {
                using (var conn = OpenConnection())
                {
                    var rows = conn.Query("EXEC master.dbo.xp_instance_regread " +
                                          "N'HKEY_LOCAL_MACHINE', N'Software\\Microsoft\\MSSQLServer\\MSSQLServer',N'BackupDirectory'").ToList();
                    return rows[0].Data;
                }
            }
        }

        public string DataDirectory
        {
            get
            {
                using (var conn = OpenConnection())
                {
                    return conn.QuerySingle<string>("SELECT CONVERT(SYSNAME, SERVERPROPERTY('InstanceDefaultDataPath'))");
                }
            }
        }

        public static async Task<int> GetCompletion(int sessionId, IDbConnection connStatus)
        {
            var sql =  "SELECT CONVERT(NUMERIC(6,2), r.percent_complete) AS Completion, "+
                       "FROM sys.dm_exec_requests r "+
                       "WHERE command IN ('RESTORE DATABASE', 'BACKUP DATABASE') "+
                      $"AND r.session_id = {sessionId}";

            var r = await connStatus.QuerySingleAsync(sql);
            return r.Completion;
        }

        public void Backup(string database, string backupFile)
        {
            using (var conn = OpenConnection())
            {
                conn.ExecuteAsync(
                    $"BACKUP DATABASE [{database}] " +
                    $"TO DISK = N'{backupFile}' " +
                    $"WITH NOFORMAT, INIT, SKIP, NOREWIND;");
            }
        }

        public void Copy(IServerTasks from, params string[] dbs)
        {
            foreach (var db in dbs)
            {
                var backup = FetchBackup(from, db);
                Restore(backup, db);
            }
        }

        public string FetchBackup(IServerTasks from, string db)
        {
            var backupName = DateTime.Now.PostfixBackup(db + ".bak");
            var fullPath = Path.Combine(from.BackupDirectory, backupName);
            var remotePath = Path.Combine(from.SharedBackupDirectory, backupName);
            var localPath = Path.Combine(BackupDirectory, backupName);
            from.Backup(db, fullPath);
            File.Copy(remotePath, localPath);
            return localPath;
        }

        public void Restore(string backupFile, string database)
        {
            if (!File.Exists(backupFile))
            {
                throw new Exception("Can't restore not existing file.");
            }

            SwitchToSingleUserMode(database);
            var conn = OpenConnection();

            try
            {
                var task =
                    $@"RESTORE DATABASE [{database}] " +
                     $"FROM DISK = N'{backupFile}' " +
                      "WITH FILE = 1, " +
                     $"{CreateMoveStatement(database, backupFile)}, " +
                      "NOUNLOAD, REPLACE, RECOVERY, STATS = 25;";
                conn.Execute(task);
            }
            finally
            {
                conn.Close();
                SwitchMultiUserMode(database);
            }
        }

        public bool IsDbMultiUser(string database)
        {
            using (var con = OpenConnection())
            {
                return con.Query(
                    " SELECT COUNT(*) FROM sys.databases " +
                    $"WHERE name = '{database}' " +
                    " AND state != 6 " +
                    " AND user_access_desc = 'MULTI_USER'").Any();
            }
        }

        public void SwitchMultiUserMode(string database)
        {
            if (!DoesDbExist(database) || IsDbMultiUser(database)) return;
            using (var conn = OpenConnection())
            {
                conn.Execute($"ALTER DATABASE [{database}] SET MULTI_USER;");
            }
        }

        public void SwitchToSingleUserMode(string database)
        {
            if (!DoesDbExist(database) || !IsDbMultiUser(database)) return;
            using (var conn = OpenConnection())
            {
                conn.Execute($"ALTER DATABASE [{database}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;");
            }
        }

        /// <inheritdoc />
        public IEnumerable<(string, string)> ReadFilelistFromBackup(string backupFilename)
        {
            using (var conn = OpenConnection())
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

        public IEnumerable<(string, string)> RelocateLogicalFiles(string newName, IEnumerable<(string, string)> fileList)
        {
            foreach (var (logical, physical) in fileList)
            {
                var fileName = Path.GetFileName(physical);
                var ext = Path.GetExtension(fileName);
                yield return (logical, Path.Combine(DataDirectory, newName + ext));
            }
        }

        public string CreateMoveStatement(string dbName, string backupFilename)
        {
            var fileList = ReadFilelistFromBackup(backupFilename);
            var pairs = RelocateLogicalFiles(dbName, fileList);
            var moves = new List<string>();
            foreach (var (logical, physical) in pairs)
            {
                moves.Add($"MOVE N'{logical}' TO N'{physical}'");
            }
            return moves.Join(", ");
        }

        public void DropDatabase(string database)
        {
            using (var con = OpenConnection())
            {
                con.Execute($"DROP DATABASE [{database}]");
            }
        }

        public bool DoesDbExist(string dbName)
        {
            var names = GetDatabaseNames();
            return names.Any(n => n == dbName);
        }

        public IEnumerable<string> GetDatabaseNames()
        {
            using (var con = OpenConnection())
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