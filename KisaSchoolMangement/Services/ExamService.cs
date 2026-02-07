using System;
using System.Collections.ObjectModel;
using System.Configuration;
using MySql.Data.MySqlClient;
using KisaSchoolMangement.Models;

namespace KisaSchoolMangement.Services
{
    public class ExamService
    {
        private readonly string _connectionString;

        public ExamService()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["KisaSchoolDB"].ConnectionString;
        }

        public ObservableCollection<ExamModel> GetAllExams()
        {
            var exams = new ObservableCollection<ExamModel>();

            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = @"SELECT e.*, c.name as class_name, et.name as exam_type_name
                                    FROM exams e
                                    LEFT JOIN classes c ON e.class_id = c.id
                                    LEFT JOIN exam_types et ON e.exam_type_id = et.id
                                    WHERE e.is_active = 1
                                    ORDER BY e.start_date DESC";

                    using (var cmd = new MySqlCommand(query, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            exams.Add(new ExamModel
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                Name = reader["name"].ToString(),
                                ClassId = Convert.ToInt32(reader["class_id"]),
                                ExamTypeId = Convert.ToInt32(reader["exam_type_id"]),
                                StartDate = reader["start_date"].ToString(),
                                EndDate = reader["end_date"].ToString(),
                                AcademicYear = reader["academic_year"].ToString(),
                                IsActive = Convert.ToBoolean(reader["is_active"]),
                                CreatedAt = reader["created_at"].ToString(),
                                ClassName = reader["class_name"].ToString(),
                                ExamTypeName = reader["exam_type_name"].ToString()
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error loading exams: {ex.Message}", "Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }

            return exams;
        }

        public bool AddExam(ExamModel exam)
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = @"INSERT INTO exams (name, class_id, exam_type_id, start_date, end_date, academic_year, created_at)
                                     VALUES (@Name, @ClassId, @ExamTypeId, @StartDate, @EndDate, @AcademicYear, @CreatedAt)";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Name", exam.Name ?? "");
                        cmd.Parameters.AddWithValue("@ClassId", exam.ClassId);
                        cmd.Parameters.AddWithValue("@ExamTypeId", exam.ExamTypeId);
                        cmd.Parameters.AddWithValue("@StartDate", exam.StartDate ?? "");
                        cmd.Parameters.AddWithValue("@EndDate", exam.EndDate ?? "");
                        cmd.Parameters.AddWithValue("@AcademicYear", exam.AcademicYear ?? "");
                        cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error adding exam: {ex.Message}", "Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return false;
            }
        }
    }
}