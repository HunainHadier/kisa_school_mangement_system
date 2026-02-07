using KisaSchoolMangement.Models;
using KisaSchoolMangement.Services;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace KisaSchoolMangement.Views.Students
{
    public partial class StudentsWithDonorsView : Window
    {
        private readonly StudentDonorService _studentDonorService;
        private ObservableCollection<StudentDonorAssignment> _allAssignments;
        private ObservableCollection<StudentDonorAssignment> _filteredAssignments;

        public StudentsWithDonorsView()
        {
            try
            {
                InitializeComponent();
                _studentDonorService = new StudentDonorService();
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Window initialization error: {ex.Message}\n\nStack Trace: {ex.StackTrace}",
                    "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadData()
        {
            try
            {
                _allAssignments = _studentDonorService.GetAllStudentDonorAssignments();

                // Check if data is loaded
                if (_allAssignments == null)
                {
                    MessageBox.Show("No data returned from service", "Info",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    _allAssignments = new ObservableCollection<StudentDonorAssignment>();
                }

                _filteredAssignments = new ObservableCollection<StudentDonorAssignment>(_allAssignments);
                dgStudentsWithDonors.ItemsSource = _filteredAssignments;

                UpdateSummary();
                LoadFilters();
                txtStatus.Text = $"✅ Loaded {_allAssignments.Count} assignments";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}\n\nStack Trace: {ex.StackTrace}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                txtStatus.Text = "❌ Error loading data";
            }
        }

        private void LoadFilters()
        {
            try
            {
                // Check if we have data
                if (_allAssignments == null || _allAssignments.Count == 0)
                {
                    // Add default items only
                    cmbClassFilter.Items.Clear();
                    cmbClassFilter.Items.Add(new ComboBoxItem { Content = "All Classes", IsSelected = true });

                    cmbDonorFilter.Items.Clear();
                    cmbDonorFilter.Items.Add(new ComboBoxItem { Content = "All Donors", IsSelected = true });
                    return;
                }

                // Load classes
                var classes = _allAssignments
                    .Where(a => !string.IsNullOrEmpty(a.ClassName))
                    .Select(a => a.ClassName)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToList();

                cmbClassFilter.Items.Clear();
                cmbClassFilter.Items.Add(new ComboBoxItem { Content = "All Classes", IsSelected = true });
                foreach (var className in classes)
                {
                    cmbClassFilter.Items.Add(new ComboBoxItem { Content = className });
                }

                // Load donors
                var donors = _allAssignments
                    .Where(a => !string.IsNullOrEmpty(a.DonorName))
                    .Select(a => a.DonorName)
                    .Distinct()
                    .OrderBy(d => d)
                    .ToList();

                cmbDonorFilter.Items.Clear();
                cmbDonorFilter.Items.Add(new ComboBoxItem { Content = "All Donors", IsSelected = true });
                foreach (var donorName in donors)
                {
                    cmbDonorFilter.Items.Add(new ComboBoxItem { Content = donorName });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading filters: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateSummary()
        {
            try
            {
                if (_filteredAssignments == null)
                {
                    txtSummary.Text = "📊 Summary: 0 students found";
                    txtRecordCount.Text = "Total Records: 0";
                    return;
                }

                var totalStudents = _filteredAssignments.Count;
                var totalAmount = _filteredAssignments.Sum(a => a.DonationAmount);
                var activeAssignments = _filteredAssignments.Count(a => a.Status == "Active");

                txtSummary.Text = $"📊 Summary: {totalStudents} students | Active: {activeAssignments} | Total Donation: {totalAmount:C}";
                txtRecordCount.Text = $"Total Records: {totalStudents}";
            }
            catch (Exception ex)
            {
                txtSummary.Text = "📊 Summary: Error calculating summary";
                MessageBox.Show($"Error updating summary: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FilterData()
        {
            try
            {
                if (_allAssignments == null) return;

                var searchText = txtSearch?.Text?.ToLower() ?? "";
                var selectedClass = (cmbClassFilter.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "All Classes";
                var selectedDonor = (cmbDonorFilter.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "All Donors";

                var filtered = _allAssignments.Where(a =>
                    (string.IsNullOrEmpty(searchText) ||
                     (a.StudentName?.ToLower().Contains(searchText) ?? false) ||
                     (a.DonorName?.ToLower().Contains(searchText) ?? false) ||
                     (a.ClassName?.ToLower().Contains(searchText) ?? false)) &&

                    (selectedClass == "All Classes" || a.ClassName == selectedClass) &&

                    (selectedDonor == "All Donors" || a.DonorName == selectedDonor)
                ).ToList();

                _filteredAssignments = new ObservableCollection<StudentDonorAssignment>(filtered);
                dgStudentsWithDonors.ItemsSource = _filteredAssignments;
                UpdateSummary();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error filtering data: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Rest of your event handlers remain the same...
        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterData();
        }

        private void CmbClassFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterData();
        }

        private void CmbDonorFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterData();
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void BtnExportExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_filteredAssignments == null || _filteredAssignments.Count == 0)
                {
                    MessageBox.Show("No data to export", "Info",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "CSV Files (*.csv)|*.csv",
                    FileName = $"Students_With_Donors_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    _studentDonorService.ExportToExcel(_filteredAssignments.ToList(), saveFileDialog.FileName);
                    MessageBox.Show($"Data exported successfully to {saveFileDialog.FileName}", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    txtStatus.Text = "✅ Data exported successfully";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting data: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                txtStatus.Text = "❌ Error exporting data";
            }
        }

        private void BtnRemoveAssignment_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as Button;
                var assignment = button?.DataContext as StudentDonorAssignment;
                if (assignment != null)
                {
                    var result = MessageBox.Show(
                        $"Are you sure you want to remove donor assignment for {assignment.StudentName}?",
                        "Confirm Removal",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        if (_studentDonorService.RemoveAssignment(assignment.StudentId, assignment.DonorId))
                        {
                            MessageBox.Show("Assignment removed successfully!", "Success",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                            LoadData();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error removing assignment: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}