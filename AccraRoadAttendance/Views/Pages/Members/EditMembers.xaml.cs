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
    public partial class EditMembers : UserControl, INotifyPropertyChanged
    {
        private readonly MainWindow _mainWindow;
        private readonly AttendanceDbContext _context;
        private Member _currentMember;

        public EditMembers(AttendanceDbContext context, MainWindow mainWindow, Member member)
        {
            System.Diagnostics.Debug.WriteLine("Entering EditMembers constructor");
            InitializeComponent();
            _context = context;
            _mainWindow = mainWindow;
            _currentMember = member;
            DataContext = this;
            System.Diagnostics.Debug.WriteLine("Before PopulateForm");
            PopulateForm();
            System.Diagnostics.Debug.WriteLine("After PopulateForm");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Bindable properties for form inputs
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string OtherNames { get; set; }
        public string Gender { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string SelectedPicturePath { get; set; }

        private void PopulateForm()
        {
            FirstName = _currentMember.FirstName;
            LastName = _currentMember.LastName;
            OtherNames = _currentMember.OtherNames;
            Gender = _currentMember.Sex.ToString();
            PhoneNumber = _currentMember.PhoneNumber;
            Email = _currentMember.Email;
            SelectedPicturePath = _currentMember.PicturePath;
            ImagePreview.Source = string.IsNullOrEmpty(SelectedPicturePath) ? null : new BitmapImage(new Uri(SelectedPicturePath));

            OnPropertyChanged(nameof(FirstName));
            OnPropertyChanged(nameof(LastName));
            OnPropertyChanged(nameof(OtherNames));
            OnPropertyChanged(nameof(Gender));
            OnPropertyChanged(nameof(PhoneNumber));
            OnPropertyChanged(nameof(Email));
            OnPropertyChanged(nameof(SelectedPicturePath));
        }

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
                OnPropertyChanged(nameof(SelectedPicturePath));
            }
        }

        private async void UpdateMember_Click(object sender, RoutedEventArgs e)
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

                // Attach and update member details
                var memberToUpdate = await _context.Members.FindAsync(_currentMember.Id);

                if (memberToUpdate == null)
                {
                    MessageBox.Show("The member could not be found in the database.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Update member details
                memberToUpdate.FirstName = FirstName;
                memberToUpdate.LastName = LastName;
                memberToUpdate.OtherNames = OtherNames;
                memberToUpdate.Sex = parsedGender;
                memberToUpdate.PhoneNumber = PhoneNumber;
                memberToUpdate.Email = Email;
                memberToUpdate.PicturePath = SelectedPicturePath;

                await _context.SaveChangesAsync();

                MessageBox.Show("Member updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                _mainWindow.NavigateToMembers();
            }
            catch (DbUpdateException dbEx)
            {
                // Log inner exceptions
                var innerException = dbEx.InnerException;
                while (innerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner Exception: {innerException.Message}");
                    innerException = innerException.InnerException;
                }
                MessageBox.Show($"An error occurred while updating the member: {dbEx.Message}. Check the debug output for more details.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[{DateTime.Now}] Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
                MessageBox.Show($"An error occurred while updating the member: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            // Here you might want to reload the original member data or just close the edit view
            PopulateForm();
        }
    }
}