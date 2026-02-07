using System;
using System.Collections.ObjectModel;
using System.Configuration;
using MySql.Data.MySqlClient;
using KisaSchoolMangement.Models;

namespace KisaSchoolMangement.Services
{
    public class SectionService
    {
        private readonly string _connectionString;

        public SectionService()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["KisaSchoolDB"].ConnectionString;
        }

        public ObservableCollection<SectionModel> GetAllSections()
        {
            var sections = new ObservableCollection<SectionModel>();

            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = @"SELECT s.*, c.name as class_name 
                                    FROM sections s
                                    LEFT JOIN classes c ON s.class_id = c.id
                                    ORDER BY s.name";

                    using (var cmd = new MySqlCommand(query, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            sections.Add(new SectionModel
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                ClassId = Convert.ToInt32(reader["class_id"]),
                                Name = reader["name"].ToString(),
                                Capacity = reader["capacity"] != DBNull.Value ? Convert.ToInt32(reader["capacity"]) : 0,
                                CreatedAt = reader["created_at"]?.ToString(),
                                ClassName = reader["class_name"]?.ToString()
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error loading sections: {ex.Message}", "Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }

            return sections;
        }

        public bool AddSection(SectionModel sectionModel)
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = @"INSERT INTO sections (class_id, name, capacity, created_at)
                                     VALUES (@ClassId, @Name, @Capacity, @CreatedAt)";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ClassId", sectionModel.ClassId);
                        cmd.Parameters.AddWithValue("@Name", sectionModel.Name ?? "");
                        cmd.Parameters.AddWithValue("@Capacity", sectionModel.Capacity);
                        cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error adding section: {ex.Message}", "Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return false;
            }
        }

        public bool UpdateSection(SectionModel sectionModel)
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = @"UPDATE sections 
                                     SET class_id = @ClassId, name = @Name, capacity = @Capacity
                                     WHERE id = @Id";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", sectionModel.Id);
                        cmd.Parameters.AddWithValue("@ClassId", sectionModel.ClassId);
                        cmd.Parameters.AddWithValue("@Name", sectionModel.Name ?? "");
                        cmd.Parameters.AddWithValue("@Capacity", sectionModel.Capacity);

                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error updating section: {ex.Message}", "Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return false;
            }
        }

        public bool DeleteSection(int id)
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = "DELETE FROM sections WHERE id = @Id";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);
                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error deleting section: {ex.Message}", "Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return false;
            }
        }
    }
}