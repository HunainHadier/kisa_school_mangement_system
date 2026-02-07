using KisaSchoolMangement.Services;
using System.Windows;

namespace KisaSchoolMangement.Views.User
{
    public partial class ResetPasswordWindow : Window
    {
        private readonly int _userId;
        private readonly string _userFullName;
        private readonly UserService _userService;

        public ResetPasswordWindow(int userId, string userFullName)
        {
            InitializeComponent();
            _userId = userId;
            _userFullName = userFullName;
            _userService = new UserService();

            // Set user info
            this.DataContext = new { UserInfo = $"Reset password for: {userFullName}" };
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            string newPassword = txtNewPassword.Password;
            string confirmPassword = txtConfirmPassword.Password;

            // Validation
            if (string.IsNullOrEmpty(newPassword))
            {
                MessageBox.Show("Please enter new password", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (newPassword != confirmPassword)
            {
                MessageBox.Show("Passwords do not match", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (newPassword.Length < 6)
            {
                MessageBox.Show("Password must be at least 6 characters", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Reset password
            bool success = _userService.ResetPassword(_userId, newPassword, 1); // Assuming owner is resetting

            if (success)
            {
                DialogResult = true;
                Close();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}