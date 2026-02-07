using KisaSchoolMangement.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Windows;

namespace KisaSchoolMangement.Services
{
    public class DonorService
    {
        private readonly string _connectionString;

        public DonorService()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["KisaSchoolDB"].ConnectionString;
        }

        // ✅ Get all donors
        public ObservableCollection<DonorModel> GetAllDonors()
        {
            var donors = new ObservableCollection<DonorModel>();

            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = @"SELECT * FROM donors ORDER BY id DESC";

                    using (var cmd = new MySqlCommand(query, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            donors.Add(new DonorModel
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                Name = reader["name"] != DBNull.Value ? reader["name"].ToString() : "",
                                ContactPerson = reader["contact_person"] != DBNull.Value ? reader["contact_person"].ToString() : "",
                                Email = reader["email"] != DBNull.Value ? reader["email"].ToString() : "",
                                Phone = reader["contact_number"] != DBNull.Value ? reader["contact_number"].ToString() : "",
                                ContactNumber = reader["contact_number"] != DBNull.Value ? reader["contact_number"].ToString() : "",
                                WhatsAppNumber = reader["whatsapp_number"] != DBNull.Value ? reader["whatsapp_number"].ToString() : "",
                                Address = reader["address"] != DBNull.Value ? reader["address"].ToString() : "",
                                Notes = reader["notes"] != DBNull.Value ? reader["notes"].ToString() : "",
                                MonthlyAmount = reader["monthly_amount"] != DBNull.Value ? Convert.ToDecimal(reader["monthly_amount"]) : (decimal?)null,
                                Status = reader["status"] != DBNull.Value ? reader["status"].ToString() : "active",
                                LeftDate = reader["left_date"] != DBNull.Value ? reader["left_date"].ToString() : "",
                                LeavingReason = reader["leaving_reason"] != DBNull.Value ? reader["leaving_reason"].ToString() : "",
                                CreatedAt = reader["created_at"] != DBNull.Value ? reader["created_at"].ToString() : ""
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading donors: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return donors;
        }

        // ✅ Add new donor
        public bool AddDonor(DonorModel donor)
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = @"INSERT INTO donors 
                                    (name, contact_person, email, contact_number, whatsapp_number, 
                                     address, notes, monthly_amount, status, created_at) 
                                    VALUES 
                                    (@Name, @ContactPerson, @Email, @ContactNumber, @WhatsAppNumber,
                                     @Address, @Notes, @MonthlyAmount, @Status, @CreatedAt)";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Name", donor.Name ?? "");
                        cmd.Parameters.AddWithValue("@ContactPerson", donor.ContactPerson ?? "");
                        cmd.Parameters.AddWithValue("@Email", donor.Email ?? "");
                        cmd.Parameters.AddWithValue("@ContactNumber", donor.ContactNumber ?? "");
                        cmd.Parameters.AddWithValue("@WhatsAppNumber", donor.WhatsAppNumber ?? "");
                        cmd.Parameters.AddWithValue("@Address", donor.Address ?? "");
                        cmd.Parameters.AddWithValue("@Notes", donor.Notes ?? "");
                        cmd.Parameters.AddWithValue("@MonthlyAmount", donor.MonthlyAmount.HasValue ? donor.MonthlyAmount.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@Status", donor.Status ?? "active");
                        cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                        int result = cmd.ExecuteNonQuery();
                        return result > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding donor: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        // ✅ Update donor
        public bool UpdateDonor(DonorModel donor)
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = @"UPDATE donors SET 
                                    name = @Name,
                                    contact_person = @ContactPerson,
                                    email = @Email,
                                    contact_number = @ContactNumber,
                                    whatsapp_number = @WhatsAppNumber,
                                    address = @Address,
                                    notes = @Notes,
                                    monthly_amount = @MonthlyAmount,
                                    status = @Status
                                    WHERE id = @Id";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", donor.Id);
                        cmd.Parameters.AddWithValue("@Name", donor.Name ?? "");
                        cmd.Parameters.AddWithValue("@ContactPerson", donor.ContactPerson ?? "");
                        cmd.Parameters.AddWithValue("@Email", donor.Email ?? "");
                        cmd.Parameters.AddWithValue("@ContactNumber", donor.ContactNumber ?? "");
                        cmd.Parameters.AddWithValue("@WhatsAppNumber", donor.WhatsAppNumber ?? "");
                        cmd.Parameters.AddWithValue("@Address", donor.Address ?? "");
                        cmd.Parameters.AddWithValue("@Notes", donor.Notes ?? "");
                        cmd.Parameters.AddWithValue("@MonthlyAmount", donor.MonthlyAmount.HasValue ? donor.MonthlyAmount.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@Status", donor.Status ?? "active");

                        int result = cmd.ExecuteNonQuery();
                        return result > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating donor: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        // ✅ Delete donor
        public bool DeleteDonor(int id)
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = "DELETE FROM donors WHERE id = @Id";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);
                        int result = cmd.ExecuteNonQuery();
                        return result > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting donor: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        // ✅ Get donor by ID
        public DonorModel GetDonorById(int id)
        {
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = "SELECT * FROM donors WHERE id = @Id";

                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new DonorModel
                                {
                                    Id = Convert.ToInt32(reader["id"]),
                                    Name = reader["name"] != DBNull.Value ? reader["name"].ToString() : "",
                                    ContactPerson = reader["contact_person"] != DBNull.Value ? reader["contact_person"].ToString() : "",
                                    Email = reader["email"] != DBNull.Value ? reader["email"].ToString() : "",
                                    ContactNumber = reader["contact_number"] != DBNull.Value ? reader["contact_number"].ToString() : "",
                                    WhatsAppNumber = reader["whatsapp_number"] != DBNull.Value ? reader["whatsapp_number"].ToString() : "",
                                    Address = reader["address"] != DBNull.Value ? reader["address"].ToString() : "",
                                    Notes = reader["notes"] != DBNull.Value ? reader["notes"].ToString() : "",
                                    MonthlyAmount = reader["monthly_amount"] != DBNull.Value ? Convert.ToDecimal(reader["monthly_amount"]) : (decimal?)null,
                                    Status = reader["status"] != DBNull.Value ? reader["status"].ToString() : "active"
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error getting donor: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return null;
        }
    }
}