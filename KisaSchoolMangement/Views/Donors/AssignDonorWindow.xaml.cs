using KisaSchoolMangement.Models;
using KisaSchoolMangement.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace KisaSchoolMangement.Views.Donors
{
    public partial class AssignDonorWindow : Window
    {
        private readonly DonorModel _selectedDonor;
        private readonly DonorStudentModel _selectedAssignment;
        private readonly StudentService _studentService;
        private readonly DonorStudentService _donorStudentService;
        private ObservableCollection<StudentModel> _students;

        // Original constructor for new assignment
        public AssignDonorWindow(DonorModel donor) 
        {
            InitializeComponent();
            _selectedDonor = donor;
            _studentService = new StudentService();
            _donorStudentService = new DonorStudentService();

            LoadStudents();
            DisplayDonorInfo();
            SetDefaultDates();
            Title = $"Assign Donor: {donor.Name}";
        }

        // New constructor for editing existing assignment
        public AssignDonorWindow(DonorStudentModel assignment)
        {
            InitializeComponent();
            _selectedAssignment = assignment;
            _studentService = new StudentService();
            _donorStudentService = new DonorStudentService();

            LoadStudents();
            DisplayAssignmentInfo();
            SetAssignmentData();
            Title = $"Edit Assignment: {assignment.StudentName} - {assignment.DonorName}";
        }

        private void LoadStudents()
        {
            try
            {
                _students = _studentService.GetAllStudents();
                cmbStudents.ItemsSource = _students;

                if (_students.Count > 0 && _selectedAssignment == null)
                {
                    cmbStudents.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading students: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DisplayDonorInfo()
        {
            if (_selectedDonor != null)
            {
                txtDonorInfo.Text = $"🆔 Donor ID: {_selectedDonor.Id}\n" +
                                   $"👤 Name: {_selectedDonor.Name}\n" +
                                   $"📞 Phone: {_selectedDonor.ContactNumber}\n" + // Changed from Phone to ContactNumber
                                   $"📧 Email: {_selectedDonor.Email}\n" +
                                   $"🏠 Address: {_selectedDonor.Address}";
            }
        }

        private void DisplayAssignmentInfo()
        {
            if (_selectedAssignment != null)
            {
                txtDonorInfo.Text = $"🆔 Assignment Details\n" +
                                   $"👤 Student: {_selectedAssignment.StudentName}\n" +
                                   $"🤝 Donor: {_selectedAssignment.DonorName}\n" +
                                   $"📞 Donor Phone: {_selectedAssignment.DonorPhone}\n" +
                                   $"💰 Current Amount: {_selectedAssignment.Amount:C}";
            }
        }

        private void SetDefaultDates()
        {
            dpStartDate.SelectedDate = DateTime.Today;
            dpEndDate.SelectedDate = DateTime.Today.AddYears(1);
        }

        private void SetAssignmentData()
        {
            if (_selectedAssignment != null)
            {
                // Set selected student
                foreach (var student in _students)
                {
                    if (student.Id == _selectedAssignment.StudentId)
                    {
                        cmbStudents.SelectedItem = student;
                        break;
                    }
                }

                // Set amount
                txtAmount.Text = _selectedAssignment.Amount.ToString();

                // Set dates
                if (DateTime.TryParse(_selectedAssignment.StartDate, out DateTime startDate))
                    dpStartDate.SelectedDate = startDate;

                if (DateTime.TryParse(_selectedAssignment.EndDate, out DateTime endDate))
                    dpEndDate.SelectedDate = endDate;
                else
                    dpEndDate.SelectedDate = null;

                // Set notes
                txtNote.Text = _selectedAssignment.Note;

                // Disable student selection for editing
                cmbStudents.IsEnabled = false;
            }
        }

        private void BtnAssign_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm())
                return;

            try
            {
                if (_selectedAssignment != null)
                {
                    // Update existing assignment
                    UpdateExistingAssignment();
                }
                else
                {
                    // Create new assignment
                    CreateNewAssignment();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving assignment: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CreateNewAssignment()
        {
            var selectedStudent = cmbStudents.SelectedItem as StudentModel;

            if (selectedStudent == null)
            {
                MessageBox.Show("Please select a student!", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var assignment = new DonorStudentModel
            {
                StudentId = selectedStudent.Id,
                DonorId = _selectedDonor.Id,
                Amount = decimal.Parse(txtAmount.Text),
                StartDate = dpStartDate.SelectedDate?.ToString("yyyy-MM-dd") ?? DateTime.Today.ToString("yyyy-MM-dd"),
                EndDate = dpEndDate.SelectedDate?.ToString("yyyy-MM-dd"),
                Note = txtNote.Text.Trim()
            };

            bool assigned = _donorStudentService.AssignDonorToStudent(assignment);

            if (assigned)
            {
                MessageBox.Show($"✅ Successfully assigned donor to student!\n\n" +
                              $"Donor: {_selectedDonor.Name}\n" +
                              $"Student: {selectedStudent.Name}\n" +
                              $"Amount: {assignment.Amount:C}",
                    "Assignment Successful", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
        }

        private void UpdateExistingAssignment()
        {
            var assignment = new DonorStudentModel
            {
                StudentId = _selectedAssignment.StudentId,
                DonorId = _selectedAssignment.DonorId,
                Amount = decimal.Parse(txtAmount.Text),
                StartDate = dpStartDate.SelectedDate?.ToString("yyyy-MM-dd") ?? DateTime.Today.ToString("yyyy-MM-dd"),
                EndDate = dpEndDate.SelectedDate?.ToString("yyyy-MM-dd"),
                Note = txtNote.Text.Trim()
            };

            bool updated = _donorStudentService.UpdateAssignment(assignment);

            if (updated)
            {
                MessageBox.Show($"✅ Successfully updated assignment!\n\n" +
                              $"Student: {_selectedAssignment.StudentName}\n" +
                              $"Donor: {_selectedAssignment.DonorName}\n" +
                              $"New Amount: {assignment.Amount:C}",
                    "Update Successful", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
        }

        private bool ValidateForm()
        {
            // Validate Student Selection (only for new assignments)
            if (_selectedAssignment == null && cmbStudents.SelectedItem == null)
            {
                MessageBox.Show("Please select a student!", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                cmbStudents.Focus();
                return false;
            }

            // Validate Amount
            if (string.IsNullOrWhiteSpace(txtAmount.Text) || !decimal.TryParse(txtAmount.Text, out decimal amount) || amount <= 0)
            {
                MessageBox.Show("Please enter a valid amount greater than 0!", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtAmount.Focus();
                txtAmount.SelectAll();
                return false;
            }

            // Validate Start Date
            if (dpStartDate.SelectedDate == null)
            {
                MessageBox.Show("Please select a start date!", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                dpStartDate.Focus();
                return false;
            }

            return true;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}