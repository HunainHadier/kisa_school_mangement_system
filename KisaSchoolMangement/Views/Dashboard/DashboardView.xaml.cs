using KisaSchoolMangement.Services;
using KisaSchoolMangement.ViewModels;
using KisaSchoolMangement.Views.Auth;
using KisaSchoolMangement.Views.Class;
using KisaSchoolMangement.Views.Donors;
using KisaSchoolMangement.Views.Exams;
using KisaSchoolMangement.Views.Section;
using KisaSchoolMangement.Views.Student;
using KisaSchoolMangement.Views.Students;
using KisaSchoolMangement.Views.Teachers;
using KisaSchoolMangement.Views.User;
using System;
using System.Windows;
using System.Windows.Threading;

namespace KisaSchoolMangement.Views.Dashboard
{
    public partial class DashboardView : Window
    {
        private readonly AuthService.User _currentUser;
        private readonly DispatcherTimer _timer;

        // Constructor without parameters (for design time)
        public DashboardView()
        {
            InitializeComponent();
            DataContext = new DashboardViewModel();
        }

        // Constructor with user parameter (for login)
        public DashboardView(AuthService.User user)
        {
            InitializeComponent();
            _currentUser = user;
            DataContext = new DashboardViewModel();

            // Set window title with user info
            this.Title = $"Kisa School - Dashboard ({user.Role})";

            // Display user info in sidebar
            txtUserName.Text = $"Welcome, {user.FullName}";
            txtUserRole.Text = user.Role;

            // Start clock timer
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            _timer.Start();

            UpdateDateTime();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            UpdateDateTime();
        }

        private void UpdateDateTime()
        {
            var now = DateTime.Now;
            txtCurrentTime.Text = now.ToString("hh:mm:ss tt");
            txtCurrentDate.Text = now.ToString("dddd, dd MMMM yyyy");
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Confirm logout
                var result = MessageBox.Show("Are you sure you want to logout?", "Confirm Logout",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // Stop timer
                    if (_timer != null)
                        _timer.Stop();

                    // Show login window
                    var loginWindow = new LoginView();
                    loginWindow.Show();

                    // Close dashboard
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during logout: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Optional: Confirm when closing window
            var result = MessageBox.Show("Are you sure you want to exit?", "Exit Application",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.No)
            {
                e.Cancel = true;
            }
            else
            {
                // Stop timer when closing
                if (_timer != null)
                    _timer.Stop();

                // Show login window again if needed
                // Application.Current.Shutdown(); // Or show login window
            }
        }

        // Update other methods to use current user ID
        private void AddUser_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Use current user ID instead of hardcoded 1
                int currentUserId = _currentUser?.Id ?? 1;

                var addUserWindow = new AddEditUserWindow(currentUserId);
                addUserWindow.Owner = this;
                addUserWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Add User window: {ex.Message}", "Error");
            }
        }

        private void ManageUsers_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Use current user ID instead of hardcoded 1
                int currentUserId = _currentUser?.Id ?? 1;

                var userView = new UserView(currentUserId);
                userView.Owner = this;
                userView.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Manage Users window: {ex.Message}", "Error");
            }
        }

        private void QuickManageUsers_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Quick access from dashboard
                int currentUserId = _currentUser?.Id ?? 1;
                var userView = new UserView(currentUserId);
                userView.Owner = this;
                userView.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Users window: {ex.Message}", "Error");
            }
        }

        // Rest of the methods remain the same...
        private void AddStudent_Click(object sender, RoutedEventArgs e)
        {
            var addStudentWindow = new AddStudentWindow();
            addStudentWindow.Owner = this;
            addStudentWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            addStudentWindow.ShowDialog();
        }

        private void ManageStudents_Click(object sender, RoutedEventArgs e)
        {
            StudentView studentView = new StudentView();
            studentView.Owner = this;
            studentView.Show();
        }

        private void ViewDonerwithStudents_Click(object sender, RoutedEventArgs e)
        {
            StudentsWithDonorsView StudentsWithDonorsView = new StudentsWithDonorsView();
            StudentsWithDonorsView.Owner = this;
            StudentsWithDonorsView.Show();
        }

        private void AddClass_Click(object sender, RoutedEventArgs e)
        {
            var addClassWindow = new AddClassWindow();
            addClassWindow.Owner = this;
            addClassWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            addClassWindow.ShowDialog();
        }

        private void ManageClasses_Click(object sender, RoutedEventArgs e)
        {
            // You can create a ClassesView window later
            MessageBox.Show("Classes management window will be implemented soon!", "Coming Soon",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void AddSection_Click(object sender, RoutedEventArgs e)
        {
            var addSectionWindow = new AddSectionWindow();
            addSectionWindow.Owner = this;
            addSectionWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            addSectionWindow.ShowDialog();
        }

        private void ManageSections_Click(object sender, RoutedEventArgs e)
        {
            // You can create a SectionsView window later
            MessageBox.Show("Sections management window will be implemented soon!", "Coming Soon",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void AddDonor_Click(object sender, RoutedEventArgs e)
        {
            DonorView donorView = new DonorView();
            donorView.Owner = this;
            donorView.Show();
        }

        private void Teachermanage_Click(object sender, RoutedEventArgs e)
        {
            TeachersManagementView teacherView = new TeachersManagementView();
            teacherView.Owner = this;
            teacherView.Show();
        }

        private void ManageDonors_Click(object sender, RoutedEventArgs e)
        {
            DonorView donorView = new DonorView();
            donorView.Owner = this;
            donorView.Show();
        }

        private void CollectFee_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Fee collection feature will be implemented soon!", "Coming Soon",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void MarkAttendance_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Attendance marking feature will be implemented soon!", "Coming Soon",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void EnterMarks_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Exam marks entry feature will be implemented soon!", "Coming Soon",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ManageAssignments_Click(object sender, RoutedEventArgs e)
        {
            var assignmentsView = new DonorStudentAssignmentsView();
            assignmentsView.Owner = this;
            assignmentsView.Show();
        }

        private void AddExam_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var addExamWindow = new AddExamWindow();
                addExamWindow.Owner = this;
                addExamWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                addExamWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Add Exam window: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ManageExams_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var manageExamsWindow = new ExamsManagementView();
                manageExamsWindow.Owner = this;
                manageExamsWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                manageExamsWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Manage Exams window: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ManageExamSubjects_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageBox.Show("Please use 'Manage Exams' window to select an exam first, then click 'Manage Subjects' button to manage exam subjects.",
                    "Info",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                var examsManagementWindow = new ExamsManagementView();
                examsManagementWindow.Owner = this;
                examsManagementWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                examsManagementWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Exam Subjects window: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ManageRoles_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Role Management feature coming soon!", "Info",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ManagePermissions_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Permissions Management feature coming soon!", "Info",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}