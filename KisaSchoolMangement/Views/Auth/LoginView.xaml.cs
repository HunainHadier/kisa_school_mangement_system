using KisaSchoolMangement.Services;
using KisaSchoolMangement.Views.Dashboard;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace KisaSchoolMangement.Views.Auth
{
    public partial class LoginView : Window
    {
        private readonly AuthService _authService;

        public LoginView()
        {
            InitializeComponent();
            _authService = new AuthService();
            InitializePlaceholderBehavior();

            // Database connection check
            if (!_authService.TestConnection())
            {
                MessageBox.Show("Database connection failed. Please check your connection.",
                    "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            // Create initial user if not exists
            _authService.CreateInitialUser();
        }

        private void InitializePlaceholderBehavior()
        {
            // Username placeholder
            txtUsername.Text = "Enter your username";
            txtUsername.Foreground = Brushes.Gray;

            txtUsername.GotFocus += (s, e) =>
            {
                if (txtUsername.Text == "Enter your username")
                {
                    txtUsername.Text = "";
                    txtUsername.Foreground = Brushes.Black;
                }
            };

            txtUsername.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtUsername.Text))
                {
                    txtUsername.Text = "Enter your username";
                    txtUsername.Foreground = Brushes.Gray;
                }
            };
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Clear previous messages
                lblMessage.Visibility = Visibility.Collapsed;

                // Get credentials
                string username = txtUsername.Text;
                string password = txtPassword.Password;

                // Clear placeholder before validation
                if (username == "Enter your username")
                    username = "";

                // Validation
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    ShowMessage("Please enter both username and password", true);
                    return;
                }

                // Show loading state
                LoginButton.Content = "Signing In...";
                LoginButton.IsEnabled = false;
                Mouse.OverrideCursor = Cursors.Wait;

                // Authenticate user
                var result = _authService.Authenticate(username, password);

                if (result.Success)
                {
                    ShowMessage("Login successful! Redirecting...", false);

                    // Small delay for user to see success message
                    await System.Threading.Tasks.Task.Delay(1000);

                    // Create dashboard with user info
                    var dashboard = new DashboardView(result.User);
                    dashboard.Show();

                    // Close login window
                    this.Close();
                }
                else
                {
                    ShowMessage(result.Message, true);
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Login error: {ex.Message}", true);
                System.Diagnostics.Debug.WriteLine($"Login Error: {ex}");
            }
            finally
            {
                // Reset button state
                LoginButton.Content = "Sign In";
                LoginButton.IsEnabled = true;
                Mouse.OverrideCursor = null;
            }
        }

        private void ShowMessage(string message, bool isError = true)
        {
            lblMessage.Text = message;
            lblMessage.Foreground = isError ? new SolidColorBrush(Color.FromRgb(231, 76, 60)) : new SolidColorBrush(Color.FromRgb(46, 204, 113));
            lblMessage.Visibility = Visibility.Visible;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // Enable window dragging
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }

        // Enter key support for login
        private void txtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                LoginButton_Click(sender, e);
            }
        }
    }
}