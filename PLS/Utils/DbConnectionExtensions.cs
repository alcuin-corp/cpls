using System.Data;
using Dapper;

namespace PLS.Utils
{
    public static class DbConnectionExtensions
    {
        public static int GetProcessId(this IDbConnection db)
        {
            return db.ExecuteScalar<int>($"SELECT @@SPID");
        }
    }
}