using KisaSchoolMangement.Models;
using KisaSchoolMangement.Services;
using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace KisaSchoolMangement.Views.Teachers
{
    public partial class AddEditTeacherWindow : Window
    {
        private readonly TeacherService _teacherService;
        private TeacherModel _teacher;
        private bool _isEditMode = false;
        private string _selectedPhotoPath;

        public AddEditTeacherWindow()
        {
            InitializeComponent();
            _teacherService = new TeacherService();
            InitializeForm();
        }

        public AddEditTeacherWindow(TeacherModel teacher) : this()
        {
            _teacher = teacher;
            _isEditMode = true;
            InitializeFormForEdit();
        }

        private void InitializeForm()
        {
            dpHireDate.SelectedDate = DateTime.Today;
            chkIsActive.IsChecked = true;
        }

        private void InitializeFormForEdit()
        {
            txtHeader.Text = "👨‍🏫 Edit Teacher";
            btnSave.Content = "💾 Update Teacher";

            // Populate form with existing data
            txtFullName.Text = _teacher.FullName;
            txtEmail.Text = _teacher.Email;
            txtPhone.Text = _teacher.Phone;

            if (DateTime.TryParse(_teacher.HireDate, out DateTime hireDate))
                dpHireDate.SelectedDate = hireDate;

            txtQualification.Text = _teacher.Qualification;
            chkIsActive.IsChecked = _teacher.IsActive;
            txtAddress.Text = _teacher.Address;

            // Load photo if exists
            if (!string.IsNullOrEmpty(_teacher.PhotoPath))
            {
                LoadTeacherPhoto(_teacher.PhotoPath);
            }
        }

        private void BtnUploadPhoto_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var openFileDialog = new OpenFileDialog
                {
                    Filter = "Image files (*.jpg; *.jpeg; *.png; *.gif)|*.jpg;*.jpeg;*.png;*.gif",
                    Title = "Select Teacher Photo"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    var fileInfo = new FileInfo(openFileDialog.FileName);
                    if (fileInfo.Length > 2 * 1024 * 1024) // 2MB limit
                    {
                        MessageBox.Show("Please select an image smaller than 2MB.", "File Too Large",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    _selectedPhotoPath = openFileDialog.FileName;
                    LoadTeacherPhoto(_selectedPhotoPath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error uploading photo: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnRemovePhoto_Click(object sender, RoutedEventArgs e)
        {
            _selectedPhotoPath = null;
            imgTeacherPhoto.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/default-teacher.png"));
        }

        private void LoadTeacherPhoto(string photoPath)
        {
            try
            {
                if (File.Exists(photoPath))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(photoPath);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    imgTeacherPhoto.Source = bitmap;
                }
                else if (photoPath.StartsWith("teacher-photos/"))
                {
                    // Load from wwwroot folder
                    string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot", photoPath);
                    if (File.Exists(fullPath))
                    {
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(fullPath);
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        imgTeacherPhoto.Source = bitmap;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading photo: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateForm())
            {
                try
                {
                    string savedPhotoPath = null;

                    // Save photo if new one is selected
                    if (!string.IsNullOrEmpty(_selectedPhotoPath))
                    {
                        savedPhotoPath = _teacherService.SaveTeacherPhoto(_selectedPhotoPath);
                    }

                    if (_isEditMode)
                    {
                        // Update existing teacher
                        _teacher.FullName = txtFullName.Text.Trim();
                        _teacher.Email = txtEmail.Text.Trim();
                        _teacher.Phone = txtPhone.Text.Trim();
                        _teacher.HireDate = dpHireDate.SelectedDate?.ToString("yyyy-MM-dd");
                        _teacher.Qualification = txtQualification.Text.Trim();
                        _teacher.IsActive = chkIsActive.IsChecked ?? true;
                        _teacher.Address = txtAddress.Text.Trim();

                        if (!string.IsNullOrEmpty(savedPhotoPath))
                            _teacher.PhotoPath = savedPhotoPath;

                        if (_teacherService.UpdateTeacher(_teacher))
                        {
                            MessageBox.Show("Teacher updated successfully!", "Success",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                            DialogResult = true;
                            Close();
                        }
                    }
                    else
                    {
                        // Add new teacher
                        var newTeacher = new TeacherModel
                        {
                            FullName = txtFullName.Text.Trim(),
                            Email = txtEmail.Text.Trim(),
                            Phone = txtPhone.Text.Trim(),
                            HireDate = dpHireDate.SelectedDate?.ToString("yyyy-MM-dd"),
                            Qualification = txtQualification.Text.Trim(),
                            IsActive = chkIsActive.IsChecked ?? true,
                            Address = txtAddress.Text.Trim(),
                            PhotoPath = savedPhotoPath
                        };

                        if (_teacherService.AddTeacher(newTeacher))
                        {
                            MessageBox.Show("Teacher added successfully!", "Success",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                            DialogResult = true;
                            Close();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving teacher: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                MessageBox.Show("Please enter teacher's full name!", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtFullName.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtQualification.Text))
            {
                MessageBox.Show("Please enter teacher's qualification!", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtQualification.Focus();
                return false;
            }

            if (!string.IsNullOrWhiteSpace(txtEmail.Text) && !IsValidEmail(txtEmail.Text))
            {
                MessageBox.Show("Please enter a valid email address!", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtEmail.Focus();
                return false;
            }

            return true;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}