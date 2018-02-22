using Dapper;

namespace PLS
{
    public static class TenantExtensions
    {
        public static void SetAppName(this Tenant tenant, string newName)
        {
            using (var conn = tenant.Server.OpenConnection())
            {
                conn.Execute($"UPDATE [{tenant.ConfigDb}].dbo.Application SET name='{newName}'");
            }
        }
    }
}