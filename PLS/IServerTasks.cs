using System.Collections.Generic;
using System.Data;

namespace PLS
{
    public interface IServerTasks
    {
        IDbConnection OpenConnection();
        string BackupDirectory { get; }
        string DataDirectory { get; }
        string SharedBackupDirectory { get; }

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
        string FetchBackup(IServerTasks from, string db);
        void Copy(IServerTasks from, params string[] dbs);
    }
}