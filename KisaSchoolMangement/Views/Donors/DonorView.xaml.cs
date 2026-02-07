using KisaSchoolMangement.Models;
using KisaSchoolMangement.Services;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace KisaSchoolMangement.Views.Donors
{
    public partial class DonorView : Window
    {
        private readonly DonorService _donorService;
        private ObservableCollection<DonorModel> _donors;
        private DonorModel _selectedDonor;

        public DonorView()
        {
            InitializeComponent();
            _donorService = new DonorService();
            LoadDonors();
        }

        // ✅ Load all donors in DataGrid
        private void LoadDonors()
        {
            _donors = _donorService.GetAllDonors();
            dgDonors.ItemsSource = _donors;
        }

        // ✅ Add new donor
        private void Add_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtPhone.Text))
                {
                    MessageBox.Show("Please enter Name and Phone!", "Validation Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var donor = new DonorModel
                {
                    Name = txtName.Text.Trim(),
                    ContactPerson = txtContactPerson.Text.Trim(),
                    Email = txtEmail.Text.Trim(),
                    ContactNumber = txtPhone.Text.Trim(),
                    WhatsAppNumber = txtWhatsApp.Text.Trim(),
                    Address = txtAddress.Text.Trim(),
                    Notes = txtNotes.Text.Trim(),
                    MonthlyAmount = decimal.TryParse(txtMonthlyAmount.Text, out decimal amount) ? amount : 0,
                    Status = "active",
                    CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                bool added = _donorService.AddDonor(donor);

                if (added)
                {
                    MessageBox.Show("✅ Donor added successfully!", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    ClearForm();
                    LoadDonors();
                }
                else
                {
                    MessageBox.Show("Failed to add donor!", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding donor: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ✅ Update donor
        private void Update_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedDonor == null)
            {
                MessageBox.Show("Please select a donor first!", "Info",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtPhone.Text))
                {
                    MessageBox.Show("Please enter Name and Phone!", "Validation Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                _selectedDonor.Name = txtName.Text.Trim();
                _selectedDonor.ContactPerson = txtContactPerson.Text.Trim();
                _selectedDonor.Email = txtEmail.Text.Trim();
                _selectedDonor.ContactNumber = txtPhone.Text.Trim();
                _selectedDonor.WhatsAppNumber = txtWhatsApp.Text.Trim();
                _selectedDonor.Address = txtAddress.Text.Trim();
                _selectedDonor.Notes = txtNotes.Text.Trim();
                _selectedDonor.MonthlyAmount = decimal.TryParse(txtMonthlyAmount.Text, out decimal amount) ? amount : 0;

                bool updated = _donorService.UpdateDonor(_selectedDonor);

                if (updated)
                {
                    MessageBox.Show("✏️ Donor updated successfully!", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    ClearForm();
                    LoadDonors();
                }
                else
                {
                    MessageBox.Show("Failed to update donor!", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating donor: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ✅ Delete donor
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedDonor == null)
            {
                MessageBox.Show("Please select a donor first!", "Info",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (MessageBox.Show("Are you sure you want to delete this donor?", "Confirm Delete",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                bool deleted = _donorService.DeleteDonor(_selectedDonor.Id);

                if (deleted)
                {
                    MessageBox.Show("🗑️ Donor deleted successfully!", "Deleted",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    ClearForm();
                    LoadDonors();
                }
                else
                {
                    MessageBox.Show("Failed to delete donor!", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // ✅ Refresh donors
        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadDonors();
            ClearForm();
        }

        // ✅ Assign Donor Button Click
        private void AssignDonor_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedDonor == null)
            {
                MessageBox.Show("Please select a donor first to assign!", "Info",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Open Assign Donor Window
            var assignDonorWindow = new AssignDonorWindow(_selectedDonor);
            assignDonorWindow.Owner = this;
            assignDonorWindow.ShowDialog();
        }

        // ✅ When donor is selected from DataGrid
        private void dgDonors_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedDonor = dgDonors.SelectedItem as DonorModel;
            if (_selectedDonor != null)
            {
                txtName.Text = _selectedDonor.Name;
                txtContactPerson.Text = _selectedDonor.ContactPerson;
                txtEmail.Text = _selectedDonor.Email;
                txtPhone.Text = _selectedDonor.ContactNumber;
                txtWhatsApp.Text = _selectedDonor.WhatsAppNumber;
                txtAddress.Text = _selectedDonor.Address;
                txtNotes.Text = _selectedDonor.Notes;
                txtMonthlyAmount.Text = _selectedDonor.MonthlyAmount?.ToString() ?? "0";
            }
        }

        // ✅ Clear form fields
        private void ClearForm()
        {
            txtName.Clear();
            txtContactPerson.Clear();
            txtEmail.Clear();
            txtPhone.Clear();
            txtWhatsApp.Clear();
            txtAddress.Clear();
            txtNotes.Clear();
            txtMonthlyAmount.Text = "0";
            _selectedDonor = null;
        }
    }
}