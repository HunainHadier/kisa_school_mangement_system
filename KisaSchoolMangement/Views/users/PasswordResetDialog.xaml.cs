using KisaSchoolMangement.Services;
using System.Windows;

namespace KisaSchoolMangement.Views.User
{
    public partial class PasswordResetDialog : Window
    {
        private readonly UserService _userService;
        private readonly int _userId;
        private readonly string _userName;

        public PasswordResetDialog(int userId, string userName)
        {
            InitializeComponent();
            _userService = new UserService();
            _userId = userId;
            _userName = userName;

            InitializeDialog();
        }

        private void InitializeDialog()
        {
            txtUserName.Text = _userName;
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtNewPassword.Password))
            {
                MessageBox.Show("Please enter new password", "Error");
                return;
            }

            if (txtNewPassword.Password != txtConfirmPassword.Password)
            {
                MessageBox.Show("Passwords do not match", "Error");
                return;
            }

            bool success = _userService.ResetPassword(_userId, txtNewPassword.Password, 1);
            if (success)
            {
                MessageBox.Show("Password reset successfully!", "Success");
                this.DialogResult = true;
                this.Close();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}