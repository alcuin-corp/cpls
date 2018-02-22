using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using Dapper;
using Microsoft.EntityFrameworkCore.Internal;
using Optional.Collections;

namespace PLS
{
    public interface IServerService
    {
        IDbConnection OpenConnection();
        string BackupDirectory { get; }
        string DataDirectory { get; }

        void Backup(string database, string backupFile);
        void Restore(string backupFile, string database);
        bool IsDbMultiUser(string database);
        void SwitchMultiUserMode(string database);
        void SwitchToSingleUserMode(string database);

        /// <summary>
        /// List of (LogicalName, PhysicalName)
        /// </summary>
        IEnumerable<(string, string)> ReadFilelistFromBackup(string backupFilename);

        IEnumerable<(string, string)> RelocateLogicalFiles(string newName, IEnumerable<(string, string)> fileList);
        string CreateMoveStatement(string dbName, string backupFilename);
        void DropDatabase(string database);
        IEnumerable<string> GetDatabaseNames();
    }

    public delegate IServerService ServerServiceFactory(Server server);

    public class ServerService : IServerService
    {
        private readonly Server _server;

        public ServerService(Server server)
        {
            _server = server;
        }

        public IDbConnection OpenConnection()
        {
            return new SqlConnection($"Server={_server.Hostname};User Id={_server.Login};Password={_server.Password};");
        }

        public string BackupDirectory => Path.Combine(_server.InstallPath, "Backup");
        public string DataDirectory => Path.Combine(_server.InstallPath, "DATA");

        public void Backup(string database, string backupFile)
        {
            using (var conn = OpenConnection())
            {
                conn.Execute(
                    $"BACKUP DATABASE [{database}] " +
                    $"TO DISK = N'{backupFile}' " +
                    $"WITH NOFORMAT, INIT, SKIP, NOREWIND;");
            }
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
                    $@"RESTORE DATABASE [{database}]
                       FROM DISK = N'{backupFile}'
                       WITH FILE = 1,
                       {CreateMoveStatement(database, backupFile)},
                       NOUNLOAD, REPLACE, RECOVERY, STATS = 25;";
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

        /// <summary>
        /// List of (LogicalName, PhysicalName)
        /// </summary>
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