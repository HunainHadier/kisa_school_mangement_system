using System;
using System.ComponentModel;

namespace KisaSchoolMangement.Models
{
    public class TeacherModel : INotifyPropertyChanged
    {
        private int _id;
        private int _userId;
        private string _fullName;
        private string _email;
        private string _phone;
        private string _hireDate;
        private bool _isActive;
        private string _photoPath;
        private string _qualification;
        private string _address;
        private DateTime _createdAt;

        public int Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(); }
        }

        public int UserId
        {
            get => _userId;
            set { _userId = value; OnPropertyChanged(); }
        }

        public string FullName
        {
            get => _fullName;
            set { _fullName = value; OnPropertyChanged(); }
        }

        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(); }
        }

        public string Phone
        {
            get => _phone;
            set { _phone = value; OnPropertyChanged(); }
        }

        public string HireDate
        {
            get => _hireDate;
            set { _hireDate = value; OnPropertyChanged(); }
        }

        public bool IsActive
        {
            get => _isActive;
            set { _isActive = value; OnPropertyChanged(); }
        }

        public string PhotoPath
        {
            get => _photoPath;
            set { _photoPath = value; OnPropertyChanged(); }
        }

        public string Qualification
        {
            get => _qualification;
            set { _qualification = value; OnPropertyChanged(); }
        }

        public string Address
        {
            get => _address;
            set { _address = value; OnPropertyChanged(); }
        }

        public DateTime CreatedAt
        {
            get => _createdAt;
            set { _createdAt = value; OnPropertyChanged(); }
        }

        // For display purposes
        public string Status => IsActive ? "Active" : "Inactive";
        public string StatusColor => IsActive ? "#27ae60" : "#e74c3c";

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}