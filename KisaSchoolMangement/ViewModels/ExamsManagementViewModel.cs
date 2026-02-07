using KisaSchoolMangement.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace KisaSchoolMangement.ViewModels
{
    public class ExamsManagementViewModel : INotifyPropertyChanged
    {
        private ExamModel _selectedExam;

        public ExamModel SelectedExam
        {
            get => _selectedExam;
            set
            {
                _selectedExam = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}