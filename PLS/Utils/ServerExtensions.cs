using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;
using PLS.Dtos;

namespace PLS.Utils
{
    public static class ServerExtensions
    {
        public static string ConnectionString(this Server self)
        {
            if (self == null) throw new ArgumentNullException(nameof(self));
            return $"Server={self.Hostname};User Id={self.Login};Password={self.Password};";
        }

        public static IDbConnection CreateConnection(this Server self)
        {
            return new SqlConnection(self.ConnectionString());
        }

        public static string SharedBackupDirectory(this Server self) => Path.Combine($"\\\\{self.Hostname}", "Backup");

        public static string BackupDirectory(this Server self) => self.CreateConnection().BackupDirectory();

        public static string DataDirectory(this Server self) => self.CreateConnection().DataDirectory();

        public static Task<int> BackupDatabaseAsync(this Server self, string database, string backupFile, IDbConnection conn = null)
            => (conn ?? self.CreateConnection()).Use(database).BackupDatabaseAsync(backupFile);

        public static void RestoreDatabase(this Server self, string backupFile, string database)
            => self.CreateConnection().Use(database).RestoreDatabase(database);

        public static Task<string> FetchBackupAsync(this Server self, Server from, string db)
            => self.CreateConnection().FetchBackupAsync(from.CreateConnection(), from.SharedBackupDirectory(), db);

        public static Task CopyDatabaseAsync(this Server self, Server from, string db)
            => self.CreateConnection().CopyDatabaseAsync(from.CreateConnection(), from.SharedBackupDirectory(), db);

        public static IEnumerable<string> GetDatabaseNames(this Server self)
            => self.CreateConnection().GetDatabaseNames();

        public static void DropDatabase(this Server self, string database)
            => self.CreateConnection().Use(database).Drop();
    }
}