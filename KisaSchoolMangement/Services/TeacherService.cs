using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using MySql.Data.MySqlClient;
using KisaSchoolMangement.Models;

namespace KisaSchoolMangement.Services
{
    public class TeacherService
    {
        private readonly string _connectionString;
        private readonly string _photoUploadPath;

        public TeacherService()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["KisaSchoolDB"].ConnectionString;
            _photoUploadPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot", "teacher-photos");

            // Create directory if it doesn't exist
            if (!Directory.Exists(_photoUploadPath))
            {
                Directory.CreateDirectory(_photoUploadPath);
            }
        }

        public ObservableCollection<TeacherModel> GetAllTeachers()
        {
            var teachers = new ObservableCollection<TeacherModel>();

            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = @"SELECT * FROM teachers ORDER BY full_name";

                    using (var cmd = new MySqlCommand(query, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            teachers.Add(new TeacherModel
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                UserId = reader["user_id"] != DBNull.Value ? Convert.ToInt32(reader["user_id"]) : 0,
                                FullName = reader["full_name"].ToString(),
                                Email = reader["email"]?.ToString(),
                                Phone = reader["phone"]?.ToString(),
                                HireDate = reader["hire_date"]?.ToString(),
                                IsActive = Convert.ToBoolean(reader["is_active"]),
                                PhotoPath = reader["photo_path"]?.ToString(),
                                Qualification = reader["qualification"]?.ToString(),
                                Address = reader["address"]?.ToString(),
                                CreatedAt = Convert.ToDateTime(reader["created_at"])
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error loading teachers: {ex.Message}", "Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }

            return teachers;
        }

        public bool AddTeacher(TeacherModel teacher)
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = @"INSERT INTO teachers 
                                    (user_id, full_name, email, phone, hire_date, is_active, photo_path, qualification, address, created_at)
                                    VALUES (@UserId, @FullName, @Email, @Phone, @HireDate, @IsActive, @PhotoPath, @Qualification, @Address, @CreatedAt)";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", teacher.UserId > 0 ? (object)teacher.UserId : DBNull.Value);
                        cmd.Parameters.AddWithValue("@FullName", teacher.FullName ?? "");
                        cmd.Parameters.AddWithValue("@Email", teacher.Email ?? "");
                        cmd.Parameters.AddWithValue("@Phone", teacher.Phone ?? "");
                        cmd.Parameters.AddWithValue("@HireDate", teacher.HireDate ?? "");
                        cmd.Parameters.AddWithValue("@IsActive", teacher.IsActive);
                        cmd.Parameters.AddWithValue("@PhotoPath", teacher.PhotoPath ?? "");
                        cmd.Parameters.AddWithValue("@Qualification", teacher.Qualification ?? "");
                        cmd.Parameters.AddWithValue("@Address", teacher.Address ?? "");
                        cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now);

                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error adding teacher: {ex.Message}", "Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return false;
            }
        }

        public bool UpdateTeacher(TeacherModel teacher)
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = @"UPDATE teachers SET 
                                    user_id = @UserId, 
                                    full_name = @FullName, 
                                    email = @Email, 
                                    phone = @Phone, 
                                    hire_date = @HireDate, 
                                    is_active = @IsActive, 
                                    photo_path = @PhotoPath, 
                                    qualification = @Qualification, 
                                    address = @Address
                                    WHERE id = @Id";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", teacher.Id);
                        cmd.Parameters.AddWithValue("@UserId", teacher.UserId > 0 ? (object)teacher.UserId : DBNull.Value);
                        cmd.Parameters.AddWithValue("@FullName", teacher.FullName ?? "");
                        cmd.Parameters.AddWithValue("@Email", teacher.Email ?? "");
                        cmd.Parameters.AddWithValue("@Phone", teacher.Phone ?? "");
                        cmd.Parameters.AddWithValue("@HireDate", teacher.HireDate ?? "");
                        cmd.Parameters.AddWithValue("@IsActive", teacher.IsActive);
                        cmd.Parameters.AddWithValue("@PhotoPath", teacher.PhotoPath ?? "");
                        cmd.Parameters.AddWithValue("@Qualification", teacher.Qualification ?? "");
                        cmd.Parameters.AddWithValue("@Address", teacher.Address ?? "");

                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error updating teacher: {ex.Message}", "Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return false;
            }
        }

        public bool DeleteTeacher(int teacherId)
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = "DELETE FROM teachers WHERE id = @Id";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", teacherId);
                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error deleting teacher: {ex.Message}", "Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return false;
            }
        }

        public string SaveTeacherPhoto(string sourceFilePath)
        {
            try
            {
                if (string.IsNullOrEmpty(sourceFilePath) || !File.Exists(sourceFilePath))
                    return null;

                string fileName = $"teacher_{Guid.NewGuid()}{Path.GetExtension(sourceFilePath)}";
                string destinationPath = Path.Combine(_photoUploadPath, fileName);

                File.Copy(sourceFilePath, destinationPath, true);
                return $"teacher-photos/{fileName}";
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error saving teacher photo: {ex.Message}", "Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return null;
            }
        }
    }
}