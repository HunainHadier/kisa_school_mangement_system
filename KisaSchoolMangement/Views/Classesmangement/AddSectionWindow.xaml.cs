using System;
using System.Windows;
using System.Windows.Controls;
using KisaSchoolMangement.Models;
using KisaSchoolMangement.Services;

namespace KisaSchoolMangement.Views.Section
{
    public partial class AddSectionWindow : Window
    {
        private readonly SectionService _sectionService;
        private readonly ClassService _classService;
        private readonly SectionModel _sectionToEdit;

        public AddSectionWindow()
        {
            InitializeComponent();
            _sectionService = new SectionService();
            _classService = new ClassService();
            Title = "Add New Section";

            LoadClasses();
        }

        public AddSectionWindow(SectionModel sectionModel) : this()
        {
            _sectionToEdit = sectionModel;
            Title = "Edit Section";
            LoadSectionData();
        }

        private void LoadClasses()
        {
            try
            {
                var classes = _classService.GetAllClasses();
                cmbClass.ItemsSource = classes;

                if (classes.Count > 0)
                    cmbClass.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading classes: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadSectionData()
        {
            if (_sectionToEdit != null)
            {
                txtName.Text = _sectionToEdit.Name;
                txtCapacity.Text = _sectionToEdit.Capacity.ToString();

                // Set class
                foreach (var item in cmbClass.Items)
                {
                    if (item is ClassModel classItem && classItem.Id == _sectionToEdit.ClassId)
                    {
                        cmbClass.SelectedItem = item;
                        break;
                    }
                }
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateForm())
            {
                var sectionModel = new SectionModel
                {
                    ClassId = (cmbClass.SelectedItem as ClassModel)?.Id ?? 0,
                    Name = txtName.Text.Trim(),
                    Capacity = int.TryParse(txtCapacity.Text, out int capacity) ? capacity : 30
                };

                bool success;

                if (_sectionToEdit != null)
                {
                    sectionModel.Id = _sectionToEdit.Id;
                    success = _sectionService.UpdateSection(sectionModel);
                }
                else
                {
                    success = _sectionService.AddSection(sectionModel);
                }

                if (success)
                {
                    MessageBox.Show($"Section {(_sectionToEdit != null ? "updated" : "added")} successfully!",
                        "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show($"Failed to {(_sectionToEdit != null ? "update" : "add")} section. Please try again.",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool ValidateForm()
        {
            if (cmbClass.SelectedItem == null)
            {
                MessageBox.Show("Please select a class!", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                cmbClass.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Please enter section name!", "Validation Error",
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