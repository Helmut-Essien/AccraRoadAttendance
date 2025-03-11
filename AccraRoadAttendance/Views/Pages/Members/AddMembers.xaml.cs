using AccraRoadAttendance.Data;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using AccraRoadAttendance.Models;
using Microsoft.EntityFrameworkCore;
using static AccraRoadAttendance.Models.Member;
using AccraRoadAttendance.Services;
using System.ComponentModel.DataAnnotations;
using System.IO;

namespace AccraRoadAttendance.Views.Pages.Members
{
    public partial class AddMembers : UserControl, INotifyPropertyChanged
    {
        private readonly AttendanceDbContext _context;
        private readonly INavigationService _navigationService;


        public AddMembers(AttendanceDbContext context, INavigationService navigationService)
        {
            InitializeComponent();
            _context = context;
            _navigationService = navigationService;
            DataContext = this;
            InitializeVisibility();

            // Initialize MaritalStatus ComboBox
            var maritalStatusItems = Enum.GetValues(typeof(Member.MaritalStatus))
                .Cast<Member.MaritalStatus>()
                .Select(s => new {
                    Value = s,
                    DisplayName = GetEnumDisplayName(s)
                }).ToList();

            MaritalStatusComboBox.ItemsSource = maritalStatusItems;
            MaritalStatusComboBox.DisplayMemberPath = "DisplayName";
            MaritalStatusComboBox.SelectedValuePath = "Value";

            // Initialize OccupationType ComboBox
            var occupationTypeItems = Enum.GetValues(typeof(Member.OccupationType))
                .Cast<Member.OccupationType>()
                .Select(ot => new {
                    Value = ot,
                    DisplayName = GetEnumDisplayName(ot)
                }).ToList();

            OccupationTypeComboBox.ItemsSource = occupationTypeItems;
            OccupationTypeComboBox.DisplayMemberPath = "DisplayName";
            OccupationTypeComboBox.SelectedValuePath = "Value";

            // Initialize Gender ComboBox
            var genderItems = Enum.GetValues(typeof(Member.Gender))
                .Cast<Member.Gender>()
                .Select(g => new
                {
                    Value = g,
                    DisplayName = GetEnumDisplayName(g)
                }).ToList();

            GenderComboBox.ItemsSource = genderItems;
            GenderComboBox.DisplayMemberPath = "DisplayName";
            GenderComboBox.SelectedValuePath = "Value";

            // Initialize Education ComboBox
            var educationItems = Enum.GetValues(typeof(Member.EducationalLevel))
                .Cast<Member.EducationalLevel>()
                .Select(g => new
                {
                    Value = g,
                    DisplayName = GetEnumDisplayName(g)
                }).ToList();

            EducationComboBox.ItemsSource = educationItems;
            EducationComboBox.DisplayMemberPath = "DisplayName";
            EducationComboBox.SelectedValuePath = "Value";
        }

        private void InitializeVisibility()
        {
            BaptismDatePicker.Visibility = Visibility.Collapsed;
            FamilyMemberNameTextBox.Visibility = Visibility.Collapsed;
            FamilyMemberContactTextBox.Visibility = Visibility.Collapsed;
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
        //public string Gender { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        // In AddMembers.cs (Change these properties to use enums)
        //public MaritalStatus maritalStatus { get; set; }
        //public OccupationType occupationType { get; set; }
       /* public Gender? Gender { get; set; }*/ // Also fix Gender (was a string before)
        public string? Hometown { get; set; }
        public string? NextOfKinName { get; set; }
        public string? NextOfKinContact { get; set; }
        public string? Skills { get; set; }
        public string? Address { get; set; }


        // New properties
        private DateTime? _dateOfBirth;
        public DateTime? DateOfBirth
        {
            get => _dateOfBirth;
            set { _dateOfBirth = value; OnPropertyChanged(nameof(DateOfBirth)); }
        }

        private string _nationality;
        public string Nationality
        {
            get => _nationality;
            set { _nationality = value; OnPropertyChanged(nameof(Nationality)); }
        }

        private bool _isBaptized;
        public bool IsBaptized
        {
            get => _isBaptized;
            set { _isBaptized = value; OnPropertyChanged(nameof(IsBaptized)); }
        }
        private DateTime? _baptismDate;
        public DateTime? BaptismDate
        {
            get => _baptismDate;
            set { _baptismDate = value; OnPropertyChanged(nameof(BaptismDate)); }
        }

        private bool _hasFamilyMemberInChurch;
        public bool HasFamilyMemberInChurch
        {
            get => _hasFamilyMemberInChurch;
            set { _hasFamilyMemberInChurch = value; OnPropertyChanged(nameof(HasFamilyMemberInChurch)); }
        }

        private string _familyMemberName;
        public string FamilyMemberName
        {
            get => _familyMemberName;
            set { _familyMemberName = value; OnPropertyChanged(nameof(FamilyMemberName)); }
        }

        private string _familyMemberContact;
        public string FamilyMemberContact
        {
            get => _familyMemberContact;
            set { _familyMemberContact = value; OnPropertyChanged(nameof(FamilyMemberContact)); }
        }

        private Gender? _gender;
        public Gender? Gender
        {
            get => _gender;
            set { _gender = value; OnPropertyChanged(nameof(Gender)); }
        }

        private MaritalStatus? _maritalStatus;
        public MaritalStatus? maritalStatus
        {
            get => _maritalStatus;
            set
            {
                _maritalStatus = value;
                OnPropertyChanged(nameof(maritalStatus));
            }
        }

        private OccupationType? _occupationType;
        public OccupationType? occupationType
        {
            get => _occupationType;
            set
            {
                _occupationType = value;
                OnPropertyChanged(nameof(occupationType));
            }
        }

        private EducationalLevel? _educationalLevel;
        public EducationalLevel? educationalLevel
        {
            get => _educationalLevel;
            set
            {
                _educationalLevel = value;
                OnPropertyChanged(nameof(educationalLevel));
            }
        }


        // Event handlers for visibility
        private void IsBaptizedCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            BaptismDatePicker.Visibility = Visibility.Visible;
        }

        private void IsBaptizedCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            BaptismDatePicker.Visibility = Visibility.Collapsed;
            BaptismDate = null;
        }

        private void FamilyMemberCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            FamilyMemberNameTextBox.Visibility = Visibility.Visible;
            FamilyMemberContactTextBox.Visibility = Visibility.Visible;
        }

        private void FamilyMemberCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            FamilyMemberNameTextBox.Visibility = Visibility.Collapsed;
            FamilyMemberContactTextBox.Visibility = Visibility.Collapsed;
            FamilyMemberName = string.Empty;
            FamilyMemberContact = string.Empty;
        }


        private static string GetEnumDisplayName(Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = (DisplayAttribute)Attribute.GetCustomAttribute(
                field, typeof(DisplayAttribute));
            return attribute?.Name ?? value.ToString();
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

        private async void SaveMember_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validate inputs
                if (string.IsNullOrWhiteSpace(FirstName) ||
                    string.IsNullOrWhiteSpace(LastName) ||
                    Gender == null ||
                     string.IsNullOrWhiteSpace(PhoneNumber))
                //string.IsNullOrWhiteSpace(SelectedPicturePath))

                {
                    MessageBox.Show("Please fill in all required fields.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                //// Convert Gender string to enum
                //if (!Enum.TryParse(Gender, true, out Gender parsedGender))
                //{
                //    MessageBox.Show("Invalid gender selection.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                //    return;
                //}

                // Create a new Member object
                var newMember = new Member
                {
                    FirstName = FirstName,
                    LastName = LastName,
                    OtherNames = OtherNames,
                    Sex = Gender.Value,
                    PhoneNumber = PhoneNumber,
                    Email = Email,
                    
                    // New fields
                    DateOfBirth = DateOfBirth,
                    Nationality = Nationality,
                    IsBaptized = IsBaptized,
                    BaptismDate = BaptismDate,
                    occupationType = occupationType.Value,
                    maritalStatus = maritalStatus.Value,
                    Hometown = Hometown,
                    NextOfKinName = NextOfKinName,
                    NextOfKinContact = NextOfKinContact,
                    Skills = Skills,
                    Address = Address,
                    educationalLevel = educationalLevel.Value,
                    HasFamilyMemberInChurch = HasFamilyMemberInChurch,
                    FamilyMemberName = FamilyMemberName,
                    FamilyMemberContact = FamilyMemberContact

                };

                // Generate filename
                string fileName = SanitizeFilename(newMember.FullName) + Path.GetExtension(SelectedPicturePath);
                string folderPath = Path.Combine(AppContext.BaseDirectory, "ProfilePictures");
                Directory.CreateDirectory(folderPath);
                string newFilePath = Path.Combine(folderPath, fileName);

                // Copy the selected image to the new filename
                if (!string.IsNullOrEmpty(SelectedPicturePath))
                {
                    File.Copy(SelectedPicturePath, newFilePath, true);
                }

                // Update PicturePath to the new filename path
                newMember.PicturePath = newFilePath;

                // Save to database
                _context.Members.Add(newMember);
                await _context.SaveChangesAsync();

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

        private string SanitizeFilename(string input)
        {
            // Remove invalid characters and replace spaces with underscores
            string valid = input.Replace(" ", "_")
                               .Replace("/", "_")
                               .Replace("\\", "_")
                               .Replace(":", "_")
                               .Replace("*", "_")
                               .Replace("?", "_")
                               .Replace("\"", "_")
                               .Replace("<", "_")
                               .Replace(">", "_")
                               .Replace("|", "_");
            return valid;
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
            Gender = null;
            PhoneNumber = string.Empty;
            Email = string.Empty;
            SelectedPicturePath = string.Empty;
            ImagePreview.Source = null;
            // Clear new fields
            DateOfBirth = null;
            Nationality = string.Empty;
            maritalStatus = null;
            occupationType = null;
            Hometown = string.Empty;
            NextOfKinName = string.Empty;
            NextOfKinContact = string.Empty;
            Skills = string.Empty;
            Address = string.Empty;
            educationalLevel = null;
            IsBaptized = false;
            BaptismDate = null;
            HasFamilyMemberInChurch = false;
            FamilyMemberName = string.Empty;
            FamilyMemberContact = string.Empty;

            // Notify the UI to update bound fields
            OnPropertyChanged(nameof(FirstName));
            OnPropertyChanged(nameof(LastName));
            OnPropertyChanged(nameof(OtherNames));
            OnPropertyChanged(nameof(Gender));
            OnPropertyChanged(nameof(PhoneNumber));
            OnPropertyChanged(nameof(Email));
            OnPropertyChanged(nameof(SelectedPicturePath));
            OnPropertyChanged(nameof(DateOfBirth));
            OnPropertyChanged(nameof(Nationality));
            OnPropertyChanged(nameof(maritalStatus));
            OnPropertyChanged(nameof(occupationType));
            OnPropertyChanged(nameof(IsBaptized));
            OnPropertyChanged(nameof(BaptismDate));
            OnPropertyChanged(nameof(HasFamilyMemberInChurch));
            OnPropertyChanged(nameof(FamilyMemberName));
            OnPropertyChanged(nameof(FamilyMemberContact));
            OnPropertyChanged(nameof(Skills));
            OnPropertyChanged(nameof(Address));
            OnPropertyChanged(nameof(educationalLevel));
            OnPropertyChanged(nameof(Hometown));
            OnPropertyChanged(nameof(NextOfKinName));
            OnPropertyChanged(nameof(NextOfKinContact));
        }
    }
}