using KisaSchoolMangement.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Windows;

namespace KisaSchoolMangement.Services
{
    public class TeacherSubjectService
    {
        private string connectionString;

        public TeacherSubjectService()
        {
            connectionString = ConfigurationManager.ConnectionStrings["KisaSchoolDB"].ConnectionString;
        }

        public bool AssignSubjectToTeacher(TeacherSubjectAssignment assignment)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"INSERT INTO teacher_subjects 
                                   (teacher_id, subject_id, class_id, academic_year) 
                                   VALUES (@TeacherId, @SubjectId, @ClassId, @AcademicYear)";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@TeacherId", assignment.TeacherId);
                        command.Parameters.AddWithValue("@SubjectId", assignment.SubjectId);
                        command.Parameters.AddWithValue("@ClassId", assignment.ClassId);
                        command.Parameters.AddWithValue("@AcademicYear", assignment.AcademicYear);

                        return command.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (MySqlException ex) when (ex.Number == 1062) // Duplicate entry
            {
                throw new Exception("This subject is already assigned to the teacher for this class.");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error assigning subject: {ex.Message}");
            }
        }

        public ObservableCollection<TeacherSubjectAssignment> GetTeacherAssignments(int teacherId)
        {
            var assignments = new ObservableCollection<TeacherSubjectAssignment>();

            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"SELECT ts.*, t.full_name as TeacherName, 
                                   s.name as SubjectName, s.code as SubjectCode,
                                   c.name as ClassName
                                   FROM teacher_subjects ts
                                   JOIN teachers t ON ts.teacher_id = t.id
                                   JOIN subjects s ON ts.subject_id = s.id
                                   JOIN classes c ON ts.class_id = c.id
                                   WHERE ts.teacher_id = @TeacherId
                                   ORDER BY c.name, s.name";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@TeacherId", teacherId);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                assignments.Add(new TeacherSubjectAssignment
                                {
                                    Id = reader.GetInt32("id"),
                                    TeacherId = reader.GetInt32("teacher_id"),
                                    SubjectId = reader.GetInt32("subject_id"),
                                    ClassId = reader.GetInt32("class_id"),
                                    AcademicYear = reader.GetString("academic_year"),
                                    CreatedAt = reader.GetDateTime("created_at"),
                                    TeacherName = reader.GetString("TeacherName"),
                                    SubjectName = reader.GetString("SubjectName"),
                                    ClassName = reader.GetString("ClassName"),
                                    SubjectCode = reader.GetString("SubjectCode")
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading assignments: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return assignments;
        }

        public bool RemoveAssignment(int assignmentId)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "DELETE FROM teacher_subjects WHERE id = @Id";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", assignmentId);
                        return command.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error removing assignment: {ex.Message}");
            }
        }

        public ObservableCollection<SubjectModel> GetAvailableSubjects()
        {
            var subjects = new ObservableCollection<SubjectModel>();

            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT id, name, code, description FROM subjects ORDER BY name";

                    using (var command = new MySqlCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            subjects.Add(new SubjectModel
                            {
                                Id = reader.GetInt32("id"),
                                Name = reader.GetString("name"),
                                Code = reader.GetString("code"),
                                Description = reader.IsDBNull(reader.GetOrdinal("description"))
                                            ? string.Empty
                                            : reader.GetString("description")
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading subjects: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return subjects;
        }

        public ObservableCollection<ClassModel> GetAvailableClasses()
        {
            var classes = new ObservableCollection<ClassModel>();

            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT id, name, code, description FROM classes ORDER BY name";

                    using (var command = new MySqlCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            classes.Add(new ClassModel
                            {
                                Id = reader.GetInt32("id"),
                                Name = reader.GetString("name"),
                                Code = reader.GetString("code"),
                                Description = reader.IsDBNull(reader.GetOrdinal("description"))
                                            ? string.Empty
                                            : reader.GetString("description")
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading classes: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return classes;
        }
    }
}