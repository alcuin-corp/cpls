namespace PLS.Services
{
    public delegate IConfigDatabaseService ConfigDatabaseServiceFactory(string connectionString, string database);
}