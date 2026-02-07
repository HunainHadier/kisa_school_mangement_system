using KisaSchoolMangement.Models;
using KisaSchoolMangement.Services;
using KisaSchoolMangement.Views.User;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace KisaSchoolMangement.Views.User
{
    public partial class UserView : Window
    {
        private readonly UserService _userService;
        private ObservableCollection<UserModel> _allUsers;
        private int _currentUserId;

        // Constructor with 0 parameters (Design time ke liye)
        public UserView()
        {
            InitializeComponent();
            _userService = new UserService();
            _currentUserId = 1; // Default as Owner
            InitializeFilters(); // ✅ Ye line add karein
            LoadUsers();
            LoadStatistics();
        }

        // Constructor with 1 parameter (Runtime ke liye)
        public UserView(int currentUserId)
        {
            InitializeComponent();
            _userService = new UserService();
            _currentUserId = currentUserId;
            InitializeFilters(); // ✅ Ye line add karein
            LoadUsers();
            LoadStatistics();
        }

        // ✅ IMPORTANT: InitializeFilters method add karein
        private void InitializeFilters()
        {
            try
            {
                // cmbRoleFilter ko database se roles se bharein
                if (cmbRoleFilter != null)
                {
                    cmbRoleFilter.Items.Clear();
                    cmbRoleFilter.Items.Add(new ComboBoxItem { Content = "All Roles" });

                    var roles = _userService.GetAllRoles();
                    foreach (var role in roles)
                    {
                        cmbRoleFilter.Items.Add(new ComboBoxItem { Content = role.Name });
                    }
                    cmbRoleFilter.SelectedIndex = 0;
                }

                // cmbStatusFilter ko initialize karein
                if (cmbStatusFilter != null)
                {
                    cmbStatusFilter.Items.Clear();
                    cmbStatusFilter.Items.Add(new ComboBoxItem { Content = "All Status" });
                    cmbStatusFilter.Items.Add(new ComboBoxItem { Content = "Active" });
                    cmbStatusFilter.Items.Add(new ComboBoxItem { Content = "Inactive" });
                    cmbStatusFilter.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing filters: {ex.Message}", "Error");
            }
        }

        private void LoadUsers()
        {
            try
            {
                _allUsers = _userService.GetAllUsers(_currentUserId);
                dgUsers.ItemsSource = _allUsers;

                // Update total count
                txtTotalUsers.Text = _allUsers.Count.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading users: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadStatistics()
        {
            try
            {
                int activeCount = _userService.GetActiveUsersCount();
                txtActiveUsers.Text = activeCount.ToString();

                if (_allUsers != null)
                    txtInactiveUsers.Text = (_allUsers.Count - activeCount).ToString();
            }
            catch (Exception ex)
            {
                // Handle error
                txtInactiveUsers.Text = "0";
            }
        }

        private void btnAddUser_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddEditUserWindow(_currentUserId);
            addWindow.Owner = this;
            addWindow.ShowDialog();

            if (addWindow.DialogResult == true)
            {
                LoadUsers();
            }
        }

        private void btnEditUser_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int userId)
            {
                var editWindow = new AddEditUserWindow(_currentUserId, userId);
                editWindow.Owner = this;
                editWindow.ShowDialog();

                if (editWindow.DialogResult == true)
                {
                    LoadUsers();
                }
            }
        }

        private void btnResetPassword_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int userId)
            {
                var user = _userService.GetUserById(userId);
                if (user != null)
                {
                    var result = MessageBox.Show($"Are you sure you want to reset password for '{user.FullName}'?",
                        "Confirm Password Reset", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        // Simple password reset dialog
                        var passwordDialog = new PasswordResetDialog(user.Id, user.FullName);
                        passwordDialog.Owner = this;
                        passwordDialog.ShowDialog();

                        if (passwordDialog.DialogResult == true)
                        {
                            MessageBox.Show("Password reset successfully!", "Success",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
            }
        }

        private void btnDeleteUser_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int userId)
            {
                var user = _userService.GetUserById(userId);
                if (user != null)
                {
                    var result = MessageBox.Show($"Are you sure you want to delete user '{user.FullName}'?",
                        "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    if (result == MessageBoxResult.Yes)
                    {
                        bool success = _userService.DeleteUser(userId, _currentUserId);
                        if (success)
                        {
                            MessageBox.Show("User deleted successfully!", "Success",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                            LoadUsers();
                        }
                    }
                }
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            FilterUsers();
        }

        private void FilterUsers()
        {
            if (_allUsers == null) return;

            string searchText = txtSearch?.Text?.ToLower() ?? "";
            string roleFilter = (cmbRoleFilter?.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "All Roles";
            string statusFilter = (cmbStatusFilter?.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "All Status";

            if (string.IsNullOrEmpty(searchText) && roleFilter == "All Roles" && statusFilter == "All Status")
            {
                dgUsers.ItemsSource = _allUsers;
                return;
            }

            var filteredUsers = _allUsers.Where(u =>
                (string.IsNullOrEmpty(searchText) ||
                 u.FullName.ToLower().Contains(searchText) ||
                 u.Username.ToLower().Contains(searchText) ||
                 u.Email.ToLower().Contains(searchText) ||
                 (u.Phone != null && u.Phone.ToLower().Contains(searchText))) &&
                (roleFilter == "All Roles" || u.RoleName == roleFilter) &&
                (statusFilter == "All Status" ||
                 (statusFilter == "Active" && u.IsActive) ||
                 (statusFilter == "Inactive" && !u.IsActive))
            ).ToList();

            dgUsers.ItemsSource = new ObservableCollection<UserModel>(filteredUsers);
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterUsers();
        }

        private void cmbRoleFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterUsers();
        }

        private void cmbStatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterUsers();
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadUsers();
            if (txtSearch != null) txtSearch.Clear();
            if (cmbRoleFilter != null) cmbRoleFilter.SelectedIndex = 0;
            if (cmbStatusFilter != null) cmbStatusFilter.SelectedIndex = 0;
        }

        private void dgUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Selection change logic if needed
        }
    }
}