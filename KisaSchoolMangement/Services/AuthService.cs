using KisaSchoolMangement.Database;
using MySql.Data.MySqlClient;
using BCryptNet = BCrypt.Net.BCrypt;

namespace KisaSchoolMangement.Services
{
    public class AuthService
    {
        public class LoginResult
        {
            public bool Success { get; set; }
            public string Message { get; set; }
            public User User { get; set; }
        }

        public class User
        {
            public int Id { get; set; }
            public string Username { get; set; }
            public string FullName { get; set; }
            public string Email { get; set; }
            public string Role { get; set; }
            public int RoleId { get; set; }
            public bool IsActive { get; set; }
        }

        public LoginResult Authenticate(string username, string password)
        {
            try
            {
                using var conn = DbConnectionHelper.GetConnection();

                // Connection check
                if (conn.State != System.Data.ConnectionState.Open)
                {
                    return new LoginResult
                    {
                        Success = false,
                        Message = "Database connection failed!"
                    };
                }

                string query = @"
                    SELECT u.id, u.username, u.password_hash, u.full_name, u.email, 
                           u.is_active, r.name as role_name, r.id as role_id
                    FROM users u 
                    INNER JOIN roles r ON u.role_id = r.id 
                    WHERE u.username = @username AND u.is_active = 1";

                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@username", username);

                using var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    int passwordHashIndex = reader.GetOrdinal("password_hash");

                    if (reader.IsDBNull(passwordHashIndex))
                    {
                        return new LoginResult
                        {
                            Success = false,
                            Message = "Password not set for this user!"
                        };
                    }

                    string storedPasswordHash = reader.GetString(passwordHashIndex);

                    // BCrypt password verification
                    bool passwordValid = BCryptNet.Verify(password, storedPasswordHash);

                    System.Diagnostics.Debug.WriteLine($"Password verification: {passwordValid}");

                    if (passwordValid)
                    {
                        return new LoginResult
                        {
                            Success = true,
                            Message = "Login successful!",
                            User = new User
                            {
                                Id = reader.GetInt32("id"),
                                Username = reader.GetString("username"),
                                FullName = reader.IsDBNull(reader.GetOrdinal("full_name")) ? "" : reader.GetString("full_name"),
                                Email = reader.IsDBNull(reader.GetOrdinal("email")) ? "" : reader.GetString("email"),
                                Role = reader.GetString("role_name"),
                                RoleId = reader.GetInt32("role_id"),
                                IsActive = reader.GetBoolean("is_active")
                            }
                        };
                    }
                    else
                    {
                        return new LoginResult
                        {
                            Success = false,
                            Message = "Invalid password!"
                        };
                    }
                }
                else
                {
                    return new LoginResult
                    {
                        Success = false,
                        Message = "User not found or inactive!"
                    };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Authentication Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");

                return new LoginResult
                {
                    Success = false,
                    Message = $"Login failed: {ex.Message}"
                };
            }
        }

        public void CreateInitialUser()
        {
            try
            {
                using var conn = DbConnectionHelper.GetConnection();

                string checkQuery = "SELECT COUNT(*) FROM users WHERE username = 'owner'";
                using var checkCmd = new MySqlCommand(checkQuery, conn);
                var userCount = Convert.ToInt32(checkCmd.ExecuteScalar());

                if (userCount > 0)
                {
                    System.Diagnostics.Debug.WriteLine("Initial user already exists. Skipping creation.");
                    return;
                }

                string hashedPassword = BCryptNet.HashPassword("owner123");
                System.Diagnostics.Debug.WriteLine($"Created hash for owner: {hashedPassword}");

                string insertQuery = @"
                    INSERT INTO users (username, password_hash, full_name, email, role_id, is_active) 
                    VALUES (@username, @password, @fullname, @email, @roleId, 1)";

                using var cmd = new MySqlCommand(insertQuery, conn);
                cmd.Parameters.AddWithValue("@username", "owner");
                cmd.Parameters.AddWithValue("@password", hashedPassword);
                cmd.Parameters.AddWithValue("@fullname", "System Owner");
                cmd.Parameters.AddWithValue("@email", "owner@kisaschool.com");
                cmd.Parameters.AddWithValue("@roleId", 1);

                cmd.ExecuteNonQuery();

                System.Diagnostics.Debug.WriteLine("Initial user created successfully.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"User creation error: {ex.Message}");
            }
        }

        public bool UpdateUserPassword(string username, string newPassword)
        {
            try
            {
                using var conn = DbConnectionHelper.GetConnection();
                string hashedPassword = BCryptNet.HashPassword(newPassword);

                string query = "UPDATE users SET password_hash = @password WHERE username = @username";
                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@password", hashedPassword);
                cmd.Parameters.AddWithValue("@username", username);

                return cmd.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Password update error: {ex.Message}");
                return false;
            }
        }

        public bool TestConnection()
        {
            try
            {
                using var conn = DbConnectionHelper.GetConnection();
                return conn.State == System.Data.ConnectionState.Open;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database connection error: {ex.Message}");
                return false;
            }
        }
    }
}
