using KisaSchoolMangement.Models;
using KisaSchoolMangement.Services;
using System;
using System.Windows;
using System.Windows.Controls;

namespace KisaSchoolMangement.Views.Exams
{
    public partial class AddExamWindow : Window
    {
        private readonly ExamService _examService;
        private readonly ClassService _classService;

        public AddExamWindow()
        {
            InitializeComponent();
            _examService = new ExamService();
            _classService = new ClassService();

            LoadClasses();
            SetDefaultDates();
        }

        private void LoadClasses()
        {
            try
            {
                var classes = _classService.GetAllClasses();
                cmbClass.ItemsSource = classes;

                if (classes.Count > 0)
                    cmbClass.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading classes: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetDefaultDates()
        {
            dpStartDate.SelectedDate = DateTime.Today;
            dpEndDate.SelectedDate = DateTime.Today.AddDays(7);
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateForm())
            {
                try
                {
                    var exam = new ExamModel
                    {
                        Name = txtExamName.Text.Trim(),
                        ClassId = (cmbClass.SelectedItem as ClassModel)?.Id ?? 0,
                        ExamTypeId = cmbExamType.SelectedIndex + 1, // Simple mapping
                        StartDate = dpStartDate.SelectedDate?.ToString("yyyy-MM-dd"),
                        EndDate = dpEndDate.SelectedDate?.ToString("yyyy-MM-dd"),
                        AcademicYear = txtAcademicYear.Text.Trim(),
                        IsActive = true
                    };

                    bool success = _examService.AddExam(exam);

                    if (success)
                    {
                        MessageBox.Show("Exam added successfully!", "Success",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        DialogResult = true;
                        Close();
                    }
                    else
                    {
                        MessageBox.Show("Failed to add exam. Please try again.", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error adding exam: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(txtExamName.Text))
            {
                MessageBox.Show("Please enter exam name!", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtExamName.Focus();
                return false;
            }

            if (cmbClass.SelectedItem == null)
            {
                MessageBox.Show("Please select a class!", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                cmbClass.Focus();
                return false;
            }

            if (dpStartDate.SelectedDate == null)
            {
                MessageBox.Show("Please select start date!", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                dpStartDate.Focus();
                return false;
            }

            if (dpEndDate.SelectedDate == null)
            {
                MessageBox.Show("Please select end date!", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                dpEndDate.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtAcademicYear.Text))
            {
                MessageBox.Show("Please enter academic year!", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtAcademicYear.Focus();
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