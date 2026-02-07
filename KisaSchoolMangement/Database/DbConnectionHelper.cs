using MySql.Data.MySqlClient;
using System.Configuration;
using System.Data;

namespace KisaSchoolMangement.Database
{
    public static class DbConnectionHelper
    {
        private static readonly string connectionString =
            ConfigurationManager.ConnectionStrings["KisaSchoolDB"]?.ConnectionString
            ?? "server=localhost;database=school_mgmt;user=root;password=;";

        public static MySqlConnection GetConnection()
        {
            var conn = new MySqlConnection(connectionString);
            conn.Open();
            return conn;
        }
    }
}
