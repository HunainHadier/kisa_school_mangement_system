using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using System.IO;
using System.Windows.Media.Imaging;
using KisaSchoolMangement.Models;
using KisaSchoolMangement.Services;

namespace KisaSchoolMangement.Views.Student
{
    public partial class AddStudentWindow : Window
    {
        private readonly StudentService _studentService;
        private readonly ClassService _classService;
        private readonly SectionService _sectionService;
        private readonly StudentModel _studentToEdit;

        // Photo properties
        public string StudentPhotoPath { get; private set; }
        public string CnicPhotoPath { get; private set; }

        public AddStudentWindow()
        {
            InitializeComponent();
            _studentService = new StudentService();
            _classService = new ClassService();
            _sectionService = new SectionService();
            Title = "Add New Student";

            LoadClassesAndSections();
            SetDefaultValues();

            // Agar Add mode hai to Left Information fields hide karein
            if (_studentToEdit == null)
            {
                HideLeftInformationFields();
            }
        }

        public AddStudentWindow(StudentModel student) : this()
        {
            _studentToEdit = student;
            Title = "Edit Student";
            LoadStudentData();

            // Agar Edit mode hai to Left Information fields show karein
            ShowLeftInformationFields();
        }

        private void ShowLeftInformationFields()
        {
            // Tab mein Left Information section ko show karein
            // Yeh XAML changes ke baad automatically show hoga
        }

        private void HideLeftInformationFields()
        {
            // XAML mein Left Information section hide karne ke liye
            // Iske liye XAML mein Visibility property use karein
        }

        private void LoadClassesAndSections()
        {
            try
            {
                // Load classes
                var classes = _classService.GetAllClasses();
                cmbClass.ItemsSource = classes;

                // Load sections
                var sections = _sectionService.GetAllSections();
                cmbSection.ItemsSource = sections;

                if (classes.Count > 0)
                    cmbClass.SelectedIndex = 0;
                if (sections.Count > 0)
                    cmbSection.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading classes/sections: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetDefaultValues()
        {
            dpDOB.SelectedDate = DateTime.Today.AddYears(-5); // Default 5 years old
            dpAdmissionDate.SelectedDate = DateTime.Today;
            dpLeftDate.SelectedDate = null; // Left date initially empty
            txtAdmissionYear.Text = DateTime.Now.Year.ToString();
            txtFamilyMembers.Text = "1";
            txtChildPosition.Text = "1";
            txtMonthlyFee.Text = "0";
            txtFatherIncome.Text = "0";
            txtMotherIncome.Text = "0";

            // Default status
            cmbStatus.SelectedIndex = 0; // "active"
        }

        private void LoadStudentData()
        {
            if (_studentToEdit != null)
            {
                txtGrNo.Text = _studentToEdit.GrNo;
                txtStudentName.Text = _studentToEdit.StudentName;

                if (DateTime.TryParse(_studentToEdit.DOB, out DateTime dob))
                    dpDOB.SelectedDate = dob;

                cmbGender.Text = _studentToEdit.Gender;

                // Set class and section
                foreach (var item in cmbClass.Items)
                {
                    if (item is ClassModel classItem && classItem.Id == _studentToEdit.ClassId)
                    {
                        cmbClass.SelectedItem = item;
                        break;
                    }
                }

                foreach (var item in cmbSection.Items)
                {
                    if (item is SectionModel sectionItem && sectionItem.Id == _studentToEdit.SectionId)
                    {
                        cmbSection.SelectedItem = item;
                        break;
                    }
                }

                if (DateTime.TryParse(_studentToEdit.AdmissionDate, out DateTime admissionDate))
                    dpAdmissionDate.SelectedDate = admissionDate;

                txtAdmissionYear.Text = _studentToEdit.AdmissionYear.ToString();
                txtAdmissionClass.Text = _studentToEdit.AdmissionClass;
                txtFatherName.Text = _studentToEdit.FatherName;
                txtFamilyCode.Text = _studentToEdit.FamilyCode.ToString();
                txtDistrictCode.Text = _studentToEdit.DistrictCode.ToString();
                cmbSyed.SelectedItem = _studentToEdit.IsSyed ? cmbSyed.Items[0] : cmbSyed.Items[1];
                txtFamilyMembers.Text = _studentToEdit.FamilyMembers.ToString();
                txtChildPosition.Text = _studentToEdit.ChildPosition.ToString();

                if (!string.IsNullOrEmpty(_studentToEdit.StudentCategory))
                    cmbStudentCategory.Text = _studentToEdit.StudentCategory;

                // CNIC Information
                txtChildCNIC.Text = _studentToEdit.ChildCNICNumber;
                txtFatherCNIC.Text = _studentToEdit.FatherCNIC;
                txtFatherOccupation.Text = _studentToEdit.FatherOccupation;
                txtFatherIncome.Text = _studentToEdit.FatherMonthlyIncome.HasValue ? _studentToEdit.FatherMonthlyIncome.Value.ToString() : "0";

                // Mother Information
                txtMotherName.Text = _studentToEdit.MotherName;
                txtMotherCNIC.Text = _studentToEdit.MotherCNICNumber;
                txtMotherIncome.Text = _studentToEdit.MotherMonthlyIncome.HasValue ? _studentToEdit.MotherMonthlyIncome.Value.ToString() : "0";

                // Guardian Information
                txtGuardianName.Text = _studentToEdit.GuardianName;
                txtGuardianPhone.Text = _studentToEdit.GuardianPhone;
                txtMotherPhone.Text = _studentToEdit.MotherPhone;
                txtMotherWhatsapp.Text = _studentToEdit.MotherWhatsapp;

                // Residential Information
                if (!string.IsNullOrEmpty(_studentToEdit.HomeStatus))
                    cmbHomeStatus.Text = _studentToEdit.HomeStatus;

                txtAddress.Text = _studentToEdit.Address;
                txtScholarshipReason.Text = _studentToEdit.ReasonOfScholarship;

                if (_studentToEdit.MonthlyFee.HasValue)
                    txtMonthlyFee.Text = _studentToEdit.MonthlyFee.Value.ToString();

                chkIsActive.IsChecked = _studentToEdit.IsActive;

                // NEW FIELDS: Left Information
                txtLeftMonth.Text = _studentToEdit.LeftMonth;

                if (!string.IsNullOrEmpty(_studentToEdit.LeftDate) && DateTime.TryParse(_studentToEdit.LeftDate, out DateTime leftDate))
                    dpLeftDate.SelectedDate = leftDate;

                txtLeftReason.Text = _studentToEdit.ReasonOfSchoolLeft;

                // Set Status
                if (!string.IsNullOrEmpty(_studentToEdit.Status))
                {
                    foreach (ComboBoxItem item in cmbStatus.Items)
                    {
                        if (item.Content.ToString() == _studentToEdit.Status)
                        {
                            cmbStatus.SelectedItem = item;
                            break;
                        }
                    }
                }

                // Load photos if they exist
                if (!string.IsNullOrEmpty(_studentToEdit.StudentPhotoPath))
                {
                    LoadImage(_studentToEdit.StudentPhotoPath, imgStudentPhoto);
                    StudentPhotoPath = _studentToEdit.StudentPhotoPath;
                }

                if (!string.IsNullOrEmpty(_studentToEdit.CnicPhotoPath))
                {
                    LoadImage(_studentToEdit.CnicPhotoPath, imgCnicPhoto);
                    CnicPhotoPath = _studentToEdit.CnicPhotoPath;
                }
            }
        }

        // Photo Upload Methods
        private void btnUploadStudentPhoto_Click(object sender, RoutedEventArgs e)
        {
            StudentPhotoPath = UploadPhoto(imgStudentPhoto);
        }

        private void btnUploadCnicPhoto_Click(object sender, RoutedEventArgs e)
        {
            CnicPhotoPath = UploadPhoto(imgCnicPhoto);
        }

        private string UploadPhoto(System.Windows.Controls.Image imageControl)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.jpg; *.jpeg; *.png; *.bmp)|*.jpg; *.jpeg; *.png; *.bmp",
                Title = "Select Student Photo"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    LoadImage(openFileDialog.FileName, imageControl);
                    return openFileDialog.FileName;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading image: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            return null;
        }

        private void LoadImage(string filePath, System.Windows.Controls.Image imageControl)
        {
            if (File.Exists(filePath))
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(filePath);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                imageControl.Source = bitmap;
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateForm())
            {
                try
                {
                    var student = new StudentModel
                    {
                        GrNo = txtGrNo.Text.Trim(),
                        StudentName = txtStudentName.Text.Trim(),
                        DOB = dpDOB.SelectedDate?.ToString("yyyy-MM-dd"),
                        Gender = (cmbGender.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? cmbGender.Text,
                        ClassId = (cmbClass.SelectedItem as ClassModel)?.Id,
                        SectionId = (cmbSection.SelectedItem as SectionModel)?.Id,
                        AdmissionDate = dpAdmissionDate.SelectedDate?.ToString("yyyy-MM-dd"),
                        AdmissionYear = int.TryParse(txtAdmissionYear.Text, out int year) ? year : DateTime.Now.Year,
                        AdmissionClass = txtAdmissionClass.Text.Trim(),
                        FatherName = txtFatherName.Text.Trim(),
                        FamilyCode = int.TryParse(txtFamilyCode.Text, out int familyCode) ? familyCode : 0,
                        DistrictCode = int.TryParse(txtDistrictCode.Text, out int districtCode) ? districtCode : 0,
                        IsSyed = (cmbSyed.SelectedItem as ComboBoxItem)?.Content?.ToString() == "Syed",
                        FamilyMembers = int.TryParse(txtFamilyMembers.Text, out int familyMembers) ? familyMembers : 1,
                        ChildPosition = int.TryParse(txtChildPosition.Text, out int childPosition) ? childPosition : 1,
                        StudentCategory = (cmbStudentCategory.SelectedItem as ComboBoxItem)?.Content?.ToString() == "-- Select Category --" ?
                                         null : (cmbStudentCategory.SelectedItem as ComboBoxItem)?.Content?.ToString(),

                        // CNIC Information
                        ChildCNICNumber = txtChildCNIC.Text.Trim(),
                        FatherCNIC = txtFatherCNIC.Text.Trim(),
                        FatherOccupation = txtFatherOccupation.Text.Trim(),
                        FatherMonthlyIncome = decimal.TryParse(txtFatherIncome.Text, out decimal fatherIncome) ? fatherIncome : (decimal?)null,

                        // Mother Information
                        MotherName = txtMotherName.Text.Trim(),
                        MotherCNICNumber = txtMotherCNIC.Text.Trim(),
                        MotherMonthlyIncome = decimal.TryParse(txtMotherIncome.Text, out decimal motherIncome) ? motherIncome : (decimal?)null,

                        // Guardian Information
                        GuardianName = txtGuardianName.Text.Trim(),
                        GuardianPhone = txtGuardianPhone.Text.Trim(),
                        MotherPhone = txtMotherPhone.Text.Trim(),
                        MotherWhatsapp = txtMotherWhatsapp.Text.Trim(),

                        // Residential Information
                        HomeStatus = (cmbHomeStatus.SelectedItem as ComboBoxItem)?.Content?.ToString() == "-- Select Status --" ?
                                    null : (cmbHomeStatus.SelectedItem as ComboBoxItem)?.Content?.ToString(),
                        Address = txtAddress.Text.Trim(),
                        ReasonOfScholarship = txtScholarshipReason.Text.Trim(),
                        MonthlyFee = decimal.TryParse(txtMonthlyFee.Text, out decimal fee) ? fee : (decimal?)null,

                        // NEW: Left Information
                        LeftMonth = txtLeftMonth.Text.Trim(),
                        LeftDate = dpLeftDate.SelectedDate?.ToString("yyyy-MM-dd"),
                        ReasonOfSchoolLeft = txtLeftReason.Text.Trim(),
                        Status = (cmbStatus.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "active",

                        // Photo paths
                        StudentPhotoPath = StudentPhotoPath,
                        CnicPhotoPath = CnicPhotoPath,

                        IsActive = chkIsActive.IsChecked ?? true,
                        CreatedAt = _studentToEdit?.CreatedAt ?? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        UpdatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    };

                    if (_studentToEdit != null)
                    {
                        student.Id = _studentToEdit.Id;
                    }

                    bool success;

                    if (_studentToEdit != null)
                    {
                        success = _studentService.UpdateStudent(student);
                    }
                    else
                    {
                        success = _studentService.AddStudent(student);
                    }

                    if (success)
                    {
                        MessageBox.Show($"Student {(_studentToEdit != null ? "updated" : "added")} successfully!",
                            "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        DialogResult = true;
                        Close();
                    }
                    else
                    {
                        MessageBox.Show($"Failed to {(_studentToEdit != null ? "update" : "add")} student. Please try again.",
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving student: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool ValidateForm()
        {
            // Validate GR Number
            if (string.IsNullOrWhiteSpace(txtGrNo.Text))
            {
                MessageBox.Show("Please enter GR number!", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtGrNo.Focus();
                return false;
            }

            // Validate Student Name
            if (string.IsNullOrWhiteSpace(txtStudentName.Text))
            {
                MessageBox.Show("Please enter student name!", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtStudentName.Focus();
                return false;
            }

            // Validate DOB
            if (dpDOB.SelectedDate == null)
            {
                MessageBox.Show("Please select date of birth!", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                dpDOB.Focus();
                return false;
            }

            // Validate Guardian Name
            if (string.IsNullOrWhiteSpace(txtGuardianName.Text))
            {
                MessageBox.Show("Please enter guardian name!", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtGuardianName.Focus();
                return false;
            }

            // Validate Guardian Phone
            if (string.IsNullOrWhiteSpace(txtGuardianPhone.Text))
            {
                MessageBox.Show("Please enter guardian phone number!", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtGuardianPhone.Focus();
                return false;
            }

            // Validate Address
            if (string.IsNullOrWhiteSpace(txtAddress.Text))
            {
                MessageBox.Show("Please enter address!", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtAddress.Focus();
                return false;
            }

            // Validate Father Name
            if (string.IsNullOrWhiteSpace(txtFatherName.Text))
            {
                MessageBox.Show("Please enter father name!", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtFatherName.Focus();
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