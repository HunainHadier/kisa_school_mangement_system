using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using KisaSchoolMangement.Models;
using KisaSchoolMangement.Services;

namespace KisaSchoolMangement.Views.User
{
    public partial class RolePermissionWindow : Window, INotifyPropertyChanged
    {
        private readonly RolePermissionService _service;
        private readonly int _currentUserId;
        private RoleModel _selectedRole;

        public ObservableCollection<RoleModel> Roles { get; private set; }
        public ObservableCollection<PermissionModel> Permissions { get; private set; }

        public RoleModel SelectedRole
        {
            get => _selectedRole;
            set
            {
                if (_selectedRole != value)
                {
                    _selectedRole = value;
                    OnPropertyChanged();
                    LoadRolePermissions();
                }
            }
        }

        public RolePermissionWindow(int currentUserId)
        {
            InitializeComponent();
            _service = new RolePermissionService();
            _currentUserId = currentUserId;
            DataContext = this;
            LoadData();
        }

        private void LoadData()
        {
            Roles = _service.GetAllRoles();
            Permissions = _service.GetAllPermissions();
            OnPropertyChanged(nameof(Roles));
            OnPropertyChanged(nameof(Permissions));

            SelectedRole = Roles.FirstOrDefault();
        }

        private void LoadRolePermissions()
        {
            if (SelectedRole == null || Permissions == null)
            {
                return;
            }

            var rolePermissionKeys = _service.GetPermissionKeysForRole(SelectedRole.Id);

            foreach (var permission in Permissions)
            {
                permission.IsSelected = rolePermissionKeys.Contains(permission.Key);
            }

            OnPropertyChanged(nameof(Permissions));
        }

        private void SavePermissions_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedRole == null)
            {
                MessageBox.Show("Please select a role.", "Validation");
                return;
            }

            var selectedKeys = Permissions
                .Where(p => p.IsSelected)
                .Select(p => p.Key)
                .ToList();

            bool result = _service.SaveRolePermissions(SelectedRole.Id, selectedKeys, _currentUserId);

            if (result)
            {
                MessageBox.Show("Permissions saved successfully.", "Success");
            }
        }

        private void CreateRole_Click(object sender, RoutedEventArgs e)
        {
            string name = txtNewRoleName.Text?.Trim();
            string description = txtNewRoleDescription.Text?.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Role name is required.", "Validation");
                return;
            }

            bool result = _service.CreateRole(name, description, _currentUserId);
            if (result)
            {
                txtNewRoleName.Text = string.Empty;
                txtNewRoleDescription.Text = string.Empty;
                Roles = _service.GetAllRoles();
                OnPropertyChanged(nameof(Roles));
                SelectedRole = Roles.FirstOrDefault(r => r.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                               ?? Roles.FirstOrDefault();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
