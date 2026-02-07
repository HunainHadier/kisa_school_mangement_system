using KisaSchoolMangement.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Windows;

namespace KisaSchoolMangement.Services
{
    public class StudentDonorService
    {
        private readonly string _connectionString;

        public StudentDonorService()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["KisaSchoolDB"].ConnectionString;
        }

        public ObservableCollection<StudentDonorAssignment> GetAllStudentDonorAssignments()
        {
            var assignments = new ObservableCollection<StudentDonorAssignment>();

            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();

                    // Updated query based on your actual database structure
                    string query = @"
                        SELECT 
                            sd.student_id as StudentId,
                            s.student_name as StudentName,
                            s.guardian_phone as StudentPhone,
                            c.name as ClassName,
                            sec.name as SectionName,
                            sd.donor_id as DonorId,
                            d.name as DonorName,
                            d.email as DonorEmail,
                            d.contact_number as DonorPhone,
                            sd.amount as DonationAmount,
                            sd.start_date as AssignedDate,
                            sd.note as Notes,
                            CASE 
                                WHEN sd.end_date IS NULL THEN 'Active'
                                WHEN sd.end_date >= CURDATE() THEN 'Active'
                                ELSE 'Expired'
                            END as Status
                        FROM student_donors sd
                        JOIN students s ON sd.student_id = s.id
                        JOIN donors d ON sd.donor_id = d.id
                        LEFT JOIN classes c ON s.class_id = c.id
                        LEFT JOIN sections sec ON s.section_id = sec.id
                        ORDER BY sd.start_date DESC";

                    using (var command = new MySqlCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            try
                            {
                                var assignment = new StudentDonorAssignment
                                {
                                    StudentId = reader.GetInt32("StudentId"),
                                    DonorId = reader.GetInt32("DonorId"),
                                    DonationAmount = reader.GetDecimal("DonationAmount"),
                                    AssignedDate = reader.GetDateTime("AssignedDate"),
                                    DonationType = "Monthly Scholarship", // Default value
                                    Status = GetSafeString(reader, "Status", "Active")
                                };

                                // Safe string assignments
                                assignment.StudentName = GetSafeString(reader, "StudentName");
                                assignment.StudentPhone = GetSafeString(reader, "StudentPhone");
                                assignment.ClassName = GetSafeString(reader, "ClassName", "N/A");
                                assignment.SectionName = GetSafeString(reader, "SectionName", "N/A");
                                assignment.DonorName = GetSafeString(reader, "DonorName");
                                assignment.DonorEmail = GetSafeString(reader, "DonorEmail");
                                assignment.DonorPhone = GetSafeString(reader, "DonorPhone");
                                assignment.Notes = GetSafeString(reader, "Notes");

                                assignments.Add(assignment);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Error reading record: {ex.Message}", "Data Error",
                                    MessageBoxButton.OK, MessageBoxImage.Warning);
                                continue;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database Error: {ex.Message}", "Database Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return assignments;
        }

        private string GetSafeString(MySqlDataReader reader, string columnName, string defaultValue = "")
        {
            try
            {
                int ordinal = reader.GetOrdinal(columnName);
                return reader.IsDBNull(ordinal) ? defaultValue : reader.GetString(ordinal);
            }
            catch
            {
                return defaultValue;
            }
        }

        public bool RemoveAssignment(int studentId, int donorId)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = "DELETE FROM student_donors WHERE student_id = @StudentId AND donor_id = @DonorId";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@StudentId", studentId);
                        command.Parameters.AddWithValue("@DonorId", donorId);
                        return command.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error removing assignment: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public void ExportToExcel(System.Collections.Generic.List<StudentDonorAssignment> assignments, string filePath)
        {
            try
            {
                using (var writer = new StreamWriter(filePath))
                {
                    writer.WriteLine("Student ID,Student Name,Student Phone,Class,Section,Donor ID,Donor Name,Donor Email,Donor Phone,Donation Type,Donation Amount,Assigned Date,Notes,Status");

                    foreach (var assignment in assignments)
                    {
                        writer.WriteLine(
                            $"{assignment.StudentId}," +
                            $"\"{assignment.StudentName}\"," +
                            $"\"{assignment.StudentPhone}\"," +
                            $"\"{assignment.ClassName}\"," +
                            $"\"{assignment.SectionName}\"," +
                            $"{assignment.DonorId}," +
                            $"\"{assignment.DonorName}\"," +
                            $"\"{assignment.DonorEmail}\"," +
                            $"\"{assignment.DonorPhone}\"," +
                            $"\"{assignment.DonationType}\"," +
                            $"{assignment.DonationAmount}," +
                            $"{assignment.AssignedDate:yyyy-MM-dd}," +
                            $"\"{assignment.Notes}\"," +
                            $"\"{assignment.Status}\""
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error exporting to Excel: {ex.Message}");
            }
        }
    }
}