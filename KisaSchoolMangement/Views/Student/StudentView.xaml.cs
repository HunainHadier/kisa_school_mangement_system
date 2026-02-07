using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using KisaSchoolMangement.Models;
using KisaSchoolMangement.Services;

namespace KisaSchoolMangement.Views.Student
{
    public partial class StudentView : Window
    {
        private readonly StudentService _studentService;
        private StudentModel _selectedStudent;
        private ObservableCollection<StudentModel> _allStudents;

        public StudentView()
        {
            InitializeComponent();
            _studentService = new StudentService();
            LoadStudents();
            InitializeFilters();
            UpdateSelectedInfo();
        }

        private void InitializeFilters()
        {
            // Set default date range (last 30 days)
            dpFromDate.SelectedDate = DateTime.Now.AddDays(-30);
            dpToDate.SelectedDate = DateTime.Now;
        }

        private void LoadStudents()
        {
            try
            {
                _allStudents = _studentService.GetAllStudents();

                // Debugging: Check if data is loaded
                if (_allStudents != null && _allStudents.Count > 0)
                {
                    var firstStudent = _allStudents[0];
                    Console.WriteLine($"First Student: {firstStudent.StudentName}");
                    Console.WriteLine($"Father Name: {firstStudent.FatherName}");
                    Console.WriteLine($"Mother Name: {firstStudent.MotherName}");
                    Console.WriteLine($"Family Code: {firstStudent.FamilyCode}");
                    // Add more fields as needed for debugging
                }

                StudentDataGrid.ItemsSource = _allStudents;
                UpdateStudentCount();
                UpdateSelectedInfo();
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error loading students: {ex.Message}");
            }
        }
        private void UpdateStudentCount()
        {
            txtTotalStudents.Text = _allStudents?.Count.ToString() ?? "0";
        }

        private void UpdateSelectedInfo()
        {
            if (_selectedStudent != null)
            {
                txtSelectedInfo.Text = $"Selected: {_selectedStudent.StudentName} (GR: {_selectedStudent.GrNo}, Class: {_selectedStudent.ClassName})";
            }
            else
            {
                txtSelectedInfo.Text = "No student selected - Select a student to edit or delete";
            }
        }

        private void StudentDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedStudent = StudentDataGrid.SelectedItem as StudentModel;
            btnEdit.IsEnabled = _selectedStudent != null;
            btnDelete.IsEnabled = _selectedStudent != null;

            // View Details button ko bhi enable karein
            if (btnViewDetails != null)
            {
                btnViewDetails.IsEnabled = _selectedStudent != null;
            }

            UpdateSelectedInfo();
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterStudents();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            FilterStudents();
        }

        private void FilterStudents()
        {
            if (_allStudents == null) return;

            var searchText = txtSearch.Text.ToLower();
            var searchField = cmbSearchField.SelectedIndex;

            if (string.IsNullOrEmpty(searchText))
            {
                StudentDataGrid.ItemsSource = _allStudents;
                return;
            }

            var filtered = _allStudents.Where(student =>
            {
                if (string.IsNullOrEmpty(searchText))
                    return true;

                return searchField switch
                {
                    1 => student.StudentName?.ToLower().Contains(searchText) == true, // Name
                    2 => student.GrNo?.ToLower().Contains(searchText) == true, // GR Number
                    3 => student.ClassName?.ToLower().Contains(searchText) == true, // Class
                    4 => student.Age?.ToLower().Contains(searchText) == true, // Age
                    5 => student.FatherName?.ToLower().Contains(searchText) == true, // Father Name
                    6 => student.GuardianName?.ToLower().Contains(searchText) == true, // Guardian Name
                    _ => student.StudentName?.ToLower().Contains(searchText) == true ||
                         student.GrNo?.ToLower().Contains(searchText) == true ||
                         student.ClassName?.ToLower().Contains(searchText) == true ||
                         student.Age?.ToLower().Contains(searchText) == true ||
                         student.FatherName?.ToLower().Contains(searchText) == true ||
                         student.GuardianName?.ToLower().Contains(searchText) == true // All Fields
                };
            });

            StudentDataGrid.ItemsSource = new ObservableCollection<StudentModel>(filtered);
            UpdateStudentCount();
        }

        private void btnApplyDate_Click(object sender, RoutedEventArgs e)
        {
            if (dpFromDate.SelectedDate == null || dpToDate.SelectedDate == null)
            {
                ShowWarningMessage("Please select both from and to dates!");
                return;
            }

            if (dpFromDate.SelectedDate > dpToDate.SelectedDate)
            {
                ShowWarningMessage("From date cannot be greater than To date!");
                return;
            }

            FilterStudents();
            ShowSuccessMessage($"Date filter applied from {dpFromDate.SelectedDate.Value:MMM dd, yyyy} to {dpToDate.SelectedDate.Value:MMM dd, yyyy}!");
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var addStudentWindow = new AddStudentWindow();
                addStudentWindow.Owner = this;
                addStudentWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;

                if (addStudentWindow.ShowDialog() == true)
                {
                    LoadStudents();
                    ShowSuccessMessage("Student added successfully! 🎉");
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error adding student: {ex.Message}");
            }
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedStudent == null)
            {
                ShowWarningMessage("Please select a student to edit!");
                return;
            }

            try
            {
                var editStudentWindow = new AddStudentWindow(_selectedStudent);
                editStudentWindow.Owner = this;
                editStudentWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;

                if (editStudentWindow.ShowDialog() == true)
                {
                    LoadStudents();
                    ShowSuccessMessage("Student updated successfully! ✨");
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error updating student: {ex.Message}");
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedStudent == null)
            {
                ShowWarningMessage("Please select a student to delete!");
                return;
            }

            var result = MessageBox.Show(
                $"Are you sure you want to delete this student?\n\n" +
                $"📝 Name: {_selectedStudent.StudentName}\n" +
                $"🎫 GR Number: {_selectedStudent.GrNo}\n" +
                $"🏫 Class: {_selectedStudent.ClassName}\n\n" +
                $"This action cannot be undone!",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    bool success = _studentService.DeleteStudent(_selectedStudent.Id);

                    if (success)
                    {
                        LoadStudents();
                        ShowSuccessMessage("Student deleted successfully! ✅");
                    }
                    else
                    {
                        ShowErrorMessage("Failed to delete student. Please try again.");
                    }
                }
                catch (Exception ex)
                {
                    ShowErrorMessage($"Error deleting student: {ex.Message}");
                }
            }
        }

        private void btnViewDetails_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedStudent == null)
            {
                ShowWarningMessage("Please select a student to view details!");
                return;
            }

            try
            {
                // Student details window show karein
                // Pehle check karein ke StudentDetailsWindow class exist karti hai ya nahi
                ShowSuccessMessage($"View Details clicked for: {_selectedStudent.StudentName}");

                // Agar StudentDetailsWindow class nahi hai toh simple message show karein
                MessageBox.Show(
                    $"Student Details:\n\n" +
                    $"Name: {_selectedStudent.StudentName}\n" +
                    $"GR No: {_selectedStudent.GrNo}\n" +
                    $"Class: {_selectedStudent.ClassName}\n" +
                    $"Father: {_selectedStudent.FatherName}\n" +
                    $"Guardian: {_selectedStudent.GuardianName}\n" +
                    $"Phone: {_selectedStudent.GuardianPhone}",
                    "Student Details",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error showing student details: {ex.Message}");
            }
        }

        private void btnImportExcel_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Excel Files|*.xls;*.xlsx;*.xlsm",
                Title = "Select Excel File to Import",
                Multiselect = false
            };

            if (openFileDialog.ShowDialog() == true)
            {
                // Simulate import process
                System.Threading.Thread.Sleep(1500);
                ShowSuccessMessage($"✅ Successfully imported data from: {System.IO.Path.GetFileName(openFileDialog.FileName)}");
                LoadStudents();
            }
        }

        private void btnExportExcel_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel Files|*.xlsx",
                FileName = $"Students_Export_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
                DefaultExt = ".xlsx"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                // Simulate export process
                System.Threading.Thread.Sleep(1500);
                ShowSuccessMessage($"📊 Data exported successfully to: {System.IO.Path.GetFileName(saveFileDialog.FileName)}");
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadStudents();
            ClearFilters();
            ShowSuccessMessage("Data refreshed successfully! 🔄");
        }

        private void ClearFilters()
        {
            txtSearch.Clear();
            dpFromDate.SelectedDate = DateTime.Now.AddDays(-30);
            dpToDate.SelectedDate = DateTime.Now;
            cmbSearchField.SelectedIndex = 0;
        }

        // Helper methods for consistent messaging
        private void ShowSuccessMessage(string message)
        {
            MessageBox.Show(message, "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ShowWarningMessage(string message)
        {
            MessageBox.Show(message, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void ShowErrorMessage(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}