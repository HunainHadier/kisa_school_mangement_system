using KisaSchoolMangement.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace KisaSchoolMangement.ViewModels
{
    public class DashboardViewModel
    {
        public ObservableCollection<DashboardStat> Statistics { get; set; }
        public ObservableCollection<RecentActivity> RecentActivities { get; set; }

        public double PaidFeePercentage { get; set; } = 120; // just visual demo
        public double PendingFeePercentage { get; set; } = 60;
        public string FeeSummary { get; set; } = "Paid 70%, Pending 30%";

        public ICommand NavigateToStudentRegistrationCommand { get; set; }
        public ICommand NavigateToFeeCollectionCommand { get; set; }
        public ICommand NavigateToAttendanceCommand { get; set; }
        public ICommand NavigateToExamEntryCommand { get; set; }
        public ICommand NavigateToDonorCommand { get; set; }

        public DashboardViewModel()
        {
            // Dummy statistics
            Statistics = new ObservableCollection<DashboardStat>
            {
                new DashboardStat { Title = "Students", Count = 320, Description = "Total Registered", Color = "#3498db" },
                new DashboardStat { Title = "Teachers", Count = 25, Description = "Active", Color = "#2ecc71" },
                new DashboardStat { Title = "Donors", Count = 15, Description = "Active Donors", Color = "#e74c3c" },
                new DashboardStat { Title = "Pending Fees", Count = 40, Description = "Awaiting Payments", Color = "#f39c12" },
            };

            // Dummy activities
            RecentActivities = new ObservableCollection<RecentActivity>
            {
                new RecentActivity { Activity = "Ali Khan registered in Class 5", Time = "5 min ago" },
                new RecentActivity { Activity = "Fee received from Ayesha", Time = "20 min ago" },
                new RecentActivity { Activity = "New donor added - Mr. Ahmed", Time = "1 hr ago" },
                new RecentActivity { Activity = "Attendance marked for Class 10", Time = "2 hrs ago" },
            };

            // Commands
            NavigateToStudentRegistrationCommand = new RelayCommand(_ => Navigate("StudentRegistration"));
            NavigateToFeeCollectionCommand = new RelayCommand(_ => Navigate("FeeCollection"));
            NavigateToAttendanceCommand = new RelayCommand(_ => Navigate("Attendance"));
            NavigateToExamEntryCommand = new RelayCommand(_ => Navigate("Exam"));
            NavigateToDonorCommand = new RelayCommand(_ => Navigate("Donor"));
        }

        private void Navigate(string page)
        {
            System.Windows.MessageBox.Show($"Navigating to {page} page...");
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action<object> execute;
        private readonly Func<object, bool> canExecute;

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => canExecute == null || canExecute(parameter);
        public void Execute(object parameter) => execute(parameter);
        public event EventHandler CanExecuteChanged;
    }
}
