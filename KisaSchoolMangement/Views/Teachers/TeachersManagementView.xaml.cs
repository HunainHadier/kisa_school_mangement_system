using KisaSchoolMangement.Models;
using KisaSchoolMangement.Services;
using KisaSchoolMangement.Views.Teachers;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace KisaSchoolMangement.Views.Teachers
{
    public partial class TeachersManagementView : Window
    {
        private readonly TeacherService _teacherService;
        private ObservableCollection<TeacherModel> _teachers;
        private ObservableCollection<TeacherModel> _filteredTeachers;
        private TeacherModel _selectedTeacher;

        public TeachersManagementView()
        {
            InitializeComponent();
            _teacherService = new TeacherService();
            LoadTeachers();
        }

        private void LoadTeachers()
        {
            try
            {
                _teachers = _teacherService.GetAllTeachers();
                _filteredTeachers = new ObservableCollection<TeacherModel>(_teachers);
                dgTeachers.ItemsSource = _filteredTeachers;
                UpdateTeachersCount();
                txtStatus.Text = $"Loaded {_teachers.Count} teachers";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading teachers: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateTeachersCount()
        {
            txtTeachersCount.Text = $"📊 Total Teachers: {_filteredTeachers.Count}";
        }

        private void BtnAddTeacher_Click(object sender, RoutedEventArgs e)
        {
            var addTeacherWindow = new AddEditTeacherWindow();
            addTeacherWindow.Owner = this;
            if (addTeacherWindow.ShowDialog() == true)
            {
                LoadTeachers();
            }
        }

        private void BtnEditTeacher_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var teacher = button?.DataContext as TeacherModel;
            if (teacher != null)
            {
                var editTeacherWindow = new AddEditTeacherWindow(teacher);
                editTeacherWindow.Owner = this;
                if (editTeacherWindow.ShowDialog() == true)
                {
                    LoadTeachers();
                }
            }
        }

        private void BtnViewTeacher_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var teacher = button?.DataContext as TeacherModel;
            if (teacher != null)
            {
                // Show teacher details window
                MessageBox.Show($"Viewing details for: {teacher.FullName}", "Teacher Details",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnDeleteTeacher_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var teacher = button?.DataContext as TeacherModel;
            if (teacher != null)
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to delete {teacher.FullName}? This action cannot be undone.",
                    "Confirm Deletion",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    if (_teacherService.DeleteTeacher(teacher.Id))
                    {
                        MessageBox.Show("Teacher deleted successfully!", "Success",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadTeachers();
                    }
                    else
                    {
                        MessageBox.Show("Failed to delete teacher.", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void DgTeachers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedTeacher = dgTeachers.SelectedItem as TeacherModel;
        }

        private void TxtSearchTeacher_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterTeachers();
        }

        private void FilterTeachers()
        {
            try
            {
                string searchText = txtSearchTeacher.Text.ToLower();

                if (string.IsNullOrWhiteSpace(searchText))
                {
                    _filteredTeachers = new ObservableCollection<TeacherModel>(_teachers);
                }
                else
                {
                    var filtered = _teachers.Where(t =>
                        t.FullName.ToLower().Contains(searchText) ||
                        t.Email.ToLower().Contains(searchText) ||
                        t.Phone.ToLower().Contains(searchText) ||
                        t.Qualification.ToLower().Contains(searchText)).ToList();

                    _filteredTeachers = new ObservableCollection<TeacherModel>(filtered);
                }

                dgTeachers.ItemsSource = _filteredTeachers;
                UpdateTeachersCount();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error filtering teachers: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadTeachers();
        }

        private void BtnViewAttendance_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Teacher Attendance feature will be implemented soon!", "Coming Soon",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnAssignSubjects_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedTeacher != null)
            {
                var assignWindow = new AssignSubjectsWindow(_selectedTeacher);
                assignWindow.Owner = this;
                assignWindow.ShowDialog();
            }
            else
            {
                MessageBox.Show("Please select a teacher first!", "No Teacher Selected",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}