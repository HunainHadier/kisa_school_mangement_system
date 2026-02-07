using System;
using System.Collections.ObjectModel;
using System.Configuration;
using MySql.Data.MySqlClient;
using KisaSchoolMangement.Models;

namespace KisaSchoolMangement.Services
{
    public class SubjectService
    {
        private readonly string _connectionString;

        public SubjectService()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["KisaSchoolDB"].ConnectionString;
        }

        public ObservableCollection<SubjectModel> GetAllSubjects()
        {
            var subjects = new ObservableCollection<SubjectModel>();

            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = "SELECT * FROM subjects ORDER BY name";

                    using (var cmd = new MySqlCommand(query, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            subjects.Add(new SubjectModel
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                Name = reader["name"].ToString(),
                                Code = reader["code"]?.ToString(),
                                Description = reader["description"]?.ToString()
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error loading subjects: {ex.Message}", "Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }

            return subjects;
        }
    }
}