using System.Data;
using System.Data.SqlClient;
using Dapper;

namespace PLS.Services
{
    public class ConfigDatabaseService : IConfigDatabaseService
    {
        private readonly string _connectionString;
        private readonly string _database;

        public IDbConnection OpenConnection() => new SqlConnection(_connectionString);


        public static ConfigDatabaseServiceFactory Factory {get;} = (cnx, db) => new ConfigDatabaseService(cnx, db);

        public ConfigDatabaseService(string connectionString, string database)
        {
            _connectionString = connectionString;
            _database = database;
        }

        public string LastVersion
        {
            get
            {
                using (var conn = OpenConnection())
                {
                    return conn.ExecuteScalar<string>($"SELECT TOP 1 Version FROM [{_database}].dbo.Versions ORDER BY Date DESC;");
                }
            }
        }

        public string ApplicationName
        {
            get
            {
                using (var conn = OpenConnection())
                {
                    return conn.ExecuteScalar<string>($"SELECT name FROM [{_database}].dbo.Application;");
                }
            }
            set
            {
                using (var conn = OpenConnection())
                {
                    conn.Execute($"UPDATE [{_database}].dbo.Application SET name='{value}'");
                }
            }
        }
    }
}