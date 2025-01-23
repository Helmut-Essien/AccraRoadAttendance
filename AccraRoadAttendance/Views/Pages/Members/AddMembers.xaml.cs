using AccraRoadAttendance.Data;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using AccraRoadAttendance.Models;
using Microsoft.EntityFrameworkCore;

namespace AccraRoadAttendance.Views.Pages.Members
{
    public partial class AddMembers : UserControl, INotifyPropertyChanged
    {
        private readonly AttendanceDbContext _context;

        public AddMembers(AttendanceDbContext context)
        {
            InitializeComponent();
            _context = context;
            DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string _selectedPicturePath;
        public string SelectedPicturePath
        {
            get => _selectedPicturePath;
            set
            {
                _selectedPicturePath = value;
                OnPropertyChanged(nameof(SelectedPicturePath));
            }
        }

        // Bindable properties for form inputs
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string OtherNames { get; set; }
        public string Gender { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }

        private void UploadPicture_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif",
                Title = "Select a Picture"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                SelectedPicturePath = openFileDialog.FileName;
                ImagePreview.Source = new BitmapImage(new Uri(SelectedPicturePath));
            }
        }

        private void SaveMember_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validate inputs
                if (string.IsNullOrWhiteSpace(FirstName) ||
                    string.IsNullOrWhiteSpace(LastName) ||
                    string.IsNullOrWhiteSpace(Gender) ||
                    string.IsNullOrWhiteSpace(PhoneNumber))
                {
                    MessageBox.Show("Please fill in all required fields.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                // Convert Gender string to enum
                if (!Enum.TryParse(Gender, true, out Gender parsedGender))
                {
                    MessageBox.Show("Invalid gender selection.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Create a new Member object
                var newMember = new Member
                {
                    FirstName = FirstName,
                    LastName = LastName,
                    OtherNames = OtherNames,
                    Sex = parsedGender,
                    PhoneNumber = PhoneNumber,
                    Email = Email,
                    PicturePath = SelectedPicturePath,

                }; 

                // Save to database
                _context.Members.Add(newMember);
                _context.SaveChanges();

                MessageBox.Show("Member saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                // Optionally clear the form
                ClearForm();
            }
            catch (DbUpdateException dbEx)
            {
                var innerException = dbEx.InnerException;
                while (innerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner Exception: {innerException.Message}");
                    innerException = innerException.InnerException;
                }
                MessageBox.Show($"An error occurred while saving the member: {dbEx.Message}. Check the debug output for more details.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[{DateTime.Now}] Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
                MessageBox.Show($"An error occurred while saving the member: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            FirstName = string.Empty;
            LastName = string.Empty;
            OtherNames = string.Empty;
            Gender = string.Empty;
            PhoneNumber = string.Empty;
            Email = string.Empty;
            SelectedPicturePath = string.Empty;
            ImagePreview.Source = null;

            // Notify the UI to update bound fields
            OnPropertyChanged(nameof(FirstName));
            OnPropertyChanged(nameof(LastName));
            OnPropertyChanged(nameof(OtherNames));
            OnPropertyChanged(nameof(Gender));
            OnPropertyChanged(nameof(PhoneNumber));
            OnPropertyChanged(nameof(Email));
            OnPropertyChanged(nameof(SelectedPicturePath));
        }
    }
}
