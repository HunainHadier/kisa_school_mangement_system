using System;
using System.Collections.ObjectModel;
using System.Configuration;
using MySql.Data.MySqlClient;
using System.Linq;
using KisaSchoolMangement.Models;

namespace KisaSchoolMangement.Services
{
    public class UserService
    {
        private readonly string _connectionString;

        public UserService()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["KisaSchoolDB"].ConnectionString;
        }

        // 🔹 Get all users (only owner can see all users)
        public ObservableCollection<UserModel> GetAllUsers(int currentUserId)
        {
            var users = new ObservableCollection<UserModel>();

            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = currentUserId == 1
                        ? @"SELECT u.*, r.name as role_name, 
                                  creator.username as created_by_name,
                                  u.created_at, u.updated_at
                           FROM users u
                           LEFT JOIN roles r ON u.role_id = r.id
                           LEFT JOIN users creator ON u.created_by = creator.id
                           WHERE u.id != 1 AND u.is_deleted = 0
                           ORDER BY u.id DESC"
                        : @"SELECT u.*, r.name as role_name, 
                                  creator.username as created_by_name,
                                  u.created_at, u.updated_at
                           FROM users u
                           LEFT JOIN roles r ON u.role_id = r.id
                           LEFT JOIN users creator ON u.created_by = creator.id
                           WHERE u.id != 1 AND u.is_active = 1 AND u.is_deleted = 0
                           ORDER BY u.id DESC";

                    using (var cmd = new MySqlCommand(query, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var user = new UserModel
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                Username = reader["username"].ToString(),
                                FullName = reader["full_name"].ToString(),
                                RoleId = Convert.ToInt32(reader["role_id"]),
                                IsActive = Convert.ToBoolean(reader["is_active"])
                            };

                            // Handle nullable fields
                            user.Email = reader["email"] != DBNull.Value ? reader["email"].ToString() : "";
                            user.Phone = reader["phone"] != DBNull.Value ? reader["phone"].ToString() : "";
                            user.RoleName = reader["role_name"] != DBNull.Value ? reader["role_name"].ToString() : "No Role";
                            user.CreatedAt = Convert.ToDateTime(reader["created_at"]).ToString("yyyy-MM-dd HH:mm");
                            user.UpdatedAt = Convert.ToDateTime(reader["updated_at"]).ToString("yyyy-MM-dd HH:mm");
                            user.CreatedByName = reader["created_by_name"] != DBNull.Value ? reader["created_by_name"].ToString() : "System";

                            // Add status property for UI
                            user.Status = user.IsActive ? "Active" : "Inactive";
                            user.StatusColor = user.IsActive ? "#4CAF50" : "#F44336";

                            users.Add(user);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error loading users: {ex.Message}");
            }

            return users;
        }

        // 🔹 Get all roles (Database ke hisab se)
        public ObservableCollection<RoleModel> GetAllRoles()
        {
            var roles = new ObservableCollection<RoleModel>();

            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    // Database structure ke hisab se query
                    string query = @"SELECT id, name, description, 
                                            created_at, updated_at
                                     FROM roles 
                                     ORDER BY id";

                    using (var cmd = new MySqlCommand(query, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var role = new RoleModel
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                Name = reader["name"].ToString()
                            };

                            // Handle nullable fields
                            role.Description = reader["description"] != DBNull.Value ? reader["description"].ToString() : "";
                            role.CreatedAt = Convert.ToDateTime(reader["created_at"]).ToString("yyyy-MM-dd HH:mm");

                            // Check if updated_at column exists
                            if (reader["updated_at"] != DBNull.Value)
                            {
                                role.UpdatedAt = Convert.ToDateTime(reader["updated_at"]).ToString("yyyy-MM-dd HH:mm");
                            }

                            roles.Add(role);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error loading roles: {ex.Message}");
            }

            return roles;
        }

        // 🔹 Add new user (Database ke hisab se)
        public bool AddUser(UserModel user, int createdBy)
        {
            try
            {
                // Check if username already exists
                if (UsernameExists(user.Username))
                {
                    ShowError("Username already exists. Please choose a different username.");
                    return false;
                }

                // Check if email already exists
                if (!string.IsNullOrEmpty(user.Email) && EmailExists(user.Email))
                {
                    ShowError("Email already exists. Please use a different email.");
                    return false;
                }

                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = @"INSERT INTO users 
                            (username, password_hash, full_name, email, phone, 
                             role_id, is_active, created_by, created_at, updated_at, is_deleted) 
                            VALUES 
                            (@Username, @PasswordHash, @FullName, @Email, @Phone, 
                             @RoleId, @IsActive, @CreatedBy, @CreatedAt, @UpdatedAt, 0)";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        // Hash password
                        string passwordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);

                        cmd.Parameters.AddWithValue("@Username", user.Username);
                        cmd.Parameters.AddWithValue("@PasswordHash", passwordHash);
                        cmd.Parameters.AddWithValue("@FullName", user.FullName);
                        cmd.Parameters.AddWithValue("@Email", string.IsNullOrEmpty(user.Email) ? DBNull.Value : (object)user.Email);
                        cmd.Parameters.AddWithValue("@Phone", string.IsNullOrEmpty(user.Phone) ? DBNull.Value : (object)user.Phone);
                        cmd.Parameters.AddWithValue("@RoleId", user.RoleId);
                        cmd.Parameters.AddWithValue("@IsActive", user.IsActive);
                        cmd.Parameters.AddWithValue("@CreatedBy", createdBy);
                        cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        cmd.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                        int result = cmd.ExecuteNonQuery();

                        if (result > 0)
                        {
                            ShowSuccess("User added successfully!");
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error adding user: {ex.Message}");
            }

            return false;
        }

        // 🔹 Update user (Database ke hisab se)
        public bool UpdateUser(UserModel user, int updatedBy)
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    string query = @"UPDATE users 
                            SET full_name = @FullName,
                                email = @Email,
                                phone = @Phone,
                                role_id = @RoleId,
                                is_active = @IsActive,
                                updated_at = @UpdatedAt
                            WHERE id = @Id AND is_deleted = 0";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", user.Id);
                        cmd.Parameters.AddWithValue("@FullName", user.FullName);
                        cmd.Parameters.AddWithValue("@Email", string.IsNullOrEmpty(user.Email) ? DBNull.Value : (object)user.Email);
                        cmd.Parameters.AddWithValue("@Phone", string.IsNullOrEmpty(user.Phone) ? DBNull.Value : (object)user.Phone);
                        cmd.Parameters.AddWithValue("@RoleId", user.RoleId);
                        cmd.Parameters.AddWithValue("@IsActive", user.IsActive);
                        cmd.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                        int result = cmd.ExecuteNonQuery();

                        if (result > 0)
                        {
                            ShowSuccess("User updated successfully!");
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error updating user: {ex.Message}");
            }

            return false;
        }

        // 🔹 Reset user password
        public bool ResetPassword(int userId, string newPassword, int resetBy)
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    string passwordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);

                    string query = @"UPDATE users 
                            SET password_hash = @PasswordHash,
                                updated_at = @UpdatedAt
                            WHERE id = @Id AND is_deleted = 0";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", userId);
                        cmd.Parameters.AddWithValue("@PasswordHash", passwordHash);
                        cmd.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                        int result = cmd.ExecuteNonQuery();

                        if (result > 0)
                        {
                            ShowSuccess("Password reset successfully!");
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error resetting password: {ex.Message}");
            }

            return false;
        }

        // 🔹 Delete user (soft delete)
        public bool DeleteUser(int userId, int deletedBy)
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    string query = @"UPDATE users 
                            SET is_deleted = 1,
                                is_active = 0,
                                updated_at = @UpdatedAt
                            WHERE id = @Id";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", userId);
                        cmd.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                        int result = cmd.ExecuteNonQuery();

                        if (result > 0)
                        {
                            ShowSuccess("User deleted successfully!");
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error deleting user: {ex.Message}");
            }

            return false;
        }

        // 🔹 Check if username exists
        private bool UsernameExists(string username)
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = "SELECT COUNT(*) FROM users WHERE username = @Username AND is_deleted = 0";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Username", username);
                        int count = Convert.ToInt32(cmd.ExecuteScalar());
                        return count > 0;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        // 🔹 Check if email exists
        private bool EmailExists(string email)
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = "SELECT COUNT(*) FROM users WHERE email = @Email AND is_deleted = 0";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Email", email);
                        int count = Convert.ToInt32(cmd.ExecuteScalar());
                        return count > 0;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        // 🔹 Get user by ID
        public UserModel GetUserById(int userId)
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = @"SELECT u.*, r.name as role_name 
                                    FROM users u
                                    LEFT JOIN roles r ON u.role_id = r.id
                                    WHERE u.id = @Id AND u.is_deleted = 0";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", userId);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var user = new UserModel
                                {
                                    Id = Convert.ToInt32(reader["id"]),
                                    Username = reader["username"].ToString(),
                                    FullName = reader["full_name"].ToString(),
                                    RoleId = Convert.ToInt32(reader["role_id"]),
                                    IsActive = Convert.ToBoolean(reader["is_active"])
                                };

                                // Handle nullable fields
                                user.Email = reader["email"] != DBNull.Value ? reader["email"].ToString() : "";
                                user.Phone = reader["phone"] != DBNull.Value ? reader["phone"].ToString() : "";
                                user.RoleName = reader["role_name"] != DBNull.Value ? reader["role_name"].ToString() : "No Role";
                                user.CreatedAt = Convert.ToDateTime(reader["created_at"]).ToString("yyyy-MM-dd HH:mm");

                                return user;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error getting user: {ex.Message}");
            }

            return null;
        }

        // 🔹 Get active users count
        public int GetActiveUsersCount()
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = "SELECT COUNT(*) FROM users WHERE is_active = 1 AND is_deleted = 0 AND id != 1";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        return Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
            }
            catch
            {
                return 0;
            }
        }

        // 🔹 Get users by role
        public ObservableCollection<UserModel> GetUsersByRole(int roleId)
        {
            var users = new ObservableCollection<UserModel>();

            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = @"SELECT u.*, r.name as role_name 
                                    FROM users u
                                    LEFT JOIN roles r ON u.role_id = r.id
                                    WHERE u.role_id = @RoleId AND u.is_deleted = 0
                                    ORDER BY u.full_name";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@RoleId", roleId);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var user = new UserModel
                                {
                                    Id = Convert.ToInt32(reader["id"]),
                                    Username = reader["username"].ToString(),
                                    FullName = reader["full_name"].ToString(),
                                    RoleId = Convert.ToInt32(reader["role_id"]),
                                    IsActive = Convert.ToBoolean(reader["is_active"])
                                };

                                user.Email = reader["email"] != DBNull.Value ? reader["email"].ToString() : "";
                                user.Phone = reader["phone"] != DBNull.Value ? reader["phone"].ToString() : "";
                                user.RoleName = reader["role_name"] != DBNull.Value ? reader["role_name"].ToString() : "No Role";

                                users.Add(user);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error loading users by role: {ex.Message}");
            }

            return users;
        }

        // 🔹 Get role by ID
        public RoleModel GetRoleById(int roleId)
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = @"SELECT * FROM roles WHERE id = @Id";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", roleId);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var role = new RoleModel
                                {
                                    Id = Convert.ToInt32(reader["id"]),
                                    Name = reader["name"].ToString()
                                };

                                role.Description = reader["description"] != DBNull.Value ? reader["description"].ToString() : "";
                                role.CreatedAt = Convert.ToDateTime(reader["created_at"]).ToString("yyyy-MM-dd");

                                return role;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error getting role: {ex.Message}");
            }

            return null;
        }

        private void ShowError(string message)
        {
            System.Windows.MessageBox.Show(message, "Error",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }

        private void ShowSuccess(string message)
        {
            System.Windows.MessageBox.Show(message, "Success",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }
    }
}