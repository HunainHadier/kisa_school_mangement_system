using System;
using System.Collections.ObjectModel;
using System.Configuration;
using MySql.Data.MySqlClient;
using KisaSchoolMangement.Models;

namespace KisaSchoolMangement.Services
{
    public class StudentMarkService
    {
        private readonly string _connectionString;

        public StudentMarkService()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["KisaSchoolDB"].ConnectionString;
        }

        public ObservableCollection<StudentMarkModel> GetMarksByExam(int examId)
        {
            var marks = new ObservableCollection<StudentMarkModel>();

            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = @"SELECT sm.*, 
                                    s.first_name, s.last_name, s.admission_no,
                                    c.name as class_name,
                                    sub.name as subject_name,
                                    e.name as exam_name,
                                    es.max_marks, es.pass_marks
                                    FROM student_marks sm
                                    INNER JOIN students s ON sm.student_id = s.id
                                    INNER JOIN classes c ON s.class_id = c.id
                                    INNER JOIN subjects sub ON sm.subject_id = sub.id
                                    INNER JOIN exams e ON sm.exam_id = e.id
                                    INNER JOIN exam_subjects es ON sm.exam_id = es.exam_id AND sm.subject_id = es.subject_id
                                    WHERE sm.exam_id = @ExamId
                                    ORDER BY s.first_name, sub.name";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ExamId", examId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                marks.Add(new StudentMarkModel
                                {
                                    Id = Convert.ToInt32(reader["id"]),
                                    StudentId = Convert.ToInt32(reader["student_id"]),
                                    ExamId = Convert.ToInt32(reader["exam_id"]),
                                    SubjectId = Convert.ToInt32(reader["subject_id"]),
                                    MarksObtained = reader["marks_obtained"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["marks_obtained"]),
                                    Grade = reader["grade"]?.ToString(),
                                    Remarks = reader["remarks"]?.ToString(),
                                    StudentName = $"{reader["first_name"]} {reader["last_name"]}",
                                    StudentAdmissionNo = reader["admission_no"].ToString(),
                                    StudentClassName = reader["class_name"].ToString(),
                                    SubjectName = reader["subject_name"].ToString(),
                                    ExamName = reader["exam_name"].ToString(),
                                    MaxMarks = Convert.ToDecimal(reader["max_marks"]),
                                    PassMarks = Convert.ToDecimal(reader["pass_marks"])
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error loading student marks: {ex.Message}", "Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }

            return marks;
        }

        public bool SaveOrUpdateMarks(StudentMarkModel mark)
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    // Check if marks already exist
                    string checkQuery = "SELECT COUNT(*) FROM student_marks WHERE student_id = @StudentId AND exam_id = @ExamId AND subject_id = @SubjectId";
                    bool marksExist = false;

                    using (var checkCmd = new MySqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@StudentId", mark.StudentId);
                        checkCmd.Parameters.AddWithValue("@ExamId", mark.ExamId);
                        checkCmd.Parameters.AddWithValue("@SubjectId", mark.SubjectId);
                        marksExist = Convert.ToInt32(checkCmd.ExecuteScalar()) > 0;
                    }

                    string query;
                    if (marksExist)
                    {
                        query = @"UPDATE student_marks 
                                 SET marks_obtained = @MarksObtained, grade = @Grade, remarks = @Remarks, updated_at = @UpdatedAt
                                 WHERE student_id = @StudentId AND exam_id = @ExamId AND subject_id = @SubjectId";
                    }
                    else
                    {
                        query = @"INSERT INTO student_marks (student_id, exam_id, subject_id, marks_obtained, grade, remarks, created_at, updated_at)
                                 VALUES (@StudentId, @ExamId, @SubjectId, @MarksObtained, @Grade, @Remarks, @CreatedAt, @UpdatedAt)";
                    }

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@StudentId", mark.StudentId);
                        cmd.Parameters.AddWithValue("@ExamId", mark.ExamId);
                        cmd.Parameters.AddWithValue("@SubjectId", mark.SubjectId);
                        cmd.Parameters.AddWithValue("@MarksObtained", mark.MarksObtained);
                        cmd.Parameters.AddWithValue("@Grade", mark.Grade ?? "");
                        cmd.Parameters.AddWithValue("@Remarks", mark.Remarks ?? "");
                        cmd.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                        if (!marksExist)
                        {
                            cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        }

                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error saving marks: {ex.Message}", "Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return false;
            }
        }

        public bool SaveBulkMarks(ObservableCollection<StudentMarkModel> marks)
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            foreach (var mark in marks)
                            {
                                string query = @"INSERT INTO student_marks (student_id, exam_id, subject_id, marks_obtained, grade, remarks, created_at, updated_at)
                                       VALUES (@StudentId, @ExamId, @SubjectId, @MarksObtained, @Grade, @Remarks, @CreatedAt, @UpdatedAt)
                                       ON DUPLICATE KEY UPDATE 
                                       marks_obtained = VALUES(marks_obtained),
                                       grade = VALUES(grade),
                                       remarks = VALUES(remarks),
                                       updated_at = VALUES(updated_at)";

                                using (var cmd = new MySqlCommand(query, conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@StudentId", mark.StudentId);
                                    cmd.Parameters.AddWithValue("@ExamId", mark.ExamId);
                                    cmd.Parameters.AddWithValue("@SubjectId", mark.SubjectId);
                                    cmd.Parameters.AddWithValue("@MarksObtained", mark.MarksObtained);
                                    cmd.Parameters.AddWithValue("@Grade", mark.Grade ?? "");
                                    cmd.Parameters.AddWithValue("@Remarks", mark.Remarks ?? "");
                                    cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                                    cmd.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                                    cmd.ExecuteNonQuery();
                                }
                            }

                            transaction.Commit();
                            return true;
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error saving bulk marks: {ex.Message}", "Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return false;
            }
        }
    }
}