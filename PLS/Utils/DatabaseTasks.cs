using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.EntityFrameworkCore.Internal;

namespace PLS.Utils
{
    public class DatabaseTasks
    {
        private readonly IDbConnection _cnx;
        private readonly string _db;

        public DatabaseTasks(IDbConnection cnx, string db)
        {
            _cnx = cnx;
            _db = db;
        }

        public void Drop()
        {
            SwitchToSingleUserMode();
            _cnx.Execute($"DROP DATABASE [{_db}]");
        }

        public void RestoreDatabase(string backupFile)
        {
            if (!File.Exists(backupFile))
            {
                throw new Exception($"Backup '{backupFile}' is missing.");
            }

            SwitchToSingleUserMode();
            try
            {
                var task =
                    $@"RESTORE DATABASE [{_db}] " +
                    $"FROM DISK = N'{backupFile}' " +
                    "WITH FILE = 1, " +
                    $"{CreateMoveStatement(backupFile)}, " +
                    "NOUNLOAD, REPLACE, RECOVERY, STATS = 25;";
                _cnx.Execute(task);
            }
            finally
            {
                _cnx.Close();
                SwitchToMultiUserMode();
            }
        }

        public string CreateMoveStatement(string backupFilename)
        {
            var fileList = _cnx.ReadFilelistFromBackup(backupFilename);
            var pairs = _cnx.RelocateLogicalFiles(_db, fileList);
            return pairs.Select(pair => $"MOVE N'{pair.Item1}' TO N'{pair.Item2}'").Join();
        }

        public bool Exists()
        {
            var names = _cnx.GetDatabaseNames();
            return names.Any(n => n == _db);
        }


        public void SwitchToSingleUserMode()
        {
            if (!Exists() || !IsDatabaseMultiUser()) return;
            _cnx.Execute($"ALTER DATABASE [{_db}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;");
        }

        public bool IsDatabaseMultiUser()
        {
            return _cnx.Query(
                " SELECT COUNT(*) FROM sys.databases " +
                $"WHERE name = '{_db}' " +
                " AND state != 6 " +
                " AND user_access_desc = 'MULTI_USER'").Any();
        }

        public async Task<int> BackupDatabaseAsync(string backupFile)
        {
            var query = $"BACKUP DATABASE [{_db}] " +
                        $"TO DISK = N'{backupFile}' " +
                        $"WITH NOFORMAT, INIT, SKIP, NOREWIND, NOUNLOAD, STATS = 10;";
            return await _cnx.ExecuteAsync(query, commandTimeout: 0);
        }


        public void SwitchToMultiUserMode()
        {
            if (!Exists() || IsDatabaseMultiUser()) return;
            _cnx.Execute($"ALTER DATABASE [{_db}] SET MULTI_USER;");
        }
    }
}