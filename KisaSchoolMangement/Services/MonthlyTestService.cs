using System;
using System.Collections.ObjectModel;
using System.Configuration;
using MySql.Data.MySqlClient;
using KisaSchoolMangement.Models;

namespace KisaSchoolMangement.Services
{
    public class MonthlyTestService
    {
        private readonly string _connectionString;

        public MonthlyTestService()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["KisaSchoolDB"].ConnectionString;
        }

        public ObservableCollection<MonthlyTestModel> GetMonthlyTestsByExam(int examId)
        {
            var tests = new ObservableCollection<MonthlyTestModel>();

            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = @"SELECT mt.*, s.name as subject_name, e.name as exam_name, 
                                           st.first_name, st.last_name
                                    FROM monthly_tests mt
                                    INNER JOIN subjects s ON mt.subject_id = s.id
                                    INNER JOIN exams e ON mt.exam_id = e.id
                                    INNER JOIN students st ON mt.student_id = st.id
                                    WHERE mt.exam_id = @ExamId
                                    ORDER BY mt.month, s.name";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ExamId", examId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                tests.Add(new MonthlyTestModel
                                {
                                    Id = Convert.ToInt32(reader["id"]),
                                    ExamId = Convert.ToInt32(reader["exam_id"]),
                                    SubjectId = Convert.ToInt32(reader["subject_id"]),
                                    Month = Convert.ToInt32(reader["month"]),
                                    MaxMarks = Convert.ToDecimal(reader["max_marks"]),
                                    ObtainedMarks = Convert.ToDecimal(reader["obtained_marks"]),
                                    TestDate = Convert.ToDateTime(reader["test_date"]),
                                    SubjectName = reader["subject_name"].ToString(),
                                    ExamName = reader["exam_name"].ToString(),
                                    StudentName = $"{reader["first_name"]} {reader["last_name"]}",
                                    MonthName = GetMonthName(Convert.ToInt32(reader["month"]))
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error loading monthly tests: {ex.Message}", "Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }

            return tests;
        }

        private string GetMonthName(int month)
        {
            return month switch
            {
                1 => "January",
                2 => "February",
                3 => "March",
                4 => "April",
                5 => "May",
                6 => "June",
                7 => "July",
                8 => "August",
                9 => "September",
                10 => "October",
                11 => "November",
                12 => "December",
                _ => "Unknown"
            };
        }

        public bool SaveMonthlyTest(MonthlyTestModel test)
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = @"INSERT INTO monthly_tests 
                                    (exam_id, subject_id, student_id, month, max_marks, obtained_marks, test_date)
                                    VALUES (@ExamId, @SubjectId, @StudentId, @Month, @MaxMarks, @ObtainedMarks, @TestDate)
                                    ON DUPLICATE KEY UPDATE 
                                    obtained_marks = @ObtainedMarks, test_date = @TestDate";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ExamId", test.ExamId);
                        cmd.Parameters.AddWithValue("@SubjectId", test.SubjectId);
                        cmd.Parameters.AddWithValue("@StudentId", test.StudentId);
                        cmd.Parameters.AddWithValue("@Month", test.Month);
                        cmd.Parameters.AddWithValue("@MaxMarks", test.MaxMarks);
                        cmd.Parameters.AddWithValue("@ObtainedMarks", test.ObtainedMarks);
                        cmd.Parameters.AddWithValue("@TestDate", test.TestDate);

                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error saving monthly test: {ex.Message}", "Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return false;
            }
        }
    }
}