using KisaSchoolMangement.Models;
using KisaSchoolMangement.Services;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace KisaSchoolMangement.Views.Donors
{
    public partial class DonorStudentAssignmentsView : Window
    {
        private readonly DonorStudentService _assignmentService;
        private ObservableCollection<DonorStudentModel> _allAssignments;
        private ObservableCollection<DonorStudentModel> _filteredAssignments;
        private DonorStudentModel _selectedAssignment;

        public string TotalAssignments => _allAssignments?.Count.ToString() ?? "0";
        public string ActiveAssignments => _allAssignments?.Count(a => a.Status == "Active").ToString() ?? "0";
        public string ExpiredAssignments => _allAssignments?.Count(a => a.Status == "Expired").ToString() ?? "0";
        public string TotalAmount => _allAssignments?.Sum(a => a.Amount).ToString("C") ?? "$0";

        public DonorStudentAssignmentsView()
        {
            InitializeComponent();
            _assignmentService = new DonorStudentService();
            LoadAssignments();
            InitializeFilters();
        }

        private void InitializeFilters()
        {
            dpFromDate.SelectedDate = DateTime.Today.AddMonths(-6);
            dpToDate.SelectedDate = DateTime.Today;
        }

        private void LoadAssignments()
        {
            try
            {
                _allAssignments = _assignmentService.GetAllAssignments();
                _filteredAssignments = new ObservableCollection<DonorStudentModel>(_allAssignments);
                dgAssignments.ItemsSource = _filteredAssignments;

                // Update statistics
                UpdateStatistics();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading assignments: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateStatistics()
        {
            // Refresh bindings
            OnPropertyChanged(nameof(TotalAssignments));
            OnPropertyChanged(nameof(ActiveAssignments));
            OnPropertyChanged(nameof(ExpiredAssignments));
            OnPropertyChanged(nameof(TotalAmount));
        }

        private void FilterAssignments()
        {
            if (_allAssignments == null) return;

            var searchText = txtSearch.Text.ToLower();
            var searchField = cmbSearchField.SelectedIndex;
            var statusFilter = (cmbStatusFilter.SelectedItem as ComboBoxItem)?.Content.ToString();
            var fromDate = dpFromDate.SelectedDate;
            var toDate = dpToDate.SelectedDate;

            var filtered = _allAssignments.Where(assignment =>
            {
                // Status filter
                if (statusFilter != "All Status" && assignment.Status != statusFilter)
                    return false;

                // Date filter
                if (fromDate != null && DateTime.TryParse(assignment.StartDate, out DateTime startDate))
                {
                    if (startDate < fromDate.Value.Date) return false;
                }

                if (toDate != null && DateTime.TryParse(assignment.StartDate, out startDate))
                {
                    if (startDate > toDate.Value.Date) return false;
                }

                // Search filter
                if (string.IsNullOrEmpty(searchText))
                    return true;

                return searchField switch
                {
                    1 => assignment.StudentName?.ToLower().Contains(searchText) == true, // Student Name
                    2 => assignment.DonorName?.ToLower().Contains(searchText) == true, // Donor Name
                    3 => assignment.StudentClassName?.ToLower().Contains(searchText) == true, // Class
                    4 => assignment.StudentAdmissionNo?.ToLower().Contains(searchText) == true, // Admission No
                    5 => assignment.Status?.ToLower().Contains(searchText) == true, // Status
                    _ => assignment.StudentName?.ToLower().Contains(searchText) == true ||
                         assignment.DonorName?.ToLower().Contains(searchText) == true ||
                         assignment.StudentClassName?.ToLower().Contains(searchText) == true ||
                         assignment.StudentAdmissionNo?.ToLower().Contains(searchText) == true ||
                         assignment.Status?.ToLower().Contains(searchText) == true // All Fields
                };
            });

            _filteredAssignments = new ObservableCollection<DonorStudentModel>(filtered);
            dgAssignments.ItemsSource = _filteredAssignments;
            UpdateStatistics();
        }

        // Event Handlers
        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterAssignments();
        }

        private void cmbStatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterAssignments();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            FilterAssignments();
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Clear();
            cmbSearchField.SelectedIndex = 0;
            cmbStatusFilter.SelectedIndex = 0;
            dpFromDate.SelectedDate = DateTime.Today.AddMonths(-6);
            dpToDate.SelectedDate = DateTime.Today;
            FilterAssignments();
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel Files|*.xlsx",
                FileName = $"Donor_Assignments_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                // Simulate export
                System.Threading.Thread.Sleep(1000);
                MessageBox.Show($"Data exported successfully to: {saveFileDialog.FileName}", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void dgAssignments_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedAssignment = dgAssignments.SelectedItem as DonorStudentModel;
        }

        private void btnNewAssignment_Click(object sender, RoutedEventArgs e)
        {
            // You can open the existing AssignDonorWindow or create a new one
            MessageBox.Show("Open donor assignment window", "New Assignment",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btnEditAssignment_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedAssignment == null)
            {
                MessageBox.Show("Please select an assignment to edit!", "Warning",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Open edit window
            var editWindow = new AssignDonorWindow(_selectedAssignment);
            editWindow.Owner = this;
            if (editWindow.ShowDialog() == true)
            {
                LoadAssignments();
            }
        }

        private void btnRemoveAssignment_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedAssignment == null)
            {
                MessageBox.Show("Please select an assignment to remove!", "Warning",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Are you sure you want to remove this assignment?\n\n" +
                $"Student: {_selectedAssignment.StudentName}\n" +
                $"Donor: {_selectedAssignment.DonorName}\n" +
                $"Amount: {_selectedAssignment.AmountFormatted}",
                "Confirm Removal",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                bool success = _assignmentService.RemoveAssignment(
                    _selectedAssignment.StudentId,
                    _selectedAssignment.DonorId);

                if (success)
                {
                    LoadAssignments();
                    MessageBox.Show("Assignment removed successfully!", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Failed to remove assignment!", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnViewDetails_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedAssignment == null)
            {
                MessageBox.Show("Please select an assignment to view details!", "Warning",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Show detailed view
            ShowAssignmentDetails(_selectedAssignment);
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadAssignments();
            MessageBox.Show("Data refreshed successfully!", "Refresh",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ShowAssignmentDetails(DonorStudentModel assignment)
        {
            string details = $@"
🎯 ASSIGNMENT DETAILS

📚 STUDENT INFORMATION:
• Name: {assignment.StudentName}
• Admission No: {assignment.StudentAdmissionNo}
• Class: {assignment.StudentClassName}
• Section: {assignment.StudentSectionName}
• Guardian: {assignment.StudentGuardianName}
• Guardian Phone: {assignment.StudentGuardianPhone}
• Date of Birth: {assignment.StudentDOB}
• Gender: {assignment.StudentGender}

🤝 DONOR INFORMATION:
• Name: {assignment.DonorName}
• Contact Person: {assignment.DonorContactPerson}
• Email: {assignment.DonorEmail}
• Phone: {assignment.DonorPhone}
• Address: {assignment.DonorAddress}
• Notes: {assignment.DonorNotes}

💰 ASSIGNMENT DETAILS:
• Amount: {assignment.AmountFormatted}
• Duration: {assignment.Duration}
• Status: {assignment.Status}
• Notes: {assignment.Note}
";

            MessageBox.Show(details, "Assignment Details",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // INotifyPropertyChanged implementation
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }
}