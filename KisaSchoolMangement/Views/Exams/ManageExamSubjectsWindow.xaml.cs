using KisaSchoolMangement.Models;
using KisaSchoolMangement.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace KisaSchoolMangement.Views.Exams
{
    public partial class ManageExamSubjectsWindow : Window
    {
        private readonly ExamModel _exam;
        private readonly ExamSubjectService _examSubjectService;
        private readonly SubjectService _subjectService;
        private ObservableCollection<ExamSubjectModel> _examSubjects;
        private ObservableCollection<SubjectModel> _allSubjects;

        public ManageExamSubjectsWindow(ExamModel exam)
        {
            InitializeComponent();
            _exam = exam;
            _examSubjectService = new ExamSubjectService();
            _subjectService = new SubjectService();

            InitializeWindow();
            LoadData();
        }

        private void InitializeWindow()
        {
            txtHeader.Text = $"Manage Subjects - {_exam.Name}";
            txtExamInfo.Text = $"Class: {_exam.ClassName} | Academic Year: {_exam.AcademicYear}";
        }

        private void LoadData()
        {
            try
            {
                // Load all subjects
                _allSubjects = _examSubjectService.GetAllSubjects(); // ✅ Yahan change kiya
                cmbSubjects.ItemsSource = _allSubjects;

                // Load exam subjects
                _examSubjects = _examSubjectService.GetSubjectsByExam(_exam.Id);

                // Agar koi subject nahi hai toh empty list show karein
                if (_examSubjects == null)
                    _examSubjects = new ObservableCollection<ExamSubjectModel>();

                dgExamSubjects.ItemsSource = _examSubjects;

                if (_allSubjects.Count > 0)
                    cmbSubjects.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnAddSubject_Click(object sender, RoutedEventArgs e)
        {
            var selectedSubject = cmbSubjects.SelectedItem as SubjectModel;
            if (selectedSubject == null)
            {
                MessageBox.Show("Please select a subject!", "Warning",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(txtMaxMarks.Text, out decimal maxMarks) || maxMarks <= 0)
            {
                MessageBox.Show("Please enter valid max marks!", "Warning",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(txtPassMarks.Text, out decimal passMarks) || passMarks <= 0)
            {
                MessageBox.Show("Please enter valid pass marks!", "Warning",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Check if subject already added
            if (_examSubjects.Any(es => es.SubjectId == selectedSubject.Id))
            {
                MessageBox.Show("This subject is already added to the exam!", "Warning",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Add to local collection
            var newExamSubject = new ExamSubjectModel
            {
                ExamId = _exam.Id,
                SubjectId = selectedSubject.Id,
                MaxMarks = maxMarks,
                PassMarks = passMarks,
                SubjectName = selectedSubject.Name,
                SubjectCode = selectedSubject.Code
            };

            _examSubjects.Add(newExamSubject);
            RefreshDataGrid();

            // Auto-save when adding
            SaveToDatabase();
        }

        private void BtnRemoveSubject_Click(object sender, RoutedEventArgs e)
        {
            var selectedSubject = dgExamSubjects.SelectedItem as ExamSubjectModel;
            if (selectedSubject == null)
            {
                MessageBox.Show("Please select a subject to remove!", "Warning",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Are you sure you want to remove {selectedSubject.SubjectName} from this exam?",
                "Confirm Removal",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Remove from database first
                bool removedFromDb = _examSubjectService.RemoveExamSubject(_exam.Id, selectedSubject.SubjectId);

                if (removedFromDb)
                {
                    _examSubjects.Remove(selectedSubject);
                    RefreshDataGrid();
                    MessageBox.Show("Subject removed successfully!", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Failed to remove subject from database.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void RefreshDataGrid()
        {
            dgExamSubjects.ItemsSource = null;
            dgExamSubjects.ItemsSource = _examSubjects;
        }

        private void SaveToDatabase()
        {
            try
            {
                bool success = _examSubjectService.SaveExamSubjects(_exam.Id, _examSubjects.ToList());

                if (success)
                {
                    txtExamInfo.Text = $"Class: {_exam.ClassName} | Academic Year: {_exam.AcademicYear} | Saved: {DateTime.Now:HH:mm:ss}";
                    MessageBox.Show("Subjects saved successfully!", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Failed to save subjects to database.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving subjects: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveToDatabase();
                MessageBox.Show($"Subjects configuration saved for {_exam.Name}!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving subjects: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}