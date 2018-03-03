using System.Data;

namespace PLS.Services
{
    public interface IConfigDatabaseService
    {
        IDbConnection OpenConnection();
        string LastVersion { get; }
        string ApplicationName { get; set; }
    }
}