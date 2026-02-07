using System;
using System.Linq;
using System.Windows;
using KisaSchoolMangement.Models;
using KisaSchoolMangement.Services;

namespace KisaSchoolMangement.Views.User
{
    public partial class AddEditUserWindow : Window
    {
        private readonly UserService _userService;
        private readonly int _currentUserId;
        private readonly int _editUserId;
        private readonly bool _isEditMode;

        // Constructor with 1 parameter (for Add new user)
        public AddEditUserWindow(int currentUserId)
        {
            InitializeComponent();
            _userService = new UserService();
            _currentUserId = currentUserId;
            _editUserId = 0;
            _isEditMode = false;

            InitializeWindow();
            LoadRoles();
            SetTitle("Add New User");
        }

        // Constructor with 2 parameters (for Edit user)
        public AddEditUserWindow(int currentUserId, int userId)
        {
            InitializeComponent();
            _userService = new UserService();
            _currentUserId = currentUserId;
            _editUserId = userId;
            _isEditMode = true;

            InitializeWindow();
            LoadRoles();
            SetTitle("Edit User");
            LoadUserData();
        }

        private void SetTitle(string title)
        {
            txtTitle.Text = title;
            this.Title = title;
        }

        private void InitializeWindow()
        {
            if (_isEditMode)
            {
                // Edit mode - hide password fields
                txtPassword.Visibility = Visibility.Collapsed;
                txtConfirmPassword.Visibility = Visibility.Collapsed;
                lblPassword.Visibility = Visibility.Collapsed;
                lblConfirmPassword.Visibility = Visibility.Collapsed;
            }
            else
            {
                // Add mode - show password fields
                txtPassword.Visibility = Visibility.Visible;
                txtConfirmPassword.Visibility = Visibility.Visible;
                lblPassword.Visibility = Visibility.Visible;
                lblConfirmPassword.Visibility = Visibility.Visible;
            }
        }

        private void LoadRoles()
        {
            try
            {
                var roles = _userService.GetAllRoles();
                cmbRole.ItemsSource = roles;
                cmbRole.DisplayMemberPath = "Name";
                cmbRole.SelectedValuePath = "Id";

                if (roles.Count > 0)
                    cmbRole.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading roles: {ex.Message}", "Error");
            }
        }

        private void LoadUserData()
        {
            try
            {
                var user = _userService.GetUserById(_editUserId);
                if (user != null)
                {
                    txtUsername.Text = user.Username;
                    txtFullName.Text = user.FullName;
                    txtEmail.Text = user.Email;
                    txtPhone.Text = user.Phone;
                    chkIsActive.IsChecked = user.IsActive;

                    // Select role
                    if (cmbRole.ItemsSource != null)
                    {
                        foreach (var role in cmbRole.Items)
                        {
                            if (role is RoleModel roleModel && roleModel.Id == user.RoleId)
                            {
                                cmbRole.SelectedItem = role;
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading user data: {ex.Message}", "Error");
            }
        }

        private bool ValidateInput()
        {
            // Username validation
            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                MessageBox.Show("Username is required!", "Validation Error");
                txtUsername.Focus();
                return false;
            }

            // Password validation for new user only
            if (!_isEditMode)
            {
                if (string.IsNullOrEmpty(txtPassword.Password))
                {
                    MessageBox.Show("Password is required!", "Validation Error");
                    txtPassword.Focus();
                    return false;
                }

                if (txtPassword.Password.Length < 6)
                {
                    MessageBox.Show("Password must be at least 6 characters!", "Validation Error");
                    txtPassword.Focus();
                    return false;
                }

                if (txtPassword.Password != txtConfirmPassword.Password)
                {
                    MessageBox.Show("Passwords do not match!", "Validation Error");
                    txtConfirmPassword.Focus();
                    return false;
                }
            }

            // Full name validation
            if (string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                MessageBox.Show("Full name is required!", "Validation Error");
                txtFullName.Focus();
                return false;
            }

            // Role validation
            if (cmbRole.SelectedItem == null)
            {
                MessageBox.Show("Please select a role!", "Validation Error");
                cmbRole.Focus();
                return false;
            }

            return true;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateInput())
                    return;

                var user = new UserModel
                {
                    Username = txtUsername.Text.Trim(),
                    FullName = txtFullName.Text.Trim(),
                    Email = txtEmail.Text.Trim(),
                    Phone = txtPhone.Text.Trim(),
                    RoleId = (cmbRole.SelectedItem as RoleModel)?.Id ?? 0,
                    IsActive = chkIsActive.IsChecked ?? true
                };

                if (_isEditMode)
                {
                    user.Id = _editUserId;
                    bool success = _userService.UpdateUser(user, _currentUserId);
                    if (success)
                    {
                        MessageBox.Show("User updated successfully!", "Success",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        this.DialogResult = true;
                        this.Close();
                    }
                }
                else
                {
                    user.PasswordHash = txtPassword.Password;
                    bool success = _userService.AddUser(user, _currentUserId);
                    if (success)
                    {
                        MessageBox.Show("User added successfully!", "Success",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        this.DialogResult = true;
                        this.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving user: {ex.Message}", "Error");
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void txtPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (!_isEditMode && txtPassword != null && txtConfirmPassword != null)
            {
                if (!string.IsNullOrEmpty(txtPassword.Password) &&
                    !string.IsNullOrEmpty(txtConfirmPassword.Password))
                {
                    if (txtPassword.Password == txtConfirmPassword.Password)
                    {
                        txtPasswordMatch.Text = "✓ Passwords match";
                        txtPasswordMatch.Foreground = System.Windows.Media.Brushes.Green;
                    }
                    else
                    {
                        txtPasswordMatch.Text = "✗ Passwords do not match";
                        txtPasswordMatch.Foreground = System.Windows.Media.Brushes.Red;
                    }
                }
            }
        }

        private void txtConfirmPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            txtPassword_PasswordChanged(sender, e);
        }
    }
}