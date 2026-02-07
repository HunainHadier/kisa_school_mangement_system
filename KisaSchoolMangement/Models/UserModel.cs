using System;

namespace KisaSchoolMangement.Models
{
    public class UserModel
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public bool IsActive { get; set; }
        public string CreatedAt { get; set; }
        public string UpdatedAt { get; set; }

        // For role assignment
        public string[] Permissions { get; set; }

        

        public string Status { get; set; }
        public string StatusColor { get; set; }


        // Additional fields
        public string LastLogin { get; set; }
        public int CreatedBy { get; set; }
        public string CreatedByName { get; set; }
    }

    public class RoleModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string CreatedAt { get; set; }
        public string UpdatedAt { get; set; }
    }

    public class PermissionModel : System.ComponentModel.INotifyPropertyChanged
    {
        public string Module { get; set; }
        public string Key { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        private bool _isSelected;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(IsSelected)));
                }
            }
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
    }
}
