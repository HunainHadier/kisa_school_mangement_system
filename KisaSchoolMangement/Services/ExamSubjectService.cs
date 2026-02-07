using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using MySql.Data.MySqlClient;
using KisaSchoolMangement.Models;

namespace KisaSchoolMangement.Services
{
    public class ExamSubjectService
    {
        private readonly string _connectionString;

        public ExamSubjectService()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["KisaSchoolDB"].ConnectionString;
        }

        // ✅ YEH METHOD PEHLE SE THI - ISKO RAHNE DO
        public ObservableCollection<ExamSubjectModel> GetSubjectsByExam(int examId)
        {
            var subjects = new ObservableCollection<ExamSubjectModel>();

            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = @"SELECT es.*, s.name as subject_name, s.code as subject_code, e.name as exam_name
                                    FROM exam_subjects es
                                    INNER JOIN subjects s ON es.subject_id = s.id
                                    INNER JOIN exams e ON es.exam_id = e.id
                                    WHERE es.exam_id = @ExamId";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ExamId", examId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                subjects.Add(new ExamSubjectModel
                                {
                                    Id = Convert.ToInt32(reader["id"]),
                                    ExamId = Convert.ToInt32(reader["exam_id"]),
                                    SubjectId = Convert.ToInt32(reader["subject_id"]),
                                    MaxMarks = Convert.ToDecimal(reader["max_marks"]),
                                    PassMarks = Convert.ToDecimal(reader["pass_marks"]),
                                    SubjectName = reader["subject_name"].ToString(),
                                    SubjectCode = reader["subject_code"].ToString(),
                                    ExamName = reader["exam_name"].ToString()
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error loading exam subjects: {ex.Message}", "Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }

            return subjects;
        }

        // ✅ YEH EK HI GetAllSubjects() METHOD RAHEGI - DUPLICATE HATA DO
        public ObservableCollection<SubjectModel> GetAllSubjects()
        {
            var subjects = new ObservableCollection<SubjectModel>();

            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    // ✅ 'is_active' condition hata do kyunki column nahi hai
                    string query = "SELECT id, name, code, description FROM subjects"; // WHERE is_active = 1 hata diya

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

        // ✅ YEH NAYE METHODS ADD KAREIN
        public bool SaveExamSubjects(int examId, List<ExamSubjectModel> examSubjects)
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    // Pehle existing subjects delete karein
                    string deleteSql = "DELETE FROM exam_subjects WHERE exam_id = @ExamId";
                    using (var deleteCmd = new MySqlCommand(deleteSql, conn))
                    {
                        deleteCmd.Parameters.AddWithValue("@ExamId", examId);
                        deleteCmd.ExecuteNonQuery();
                    }

                    // Phir naye subjects add karein
                    foreach (var subject in examSubjects)
                    {
                        string insertSql = @"INSERT INTO exam_subjects 
                                           (exam_id, subject_id, max_marks, pass_marks) 
                                           VALUES (@ExamId, @SubjectId, @MaxMarks, @PassMarks)";

                        using (var insertCmd = new MySqlCommand(insertSql, conn))
                        {
                            insertCmd.Parameters.AddWithValue("@ExamId", examId);
                            insertCmd.Parameters.AddWithValue("@SubjectId", subject.SubjectId);
                            insertCmd.Parameters.AddWithValue("@MaxMarks", subject.MaxMarks);
                            insertCmd.Parameters.AddWithValue("@PassMarks", subject.PassMarks);
                            insertCmd.ExecuteNonQuery();
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error saving exam subjects: {ex.Message}", "Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return false;
            }
        }

        public bool RemoveExamSubject(int examId, int subjectId)
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    string sql = "DELETE FROM exam_subjects WHERE exam_id = @ExamId AND subject_id = @SubjectId";
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@ExamId", examId);
                        cmd.Parameters.AddWithValue("@SubjectId", subjectId);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error removing exam subject: {ex.Message}", "Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return false;
            }
        }
    }
}