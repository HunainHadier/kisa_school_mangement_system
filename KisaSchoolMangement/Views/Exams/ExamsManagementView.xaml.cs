using KisaSchoolMangement.Models;
using KisaSchoolMangement.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace KisaSchoolMangement.Views.Exams
{
    public partial class ExamsManagementView : Window
    {
        private readonly ExamService _examService;
        private readonly StudentService _studentService;
        private readonly ExamSubjectService _examSubjectService;
        private readonly StudentMarkService _studentMarkService;

        private ObservableCollection<ExamModel> _exams;
        private ObservableCollection<StudentMarkModel> _studentMarks;
        private ExamModel _selectedExam;
        private readonly MonthlyTestService _monthlyTestService;
        private ObservableCollection<MonthlyTestModel> _monthlyTests;
        public ExamsManagementView()
        {
            InitializeComponent();
            _examService = new ExamService();
            _studentService = new StudentService();
            _examSubjectService = new ExamSubjectService();
            _studentMarkService = new StudentMarkService();
            _monthlyTestService = new MonthlyTestService();

            LoadExams();

            cmbMonthlyExams.ItemsSource = _exams;
        }


        private void LoadMonthlyStudents(int examId)
        {
            try
            {
                var selectedExam = _exams.FirstOrDefault(e => e.Id == examId);
                if (selectedExam == null) return;

                var allStudents = _studentService.GetAllStudents();
                var classStudents = allStudents.Where(s => s.ClassId == selectedExam.ClassId).ToList();

                var studentList = new ObservableCollection<StudentMarkModel>();
                foreach (var student in classStudents)
                {
                    studentList.Add(new StudentMarkModel
                    {
                        StudentId = student.Id,
                        StudentName = student.FullName,
                        //StudentAdmissionNo = student.AdmissionNo,
                        StudentClassName = student.ClassName
                    });
                }

                cmbMonthlyStudents.ItemsSource = studentList;
                cmbMonthlyStudents.DisplayMemberPath = "StudentName";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading students for monthly tests: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CmbMonthlyStudents_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedStudent = cmbMonthlyStudents.SelectedItem as StudentMarkModel;
            if (selectedStudent != null)
            {
                // Agar koi specific student selection ka logic hai toh yahan add karein
            }
        }

        private void LoadExams()
        {
            try
            {
                _exams = _examService.GetAllExams();
                cmbExams.ItemsSource = _exams;
                cmbExamsReport.ItemsSource = _exams;
                dgExams.ItemsSource = _exams;

                txtStatus.Text = $"Loaded {_exams.Count} exams";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading exams: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CmbExams_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedExam = cmbExams.SelectedItem as ExamModel;
            if (_selectedExam != null)
            {
                txtStatus.Text = $"Selected exam: {_selectedExam.Name}";
            }
        }

        private void BtnLoadStudents_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedExam == null)
            {
                MessageBox.Show("Please select an exam first!", "Warning",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                LoadStudentMarksData();
                txtStatus.Text = $"Loaded marks data for {_selectedExam.Name}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading student marks: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadStudentMarksData()
        {
            if (_selectedExam == null) return;

            try
            {
                // Get students for the selected exam's class
                var allStudents = _studentService.GetAllStudents();
                var classStudents = allStudents.Where(s => s.ClassId == _selectedExam.ClassId).ToList();

                // Get subjects for the exam
                var examSubjects = _examSubjectService.GetSubjectsByExam(_selectedExam.Id);

                // Agar koi subject nahi hai toh message show karein
                if (examSubjects == null || examSubjects.Count == 0)
                {
                    MessageBox.Show($"No subjects found for {_selectedExam.Name}. Please add subjects first using 'Manage Subjects' button.",
                        "No Subjects",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                // Get existing marks
                var existingMarks = _studentMarkService.GetMarksByExam(_selectedExam.Id);

                // Create marks grid data
                _studentMarks = new ObservableCollection<StudentMarkModel>();

                foreach (var student in classStudents)
                {
                    foreach (var subject in examSubjects)
                    {
                        var existingMark = existingMarks?.FirstOrDefault(m =>
                            m.StudentId == student.Id && m.SubjectId == subject.SubjectId);

                        var studentMark = new StudentMarkModel
                        {
                            StudentId = student.Id,
                            ExamId = _selectedExam.Id,
                            SubjectId = subject.SubjectId,
                            StudentName = student.FullName,
                            //StudentAdmissionNo = student.AdmissionNo,
                            StudentClassName = student.ClassName,
                            SubjectName = subject.SubjectName,
                            ExamName = _selectedExam.Name,
                            MaxMarks = subject.MaxMarks,
                            PassMarks = subject.PassMarks,
                            MarksObtained = existingMark?.MarksObtained ?? 0,
                            Grade = existingMark?.Grade,
                            Remarks = existingMark?.Remarks
                        };

                        // ✅ YEH LINES HATA DO - Percentage aur Status automatically calculate ho jayenge
                        // studentMark.Percentage = studentMark.MaxMarks > 0 ?
                        //     (studentMark.MarksObtained / studentMark.MaxMarks) * 100 : 0;
                        // studentMark.Status = studentMark.MarksObtained >= studentMark.PassMarks ? "Pass" : "Fail";

                        _studentMarks.Add(studentMark);
                    }
                }

                dgMarks.ItemsSource = _studentMarks;

                // Update counts
                txtStudentCount.Text = $"Students: {classStudents.Count}";
                txtSubjectCount.Text = $"Subjects: {examSubjects.Count}";

                txtStatus.Text = $"Loaded {_studentMarks.Count} marks entries for {_selectedExam.Name}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading student marks: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnSaveMarks_Click(object sender, RoutedEventArgs e)
        {
            if (_studentMarks == null || _studentMarks.Count == 0)
            {
                MessageBox.Show("No marks data to save!", "Warning",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Calculate grades before saving
                foreach (var mark in _studentMarks)
                {
                    mark.Grade = CalculateGrade(mark.MarksObtained, mark.MaxMarks);
                }

                bool success = _studentMarkService.SaveBulkMarks(_studentMarks);

                if (success)
                {
                    MessageBox.Show($"✅ Successfully saved marks for {_studentMarks.Count} entries!", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    txtStatus.Text = $"Marks saved successfully for {_selectedExam.Name}";
                }
                else
                {
                    MessageBox.Show("Failed to save marks. Please try again.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving marks: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string CalculateGrade(decimal marksObtained, decimal maxMarks)
        {
            if (maxMarks == 0) return "N/A";

            decimal percentage = (marksObtained / maxMarks) * 100;

            if (percentage >= 90) return "A+";
            if (percentage >= 80) return "A";
            if (percentage >= 70) return "B";
            if (percentage >= 60) return "C";
            if (percentage >= 50) return "D";
            if (percentage >= 40) return "E";
            return "F";
        }

        private void BtnNewExam_Click(object sender, RoutedEventArgs e)
        {
            var addExamWindow = new AddExamWindow();
            addExamWindow.Owner = this;
            if (addExamWindow.ShowDialog() == true)
            {
                LoadExams();
            }
        }

        private void BtnManageSubjects_Click(object sender, RoutedEventArgs e)
        {
            // ✅ FORCE SELECTION FROM DATAGRID
            if (_selectedExam == null && dgExams.SelectedItem != null)
            {
                _selectedExam = dgExams.SelectedItem as ExamModel;
            }

            if (_selectedExam == null)
            {
                MessageBox.Show("Please select an exam first!\n\nHow to select: Click on any exam row in the list above.",
                    "No Exam Selected",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            try
            {
                var manageSubjectsWindow = new ManageExamSubjectsWindow(_selectedExam);
                manageSubjectsWindow.Owner = this;
                manageSubjectsWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                manageSubjectsWindow.ShowDialog();

                LoadStudentMarksData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Manage Subjects: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void DgExams_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedExam = dgExams.SelectedItem as ExamModel;

            // Debugging
            if (_selectedExam != null)
            {
                MessageBox.Show($"Exam Selected: {_selectedExam.Name} (ID: {_selectedExam.Id})",
                    "Selection Debug", MessageBoxButton.OK, MessageBoxImage.Information);

                txtStatus.Text = $"Selected exam: {_selectedExam.Name}";

                // ComboBox mein bhi selection update karein
                cmbExams.SelectedItem = _exams.FirstOrDefault(ex => ex.Id == _selectedExam.Id);
            }
            else
            {
                MessageBox.Show("No exam selected or selection is null",
                    "Selection Debug", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
       
        private void BtnRefreshExams_Click(object sender, RoutedEventArgs e)
        {
            LoadExams();
        }

        private void DgExams_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _selectedExam = dgExams.SelectedItem as ExamModel;
            if (_selectedExam != null)
            {
                // Automatically open Manage Subjects on double-click
                BtnManageSubjects_Click(sender, e);
            }
        }

        private void BtnGenerateReport_Click(object sender, RoutedEventArgs e)
        {
            var selectedStudentMark = cmbStudentsReport.SelectedItem as StudentMarkModel;
            var selectedExam = cmbExamsReport.SelectedItem as ExamModel;

            if (selectedStudentMark == null || selectedExam == null)
            {
                MessageBox.Show("Please select both exam and student!", "Warning",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            GenerateReportCard(selectedStudentMark, selectedExam);
        }

        private void GenerateReportCard(StudentMarkModel student, ExamModel exam)
        {
            try
            {
                // Get all marks for this student and exam
                var allMarks = _studentMarkService.GetMarksByExam(exam.Id)
                    .Where(m => m.StudentId == student.StudentId).ToList();

                // Agar koi marks nahi hai toh warning show karein
                if (!allMarks.Any())
                {
                    MessageBox.Show($"No marks found for {student.StudentName} in {exam.Name}!\n\nPlease enter marks first in 'Enter Marks' tab.",
                        "No Marks Found",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                // Clear previous report
                spReportPreview.Children.Clear();

                // Generate report card UI
                GenerateReportCardUI(allMarks, student, exam);

                txtStatus.Text = $"Generated report card for {student.StudentName}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating report: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void GenerateReportCardUI(System.Collections.Generic.List<StudentMarkModel> marks, StudentMarkModel student, ExamModel exam)
        {
            // Convert List to ObservableCollection for the UI
            var marksCollection = new ObservableCollection<StudentMarkModel>(marks);

            // Header
            var headerStack = new StackPanel { Margin = new Thickness(0, 0, 0, 20) };

            var schoolName = new TextBlock
            {
                Text = "KISA SCHOOL",
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = System.Windows.Media.Brushes.DarkBlue
            };

            var reportTitle = new TextBlock
            {
                Text = "REPORT CARD",
                FontSize = 18,
                FontWeight = FontWeights.SemiBold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 10, 0, 0)
            };

            headerStack.Children.Add(schoolName);
            headerStack.Children.Add(reportTitle);
            spReportPreview.Children.Add(headerStack);

            // Student Information
            var studentInfoGrid = new Grid { Margin = new Thickness(0, 0, 0, 20) };
            studentInfoGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            studentInfoGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var leftStack = new StackPanel();
            leftStack.Children.Add(CreateInfoText("Student Name:", student.StudentName));
            leftStack.Children.Add(CreateInfoText("Admission No:", student.StudentAdmissionNo));
            leftStack.Children.Add(CreateInfoText("Class:", student.StudentClassName));

            var rightStack = new StackPanel();
            rightStack.Children.Add(CreateInfoText("Exam:", exam.Name));
            rightStack.Children.Add(CreateInfoText("Academic Year:", exam.AcademicYear));
            rightStack.Children.Add(CreateInfoText("Date:", DateTime.Now.ToString("dd/MM/yyyy")));

            Grid.SetColumn(leftStack, 0);
            Grid.SetColumn(rightStack, 1);
            studentInfoGrid.Children.Add(leftStack);
            studentInfoGrid.Children.Add(rightStack);

            spReportPreview.Children.Add(studentInfoGrid);

            // Marks Table
            var marksGrid = new Grid { Margin = new Thickness(0, 0, 0, 20) };
            marksGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
            marksGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            marksGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Fixed: GridLength instead of Gridlength
            marksGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            marksGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Header Row
            AddTextToGrid(marksGrid, "Subject", 0, 0, true);
            AddTextToGrid(marksGrid, "Max Marks", 1, 0, true);
            AddTextToGrid(marksGrid, "Marks Obtained", 2, 0, true);
            AddTextToGrid(marksGrid, "Grade", 3, 0, true);
            AddTextToGrid(marksGrid, "Status", 4, 0, true);

            // Data Rows
            int row = 1;
            decimal totalMarks = 0;
            decimal totalMaxMarks = 0;

            foreach (var mark in marksCollection)
            {
                AddTextToGrid(marksGrid, mark.SubjectName, 0, row);
                AddTextToGrid(marksGrid, mark.MaxMarks.ToString(), 1, row);
                AddTextToGrid(marksGrid, mark.MarksObtained.ToString(), 2, row);
                AddTextToGrid(marksGrid, mark.Grade, 3, row);
                AddTextToGrid(marksGrid, mark.Status, 4, row);

                totalMarks += mark.MarksObtained;
                totalMaxMarks += mark.MaxMarks;
                row++;
            }

            // Total Row
            AddTextToGrid(marksGrid, "TOTAL", 0, row, true);
            AddTextToGrid(marksGrid, totalMaxMarks.ToString(), 1, row, true);
            AddTextToGrid(marksGrid, totalMarks.ToString(), 2, row, true);
            AddTextToGrid(marksGrid, "", 3, row, true);

            decimal percentage = totalMaxMarks > 0 ? (totalMarks / totalMaxMarks) * 100 : 0;
            string overallGrade = CalculateGrade(totalMarks, totalMaxMarks);
            AddTextToGrid(marksGrid, $"{percentage:F2}% ({overallGrade})", 4, row, true);

            spReportPreview.Children.Add(marksGrid);

            // Remarks
            var remarksText = new TextBlock
            {
                Text = $"Remarks: {GetRemarks(percentage)}",
                FontWeight = FontWeights.SemiBold,
                FontSize = 14,
                Margin = new Thickness(0, 10, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            spReportPreview.Children.Add(remarksText);
        }

        private TextBlock CreateInfoText(string label, string value)
        {
            return new TextBlock
            {
                Text = $"{label} {value}",
                FontSize = 12,
                Margin = new Thickness(0, 2, 0, 0)
            };
        }

        private void AddTextToGrid(Grid grid, string text, int column, int row, bool isHeader = false)
        {
            var textBlock = new TextBlock
            {
                Text = text,
                FontWeight = isHeader ? FontWeights.Bold : FontWeights.Normal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(5),
                Background = isHeader ? System.Windows.Media.Brushes.LightGray : System.Windows.Media.Brushes.Transparent
            };

            Grid.SetColumn(textBlock, column);
            Grid.SetRow(textBlock, row);
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.Children.Add(textBlock);
        }

        private string GetRemarks(decimal percentage)
        {
            if (percentage >= 90) return "Outstanding! Excellent performance.";
            if (percentage >= 80) return "Very Good! Keep it up.";
            if (percentage >= 70) return "Good performance.";
            if (percentage >= 60) return "Satisfactory. Can do better.";
            if (percentage >= 50) return "Needs improvement.";
            return "Needs serious attention and hard work.";
        }

        private void CmbExamsReport_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedExam = cmbExamsReport.SelectedItem as ExamModel;
            if (selectedExam != null)
            {
                LoadStudentsForReport(selectedExam.Id);
            }
        }

      
        private void LoadStudentsForReport(int examId)
        {
            try
            {
                // Debugging
                Console.WriteLine($"Loading students for exam ID: {examId}");

                var selectedExam = _exams.FirstOrDefault(e => e.Id == examId);
                if (selectedExam == null)
                {
                    MessageBox.Show("Selected exam not found!", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var allStudents = _studentService.GetAllStudents();
                Console.WriteLine($"Total students in system: {allStudents.Count}");

                var classStudents = allStudents.Where(s => s.ClassId == selectedExam.ClassId).ToList();
                Console.WriteLine($"Students in class {selectedExam.ClassId}: {classStudents.Count}");

                // Create StudentMarkModel objects for display
                var studentList = new ObservableCollection<StudentMarkModel>();

                foreach (var student in classStudents)
                {
                    studentList.Add(new StudentMarkModel
                    {
                        StudentId = student.Id,
                        StudentName = student.FullName,
                        //StudentAdmissionNo = student.AdmissionNo,
                        StudentClassName = student.ClassName
                    });
                }

                cmbStudentsReport.ItemsSource = studentList;
                cmbStudentsReport.DisplayMemberPath = "StudentName";

                // Update status
                txtStatus.Text = $"Loaded {studentList.Count} students for report generation";

                // Debugging
                Console.WriteLine($"Students loaded in combobox: {studentList.Count}");

                if (studentList.Count == 0)
                {
                    MessageBox.Show($"No students found in class {selectedExam.ClassName} for this exam.",
                        "No Students", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading students for report: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void BtnPrintReport_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Print functionality will be implemented soon!", "Coming Soon",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnExportPdf_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("PDF export functionality will be implemented soon!", "Coming Soon",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnPrintAll_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Bulk print functionality will be implemented soon!", "Coming Soon",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CmbMonthlyExams_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedExam = cmbMonthlyExams.SelectedItem as ExamModel;
            if (selectedExam != null)
            {
                LoadMonthlyStudents(selectedExam.Id);
            }
        }

        private void BtnLoadMonthlyTests_Click(object sender, RoutedEventArgs e)
        {
            var selectedExam = cmbMonthlyExams.SelectedItem as ExamModel;
            var selectedMonthItem = cmbMonths.SelectedItem as ComboBoxItem;

            if (selectedExam == null || selectedMonthItem == null)
            {
                MessageBox.Show("Please select both exam and month!", "Warning",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                int month = Convert.ToInt32(selectedMonthItem.Tag);
                LoadMonthlyTestsData(selectedExam.Id, month);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading monthly tests: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadMonthlyTestsData(int examId, int month)
        {
            try
            {
                var selectedExam = _exams.FirstOrDefault(e => e.Id == examId);
                if (selectedExam == null) return;

                // Database se existing tests load karein
                _monthlyTests = _monthlyTestService.GetMonthlyTestsByExam(examId);

                // Filter by selected month
                var filteredTests = _monthlyTests.Where(t => t.Month == month).ToList();

                // Agar koi test nahi hai toh create karein
                if (!filteredTests.Any())
                {
                    CreateMonthlyTestsForMonth(examId, month);
                }
                else
                {
                    dgMonthlyTests.ItemsSource = filteredTests;
                }

                txtStatus.Text = $"Loaded {filteredTests.Count} monthly tests for {GetMonthName(month)}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading monthly tests data: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CreateMonthlyTestsForMonth(int examId, int month)
        {
            try
            {
                // Get students and subjects for the exam
                var exam = _exams.FirstOrDefault(e => e.Id == examId);
                if (exam == null) return;

                var students = _studentService.GetAllStudents()
                    .Where(s => s.ClassId == exam.ClassId).ToList();
                var subjects = _examSubjectService.GetSubjectsByExam(examId);

                var monthlyTests = new ObservableCollection<MonthlyTestModel>();

                foreach (var student in students)
                {
                    foreach (var subject in subjects)
                    {
                        monthlyTests.Add(new MonthlyTestModel
                        {
                            ExamId = examId,
                            SubjectId = subject.SubjectId,
                            StudentId = student.Id,
                            Month = month,
                            MaxMarks = 50, // Default monthly test marks
                            ObtainedMarks = 0,
                            TestDate = DateTime.Now,
                            SubjectName = subject.SubjectName,
                            StudentName = student.FullName,
                            MonthName = GetMonthName(month)
                        });
                    }
                }

                dgMonthlyTests.ItemsSource = monthlyTests;
                _monthlyTests = monthlyTests;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating monthly tests: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GetMonthName(int month)
        {
            return month switch
            {
                1 => "January",
                2 => "February",
                3 => "March",
                4 => "April",
                5 => "May",
                6 => "June",
                7 => "July",
                8 => "August",
                9 => "September",
                10 => "October",
                11 => "November",
                12 => "December",
                _ => "Unknown"
            };
        }

        private void BtnSaveMonthlyTests_Click(object sender, RoutedEventArgs e)
        {
            if (_monthlyTests == null || !_monthlyTests.Any())
            {
                MessageBox.Show("No monthly test data to save!", "Warning",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                bool success = true;
                foreach (var test in _monthlyTests)
                {
                    if (!_monthlyTestService.SaveMonthlyTest(test))
                    {
                        success = false;
                        break;
                    }
                }

                if (success)
                {
                    MessageBox.Show("Monthly tests saved successfully!", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    txtStatus.Text = "Monthly tests saved";
                }
                else
                {
                    MessageBox.Show("Failed to save some monthly tests.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving monthly tests: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void CheckStudentService()
        {
            try
            {
                var students = _studentService.GetAllStudents();
                MessageBox.Show($"Student Service Check: {students.Count} students found in system.",
                    "Service Check", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Student Service Error: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}