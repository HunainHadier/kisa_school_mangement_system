using MySql.Data.MySqlClient;
using System.Data;

namespace KisaSchoolMangement.Database
{
    public static class DbConnectionHelper
    {
        private static string connectionString = "server=localhost;database=school_mgmt;user=root;password=;";

        public static MySqlConnection GetConnection()
        {
            var conn = new MySqlConnection(connectionString);
            conn.Open();
            return conn;
        }
    }
}
