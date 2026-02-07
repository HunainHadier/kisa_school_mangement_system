using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using KisaSchoolMangement.Models;
using MySql.Data.MySqlClient;

namespace KisaSchoolMangement.Services
{
    public class RolePermissionService
    {
        private readonly string _connectionString;

        public RolePermissionService()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["KisaSchoolDB"].ConnectionString;
        }

        public ObservableCollection<RoleModel> GetAllRoles()
        {
            var roles = new ObservableCollection<RoleModel>();

            try
            {
                using var conn = new MySqlConnection(_connectionString);
                conn.Open();
                string query = "SELECT id, name, description, created_at, updated_at FROM roles ORDER BY name";

                using var cmd = new MySqlCommand(query, conn);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    roles.Add(new RoleModel
                    {
                        Id = Convert.ToInt32(reader["id"]),
                        Name = reader["name"].ToString(),
                        Description = reader["description"] != DBNull.Value ? reader["description"].ToString() : "",
                        CreatedAt = reader["created_at"] != DBNull.Value
                            ? Convert.ToDateTime(reader["created_at"]).ToString("yyyy-MM-dd HH:mm")
                            : "",
                        UpdatedAt = reader["updated_at"] != DBNull.Value
                            ? Convert.ToDateTime(reader["updated_at"]).ToString("yyyy-MM-dd HH:mm")
                            : ""
                    });
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error loading roles: {ex.Message}", "Error");
            }

            return roles;
        }

        public ObservableCollection<PermissionModel> GetAllPermissions()
        {
            var permissions = new ObservableCollection<PermissionModel>();

            try
            {
                using var conn = new MySqlConnection(_connectionString);
                conn.Open();
                string query = @"SELECT module, permission_key, name, description
                                 FROM permissions
                                 ORDER BY module, name";

                using var cmd = new MySqlCommand(query, conn);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    permissions.Add(new PermissionModel
                    {
                        Module = reader["module"].ToString(),
                        Key = reader["permission_key"].ToString(),
                        Name = reader["name"].ToString(),
                        Description = reader["description"] != DBNull.Value ? reader["description"].ToString() : "",
                        IsSelected = false
                    });
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error loading permissions: {ex.Message}", "Error");
            }

            return permissions;
        }

        public HashSet<string> GetPermissionKeysForRole(int roleId)
        {
            var keys = new HashSet<string>();

            try
            {
                using var conn = new MySqlConnection(_connectionString);
                conn.Open();
                string query = @"SELECT p.permission_key
                                 FROM role_permissions rp
                                 INNER JOIN permissions p ON rp.permission_id = p.id
                                 WHERE rp.role_id = @RoleId";

                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@RoleId", roleId);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    keys.Add(reader["permission_key"].ToString());
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error loading role permissions: {ex.Message}", "Error");
            }

            return keys;
        }

        public bool SaveRolePermissions(int roleId, IEnumerable<string> permissionKeys, int updatedBy)
        {
            try
            {
                using var conn = new MySqlConnection(_connectionString);
                conn.Open();

                using var tx = conn.BeginTransaction();

                using (var deleteCmd = new MySqlCommand("DELETE FROM role_permissions WHERE role_id = @RoleId", conn, tx))
                {
                    deleteCmd.Parameters.AddWithValue("@RoleId", roleId);
                    deleteCmd.ExecuteNonQuery();
                }

                foreach (var key in permissionKeys)
                {
                    string insertQuery = @"INSERT INTO role_permissions (role_id, permission_id, updated_by, updated_at)
                                           SELECT @RoleId, p.id, @UpdatedBy, @UpdatedAt
                                           FROM permissions p
                                           WHERE p.permission_key = @PermissionKey";

                    using var insertCmd = new MySqlCommand(insertQuery, conn, tx);
                    insertCmd.Parameters.AddWithValue("@RoleId", roleId);
                    insertCmd.Parameters.AddWithValue("@PermissionKey", key);
                    insertCmd.Parameters.AddWithValue("@UpdatedBy", updatedBy);
                    insertCmd.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    insertCmd.ExecuteNonQuery();
                }

                tx.Commit();
                return true;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error saving permissions: {ex.Message}", "Error");
                return false;
            }
        }

        public bool CreateRole(string name, string description, int createdBy)
        {
            try
            {
                using var conn = new MySqlConnection(_connectionString);
                conn.Open();

                string query = @"INSERT INTO roles (name, description, created_at, updated_at)
                                 VALUES (@Name, @Description, @CreatedAt, @UpdatedAt)";

                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Name", name);
                cmd.Parameters.AddWithValue("@Description", string.IsNullOrWhiteSpace(description) ? DBNull.Value : description);
                cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                cmd.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                return cmd.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error creating role: {ex.Message}", "Error");
                return false;
            }
        }

        public bool CreatePermission(string module, string key, string name, string description)
        {
            try
            {
                using var conn = new MySqlConnection(_connectionString);
                conn.Open();

                string query = @"INSERT INTO permissions (module, permission_key, name, description)
                                 VALUES (@Module, @Key, @Name, @Description)";

                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Module", module);
                cmd.Parameters.AddWithValue("@Key", key);
                cmd.Parameters.AddWithValue("@Name", name);
                cmd.Parameters.AddWithValue("@Description", string.IsNullOrWhiteSpace(description) ? DBNull.Value : description);

                return cmd.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error creating permission: {ex.Message}", "Error");
                return false;
            }
        }
    }
}
