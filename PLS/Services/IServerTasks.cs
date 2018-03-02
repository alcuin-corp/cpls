using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using PLS.Dtos;

namespace PLS.Services
{
    public interface IServerTasks
    {
        IDbConnection OpenConnection();
        string BackupDirectory { get; }
        string DataDirectory { get; }
        string SharedBackupDirectory { get; }
        Server Server { get; }

        Task<int> BackupAsync(string database, string backupFile, IDbConnection conn = null);
        void Restore(string backupFile, string database);
        bool IsDbMultiUser(string database);
        void SwitchMultiUserMode(string database);
        void SwitchToSingleUserMode(string database, Action<IDbConnection> action = null);

        /// <summary>
        /// List of (LogicalName, PhysicalName)
        /// </summary>
        IEnumerable<(string, string)> ReadFilelistFromBackup(string backupFilename);

        IEnumerable<(string, string)> RelocateLogicalFiles(string newName, IEnumerable<(string, string)> fileList);
        string CreateMoveStatement(string dbName, string backupFilename);
        void DropDatabase(string database);
        IEnumerable<string> GetDatabaseNames();
        Task<string> FetchBackupAsync(IServerTasks from, string db);
        Task CopyAsync(IServerTasks from, params string[] dbs);
        void Drop(string database);
    }
}