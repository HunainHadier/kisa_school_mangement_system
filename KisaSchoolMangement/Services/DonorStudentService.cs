using KisaSchoolMangement.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Windows;

namespace KisaSchoolMangement.Services
{
    public class DonorStudentService
    {
        private readonly string _connectionString;

        public DonorStudentService()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["KisaSchoolDB"].ConnectionString;
        }

        // ✅ Get all donor-student assignments with details
        public ObservableCollection<DonorStudentModel> GetAllAssignments()
        {
            var assignments = new ObservableCollection<DonorStudentModel>();

            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    // Updated query based on your actual database structure
                    string query = @"SELECT 
                                    sd.student_id,
                                    sd.donor_id,
                                    sd.amount,
                                    sd.start_date,
                                    sd.end_date,
                                    sd.note,
                                    s.student_name,
                                    s.gr_no as admission_no,
                                    s.guardian_phone,
                                    s.dob as student_dob,
                                    s.gender as student_gender,
                                    s.guardian_name,
                                    c.name as class_name,
                                    sec.name as section_name,
                                    d.name as donor_name,
                                    d.contact_person,
                                    d.email as donor_email,
                                    d.contact_number as donor_phone,
                                    d.address as donor_address,
                                    d.notes as donor_notes
                                FROM student_donors sd
                                INNER JOIN students s ON sd.student_id = s.id
                                INNER JOIN donors d ON sd.donor_id = d.id
                                LEFT JOIN classes c ON s.class_id = c.id
                                LEFT JOIN sections sec ON s.section_id = sec.id
                                ORDER BY sd.start_date DESC";

                    using (var cmd = new MySqlCommand(query, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            assignments.Add(new DonorStudentModel
                            {
                                StudentId = Convert.ToInt32(reader["student_id"]),
                                DonorId = Convert.ToInt32(reader["donor_id"]),
                                Amount = Convert.ToDecimal(reader["amount"]),
                                StartDate = reader["start_date"].ToString(),
                                EndDate = reader["end_date"] == DBNull.Value ? null : reader["end_date"].ToString(),
                                Note = reader["note"].ToString(),

                                // Student details
                                StudentName = reader["student_name"].ToString(),
                                StudentAdmissionNo = reader["admission_no"].ToString(),
                                StudentClassName = reader["class_name"]?.ToString() ?? "N/A",
                                StudentSectionName = reader["section_name"]?.ToString() ?? "N/A",
                                StudentGuardianName = reader["guardian_name"].ToString(),
                                StudentGuardianPhone = reader["guardian_phone"].ToString(),
                                StudentDOB = reader["student_dob"].ToString(),
                                StudentGender = reader["student_gender"].ToString(),

                                // Donor details
                                DonorName = reader["donor_name"].ToString(),
                                DonorContactPerson = reader["contact_person"].ToString(),
                                DonorEmail = reader["donor_email"].ToString(),
                                DonorPhone = reader["donor_phone"].ToString(),
                                DonorAddress = reader["donor_address"].ToString(),
                                DonorNotes = reader["donor_notes"].ToString()
                            });
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

        // ✅ Assign donor to student
        public bool AssignDonorToStudent(DonorStudentModel assignment)
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    // Check if assignment already exists
                    string checkQuery = "SELECT COUNT(*) FROM student_donors WHERE student_id = @StudentId AND donor_id = @DonorId";
                    using (var checkCmd = new MySqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@StudentId", assignment.StudentId);
                        checkCmd.Parameters.AddWithValue("@DonorId", assignment.DonorId);

                        long existingCount = (long)checkCmd.ExecuteScalar();
                        if (existingCount > 0)
                        {
                            MessageBox.Show("This donor is already assigned to the selected student!", "Duplicate Assignment",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                            return false;
                        }
                    }

                    // Insert new assignment
                    string query = @"INSERT INTO student_donors (student_id, donor_id, amount, start_date, end_date, note)
                                     VALUES (@StudentId, @DonorId, @Amount, @StartDate, @EndDate, @Note)";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@StudentId", assignment.StudentId);
                        cmd.Parameters.AddWithValue("@DonorId", assignment.DonorId);
                        cmd.Parameters.AddWithValue("@Amount", assignment.Amount);
                        cmd.Parameters.AddWithValue("@StartDate", assignment.StartDate);
                        cmd.Parameters.AddWithValue("@EndDate", string.IsNullOrEmpty(assignment.EndDate) ? DBNull.Value : (object)assignment.EndDate);
                        cmd.Parameters.AddWithValue("@Note", assignment.Note ?? "");

                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error assigning donor to student: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        // ✅ Remove assignment
        public bool RemoveAssignment(int studentId, int donorId)
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = "DELETE FROM student_donors WHERE student_id = @StudentId AND donor_id = @DonorId";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@StudentId", studentId);
                        cmd.Parameters.AddWithValue("@DonorId", donorId);
                        return cmd.ExecuteNonQuery() > 0;
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

        // ✅ Update assignment
        public bool UpdateAssignment(DonorStudentModel assignment)
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = @"UPDATE student_donors 
                                     SET amount = @Amount, start_date = @StartDate, 
                                         end_date = @EndDate, note = @Note
                                     WHERE student_id = @StudentId AND donor_id = @DonorId";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@StudentId", assignment.StudentId);
                        cmd.Parameters.AddWithValue("@DonorId", assignment.DonorId);
                        cmd.Parameters.AddWithValue("@Amount", assignment.Amount);
                        cmd.Parameters.AddWithValue("@StartDate", assignment.StartDate);
                        cmd.Parameters.AddWithValue("@EndDate", string.IsNullOrEmpty(assignment.EndDate) ? DBNull.Value : (object)assignment.EndDate);
                        cmd.Parameters.AddWithValue("@Note", assignment.Note ?? "");

                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating assignment: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
    }
}