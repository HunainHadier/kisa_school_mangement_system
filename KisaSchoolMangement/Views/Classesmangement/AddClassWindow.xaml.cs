using System.Windows;
using KisaSchoolMangement.Models;
using KisaSchoolMangement.Services;

namespace KisaSchoolMangement.Views.Class
{
    public partial class AddClassWindow : Window
    {
        private readonly ClassService _classService;
        private readonly ClassModel _classToEdit;

        public AddClassWindow()
        {
            InitializeComponent();
            _classService = new ClassService();
            Title = "Add New Class";
        }

        public AddClassWindow(ClassModel classModel) : this()
        {
            _classToEdit = classModel;
            Title = "Edit Class";
            LoadClassData();
        }

        private void LoadClassData()
        {
            if (_classToEdit != null)
            {
                txtName.Text = _classToEdit.Name;
                txtCode.Text = _classToEdit.Code;
                txtDescription.Text = _classToEdit.Description;
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateForm())
            {
                var classModel = new ClassModel
                {
                    Name = txtName.Text.Trim(),
                    Code = txtCode.Text.Trim(),
                    Description = txtDescription.Text.Trim()
                };

                bool success;

                if (_classToEdit != null)
                {
                    classModel.Id = _classToEdit.Id;
                    success = _classService.UpdateClass(classModel);
                }
                else
                {
                    success = _classService.AddClass(classModel);
                }

                if (success)
                {
                    MessageBox.Show($"Class {(_classToEdit != null ? "updated" : "added")} successfully!",
                        "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show($"Failed to {(_classToEdit != null ? "update" : "add")} class. Please try again.",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Please enter class name!", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtName.Focus();
                return false;
            }

            return true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}