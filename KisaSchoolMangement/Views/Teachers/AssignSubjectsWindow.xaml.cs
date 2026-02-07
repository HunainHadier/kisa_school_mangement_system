using KisaSchoolMangement.Models;
using KisaSchoolMangement.Services;
using System;
using System.Windows;
using System.Windows.Controls;

namespace KisaSchoolMangement.Views.Teachers
{
    public partial class AssignSubjectsWindow : Window
    {
        private readonly TeacherSubjectService _teacherSubjectService;
        private readonly TeacherModel _teacher;

        public AssignSubjectsWindow(TeacherModel teacher)
        {
            InitializeComponent();
            _teacherSubjectService = new TeacherSubjectService();
            _teacher = teacher;
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                // Set teacher info
                txtTeacherInfo.Text = $"Teacher: {_teacher.FullName} - {_teacher.Email}";

                // Load subjects and classes
                cmbSubjects.ItemsSource = _teacherSubjectService.GetAvailableSubjects();
                cmbClasses.ItemsSource = _teacherSubjectService.GetAvailableClasses();

                // Load current assignments
                RefreshAssignments();

                txtStatus.Text = "Data loaded successfully";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                txtStatus.Text = "Error loading data";
            }
        }

        private void RefreshAssignments()
        {
            try
            {
                var assignments = _teacherSubjectService.GetTeacherAssignments(_teacher.Id);
                dgAssignments.ItemsSource = assignments;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading assignments: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnAssign_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cmbSubjects.SelectedItem == null || cmbClasses.SelectedItem == null)
                {
                    MessageBox.Show("Please select both subject and class", "Warning",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var selectedSubject = cmbSubjects.SelectedItem as SubjectModel;
                var selectedClass = cmbClasses.SelectedItem as ClassModel;

                var assignment = new TeacherSubjectAssignment
                {
                    TeacherId = _teacher.Id,
                    SubjectId = selectedSubject.Id,
                    ClassId = selectedClass.Id,
                    AcademicYear = "2024-2025"
                };

                if (_teacherSubjectService.AssignSubjectToTeacher(assignment))
                {
                    MessageBox.Show("Subject assigned successfully!", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    RefreshAssignments();

                    // Reset selections
                    cmbSubjects.SelectedIndex = -1;
                    cmbClasses.SelectedIndex = -1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error assigning subject: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnRemoveAssignment_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as Button;
                var assignment = button?.DataContext as TeacherSubjectAssignment;

                if (assignment != null)
                {
                    var result = MessageBox.Show(
                        $"Are you sure you want to remove {assignment.SubjectName} from {assignment.ClassName}?",
                        "Confirm Removal",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        if (_teacherSubjectService.RemoveAssignment(assignment.Id))
                        {
                            MessageBox.Show("Assignment removed successfully!", "Success",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                            RefreshAssignments();
                        }
                        else
                        {
                            MessageBox.Show("Failed to remove assignment", "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
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

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}