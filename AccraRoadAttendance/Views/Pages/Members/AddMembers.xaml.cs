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
using System.Text.RegularExpressions;

namespace AccraRoadAttendance.Views.Pages.Members
{
    public partial class AddMembers : UserControl, INotifyPropertyChanged, INotifyDataErrorInfo
    {

        private readonly AttendanceDbContext _context;
        private readonly INavigationService _navigationService;
        private readonly Dictionary<string, List<string>> _errors = new();
        private readonly Dictionary<string, List<string>> _validationErrors = new();


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

        #region INotifyDataErrorInfo Implementation
        public bool HasErrors => _errors.Any();

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public System.Collections.IEnumerable GetErrors(string propertyName)
        {
            return _errors.GetValueOrDefault(propertyName, new List<string>());
        }

        private void UpdateErrors(string propertyName, List<string> errors)
        {
            if (errors.Any())
            {
                _errors[propertyName] = errors; // Add errors if any
            }
            else
            {
                _errors.Remove(propertyName); // Remove entry if no errors
            }

            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
            OnPropertyChanged(nameof(HasErrors));
        }
        #endregion

        #region Validation Methods
        private void ValidateAll()
        {
            ValidateFirstName();
            ValidateLastName();
            ValidateGender();
            ValidatePhoneNumber();
            ValidateMaritalStatus();
            ValidateOccupation();
            ValidateEducationLevel();
            ValidateNationality();
            ValidateAddress();
            //ValidateProfilePicture();
            ValidateDateOfBirth();
            ValidateEmail();
            ValidateBaptism();
            ValidateNextOfKinName();
            ValidateNextOfKinContact();
            ValidateFamilyMember();
        }

        private void ValidateFirstName()
        {
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(FirstName))
                errors.Add("First Name is required.");
            UpdateErrors(nameof(FirstName), errors);
        }

        private void ValidateLastName()
        {
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(LastName))
                errors.Add("Last Name is required.");
            UpdateErrors(nameof(LastName), errors);
        }

        private void ValidateGender()
        {
            var errors = new List<string>();
            if (Gender == null)
                errors.Add("Gender is required.");
            UpdateErrors(nameof(Gender), errors);
        }

        private void ValidatePhoneNumber()
        {
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(PhoneNumber))
                errors.Add("Phone Number is required.");
            else if (!Regex.IsMatch(PhoneNumber, @"^\d{10}$"))
                errors.Add("Phone Number must be 10 digits.");
            UpdateErrors(nameof(PhoneNumber), errors);
        }

        private void ValidateEmail()
        {
            var errors = new List<string>();
            if (!string.IsNullOrWhiteSpace(Email) &&
                !Regex.IsMatch(Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                errors.Add("Invalid Email format.");
            UpdateErrors(nameof(Email), errors);
        }

        private void ValidateMaritalStatus()
        {
            var errors = new List<string>();
            if (maritalStatus == null)
                errors.Add("Marital Status is required.");
            UpdateErrors(nameof(maritalStatus), errors);
        }

        private void ValidateOccupation()
        {
            var errors = new List<string>();
            if (occupationType == null)
                errors.Add("Occupation is required.");
            UpdateErrors(nameof(occupationType), errors);
        }

        private void ValidateEducationLevel()
        {
            var errors = new List<string>();
            if (educationalLevel == null)
                errors.Add("Education Level is required.");
            UpdateErrors(nameof(educationalLevel), errors);
        }

        private void ValidateNationality()
        {
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(Nationality))
                errors.Add("Nationality is required.");
            UpdateErrors(nameof(Nationality), errors);
        }

        private void ValidateAddress()
        {
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(Address))
                errors.Add("Address is required.");
            UpdateErrors(nameof(Address), errors);
        }

        //private void ValidateProfilePicture()
        //{
        //    var errors = new List<string>();
        //    if (string.IsNullOrEmpty(SelectedPicturePath))
        //        errors.Add("Profile Picture is required.");
        //    UpdateErrors(nameof(SelectedPicturePath), errors);
        //}

        private void ValidateDateOfBirth()
        {
            var errors = new List<string>();
            if (DateOfBirth == null)
                errors.Add("Date of Birth is required");
            else if (DateOfBirth > DateTime.Today)
                errors.Add("Date of Birth must be a valid past date."); 
            UpdateErrors(nameof(DateOfBirth), errors);
        }

        private void ValidateNextOfKinName()
        {
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(NextOfKinName))
                errors.Add("Next of Kin Name is required.");
            UpdateErrors(nameof(NextOfKinName), errors);
        }

        private void ValidateNextOfKinContact()
        {
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(NextOfKinContact))
                errors.Add("Next of Kin Contact is required.");
            UpdateErrors(nameof(NextOfKinContact), errors);
        }

        private void ValidateBaptism()
        {
            var errors = new List<string>();
            if (IsBaptized && BaptismDate == null)
                errors.Add("Baptism Date is required if marked as baptized.");
            else if (IsBaptized && BaptismDate > DateTime.Today)
                errors.Add("Date of Baptism must be a valid past date.");
            UpdateErrors(nameof(BaptismDate), errors);
        }

        private void ValidateFamilyMember()
        {
            var nameErrors = new List<string>();
            var contactErrors = new List<string>();
            if (HasFamilyMemberInChurch)
            {
                if (string.IsNullOrWhiteSpace(FamilyMemberName))
                    nameErrors.Add("Family Member Name is required.");
                if (string.IsNullOrWhiteSpace(FamilyMemberContact))
                    contactErrors.Add("Family Member Contact is required.");
            }
            UpdateErrors(nameof(FamilyMemberName), nameErrors);
            UpdateErrors(nameof(FamilyMemberContact), contactErrors);
        }
        #endregion

        #region Property Implementations with Validation

        // 1. First Name
        private string _firstName;
        public string FirstName
        {
            get => _firstName;
            set
            {
                if (_firstName == value) return;
                _firstName = value;
                ValidateFirstName();
                OnPropertyChanged(nameof(FirstName));
            }
        }

        // 2. Last Name
        private string _lastName;
        public string LastName
        {
            get => _lastName;
            set
            {
                if (_lastName == value) return;
                _lastName = value;
                ValidateLastName();
                OnPropertyChanged(nameof(LastName));
            }
        }

        // 3. Other Names
        private string _otherNames;
        public string OtherNames
        {
            get => _otherNames;
            set
            {
                if (_otherNames == value) return;
                _otherNames = value;
                OnPropertyChanged(nameof(OtherNames));
            }
        }

        // 4. Gender (Enum)
        private Gender? _gender;
        public Gender? Gender
        {
            get => _gender;
            set
            {
                _gender = value;
                ValidateGender();
                OnPropertyChanged(nameof(Gender));
            }
        }

        // 5. Phone Number
        private string _phoneNumber;
        public string PhoneNumber
        {
            get => _phoneNumber;
            set
            {
                if (_phoneNumber == value) return;
                _phoneNumber = value;
                ValidatePhoneNumber();
                OnPropertyChanged(nameof(PhoneNumber));
            }
        }

        // 6. Email
        private string _email;
        public string Email
        {
            get => _email;
            set
            {
                if (_email == value) return;
                _email = value;
                ValidateEmail();
                OnPropertyChanged(nameof(Email));
            }
        }

        // 7. Nationality
        private string _nationality;
        public string Nationality
        {
            get => _nationality;
            set
            {
                if (_nationality == value) return;
                _nationality = value;
                ValidateNationality();
                OnPropertyChanged(nameof(Nationality));
            }
        }

        // 8. Address
        private string _address;
        public string Address
        {
            get => _address;
            set
            {
                if (_address == value) return;
                _address = value;
                ValidateAddress();
                OnPropertyChanged(nameof(Address));
            }
        }

        // 9. Date of Birth (DateTime?)
        private DateTime? _dateOfBirth;
        public DateTime? DateOfBirth
        {
            get => _dateOfBirth;
            set
            {
                _dateOfBirth = value;
                ValidateDateOfBirth();
                OnPropertyChanged(nameof(DateOfBirth));
            }
        }

        // 10. Marital Status (Enum)
        private MaritalStatus? _maritalStatus;
        public MaritalStatus? maritalStatus
        {
            get => _maritalStatus;
            set
            {
                _maritalStatus = value;
                ValidateMaritalStatus();
                OnPropertyChanged(nameof(maritalStatus));
            }
        }

        // 11. Occupation Type (Enum)
        private OccupationType? _occupationType;
        public OccupationType? occupationType
        {
            get => _occupationType;
            set
            {
                _occupationType = value;
                ValidateOccupation();
                OnPropertyChanged(nameof(occupationType));
            }
        }

        // 12. Educational Level (Enum)
        private EducationalLevel? _educationalLevel;
        public EducationalLevel? educationalLevel
        {
            get => _educationalLevel;
            set
            {
                _educationalLevel = value;
                ValidateEducationLevel();
                OnPropertyChanged(nameof(educationalLevel));
            }
        }

        // 13. Is Baptized (Checkbox)
        private bool _isBaptized;
        public bool IsBaptized
        {
            get => _isBaptized;
            set
            {
                _isBaptized = value;
                ValidateBaptism();
                OnPropertyChanged(nameof(IsBaptized));
            }
        }

        // 14. Baptism Date (DateTime?)
        private DateTime? _baptismDate;
        public DateTime? BaptismDate
        {
            get => _baptismDate;
            set
            {
                _baptismDate = value;
                ValidateBaptism();
                OnPropertyChanged(nameof(BaptismDate));
            }
        }

        // 15. Has Family Member In Church (Checkbox)
        private bool _hasFamilyMemberInChurch;
        public bool HasFamilyMemberInChurch
        {
            get => _hasFamilyMemberInChurch;
            set
            {
                _hasFamilyMemberInChurch = value;
                ValidateFamilyMember();
                OnPropertyChanged(nameof(HasFamilyMemberInChurch));
            }
        }

        // 16. Family Member Name
        private string _familyMemberName;
        public string FamilyMemberName
        {
            get => _familyMemberName;
            set
            {
                if (_familyMemberName == value) return;
                _familyMemberName = value;
                ValidateFamilyMember();
                OnPropertyChanged(nameof(FamilyMemberName));
            }
        }

        // 17. Family Member Contact
        private string _familyMemberContact;
        public string FamilyMemberContact
        {
            get => _familyMemberContact;
            set
            {
                if (_familyMemberContact == value) return;
                _familyMemberContact = value;
                ValidateFamilyMember();
                OnPropertyChanged(nameof(FamilyMemberContact));
            }
        }

        // 18. Skills
        private string _skills;
        public string Skills
        {
            get => _skills;
            set
            {
                if (_skills == value) return;
                _skills = value;
                OnPropertyChanged(nameof(Skills));
            }
        }


        // 19. Hometown
        private string _hometown;
        public string Hometown
        {
            get => _hometown;
            set
            {
                if (_hometown == value) return;
                _hometown = value;
                OnPropertyChanged(nameof(Hometown));
            }
        }

        // 20. Next of Kin Name
        private string _nextOfKinName;
        public string NextOfKinName
        {
            get => _nextOfKinName;
            set
            {
                if (_nextOfKinName == value) return;
                _nextOfKinName = value;
                ValidateNextOfKinName();
                OnPropertyChanged(nameof(NextOfKinName));
            }
        }

        // 21. Next of Kin Contact
        private string _nextOfKinContact;
        public string NextOfKinContact
        {
            get => _nextOfKinContact;
            set
            {
                if (_nextOfKinContact == value) return;
                _nextOfKinContact = value;
                ValidateNextOfKinContact();
                OnPropertyChanged(nameof(NextOfKinContact));
            }
        }

        // 22. Profile Picture Path
        private string _selectedPicturePath;
        public string SelectedPicturePath
        {
            get => _selectedPicturePath;
            set
            {
                _selectedPicturePath = value;
                //ValidateProfilePicture();
                OnPropertyChanged(nameof(SelectedPicturePath));
            }
        }

        #endregion

        #region Event Handlers
        private void InitializeVisibility()
        {
            BaptismDatePicker.Visibility = Visibility.Collapsed;
            FamilyMemberNameTextBox.Visibility = Visibility.Collapsed;
            FamilyMemberContactTextBox.Visibility = Visibility.Collapsed;
        }
        private void IsBaptizedCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            BaptismDatePicker.Visibility = Visibility.Visible;
            ValidateBaptism();
        }

        private void IsBaptizedCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            BaptismDatePicker.Visibility = Visibility.Collapsed;
            BaptismDate = null;
            ValidateBaptism();
        }

        private void FamilyMemberCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            FamilyMemberNameTextBox.Visibility = Visibility.Visible;
            FamilyMemberContactTextBox.Visibility = Visibility.Visible;
            ValidateFamilyMember();
        }

        private void FamilyMemberCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            FamilyMemberNameTextBox.Visibility = Visibility.Collapsed;
            FamilyMemberContactTextBox.Visibility = Visibility.Collapsed;
            FamilyMemberName = string.Empty;
            FamilyMemberContact = string.Empty;
            ValidateFamilyMember();
        }

        #endregion


        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

       // private string _selectedPicturePath;
       // public string SelectedPicturePath
       // {
       //     get => _selectedPicturePath;
       //     set
       //     {
       //         _selectedPicturePath = value;
       //         OnPropertyChanged(nameof(SelectedPicturePath));
       //     }
       // }

       // // Bindable properties for form inputs
       // public string FirstName { get; set; }
       // public string LastName { get; set; }
       // public string OtherNames { get; set; }
       // //public string Gender { get; set; }
       // public string PhoneNumber { get; set; }
       // public string Email { get; set; }
       // // In AddMembers.cs (Change these properties to use enums)
       // //public MaritalStatus maritalStatus { get; set; }
       // //public OccupationType occupationType { get; set; }
       ///* public Gender? Gender { get; set; }*/ // Also fix Gender (was a string before)
       // public string? Hometown { get; set; }
       // public string? NextOfKinName { get; set; }
       // public string? NextOfKinContact { get; set; }
       // public string? Skills { get; set; }
       // public string? Address { get; set; }


       // // New properties
       // private DateTime? _dateOfBirth;
       // public DateTime? DateOfBirth
       // {
       //     get => _dateOfBirth;
       //     set { _dateOfBirth = value; OnPropertyChanged(nameof(DateOfBirth)); }
       // }

       // private string _nationality;
       // public string Nationality
       // {
       //     get => _nationality;
       //     set { _nationality = value; OnPropertyChanged(nameof(Nationality)); }
       // }

       // private bool _isBaptized;
       // public bool IsBaptized
       // {
       //     get => _isBaptized;
       //     set { _isBaptized = value; OnPropertyChanged(nameof(IsBaptized)); }
       // }
       // private DateTime? _baptismDate;
       // public DateTime? BaptismDate
       // {
       //     get => _baptismDate;
       //     set { _baptismDate = value; OnPropertyChanged(nameof(BaptismDate)); }
       // }

       // private bool _hasFamilyMemberInChurch;
       // public bool HasFamilyMemberInChurch
       // {
       //     get => _hasFamilyMemberInChurch;
       //     set { _hasFamilyMemberInChurch = value; OnPropertyChanged(nameof(HasFamilyMemberInChurch)); }
       // }

       // private string _familyMemberName;
       // public string FamilyMemberName
       // {
       //     get => _familyMemberName;
       //     set { _familyMemberName = value; OnPropertyChanged(nameof(FamilyMemberName)); }
       // }

       // private string _familyMemberContact;
       // public string FamilyMemberContact
       // {
       //     get => _familyMemberContact;
       //     set { _familyMemberContact = value; OnPropertyChanged(nameof(FamilyMemberContact)); }
       // }

       // private Gender? _gender;
       // public Gender? Gender
       // {
       //     get => _gender;
       //     set { _gender = value; OnPropertyChanged(nameof(Gender)); }
       // }

       // private MaritalStatus? _maritalStatus;
       // public MaritalStatus? maritalStatus
       // {
       //     get => _maritalStatus;
       //     set
       //     {
       //         _maritalStatus = value;
       //         OnPropertyChanged(nameof(maritalStatus));
       //     }
       // }

       // private OccupationType? _occupationType;
       // public OccupationType? occupationType
       // {
       //     get => _occupationType;
       //     set
       //     {
       //         _occupationType = value;
       //         OnPropertyChanged(nameof(occupationType));
       //     }
       // }

       // private EducationalLevel? _educationalLevel;
       // public EducationalLevel? educationalLevel
       // {
       //     get => _educationalLevel;
       //     set
       //     {
       //         _educationalLevel = value;
       //         OnPropertyChanged(nameof(educationalLevel));
       //     }
       // }


       // // Event handlers for visibility
       // private void IsBaptizedCheckBox_Checked(object sender, RoutedEventArgs e)
       // {
       //     BaptismDatePicker.Visibility = Visibility.Visible;
       // }

       // private void IsBaptizedCheckBox_Unchecked(object sender, RoutedEventArgs e)
       // {
       //     BaptismDatePicker.Visibility = Visibility.Collapsed;
       //     BaptismDate = null;
       // }

       // private void FamilyMemberCheckBox_Checked(object sender, RoutedEventArgs e)
       // {
       //     FamilyMemberNameTextBox.Visibility = Visibility.Visible;
       //     FamilyMemberContactTextBox.Visibility = Visibility.Visible;
       // }

       // private void FamilyMemberCheckBox_Unchecked(object sender, RoutedEventArgs e)
       // {
       //     FamilyMemberNameTextBox.Visibility = Visibility.Collapsed;
       //     FamilyMemberContactTextBox.Visibility = Visibility.Collapsed;
       //     FamilyMemberName = string.Empty;
       //     FamilyMemberContact = string.Empty;
       // }


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
                // Trigger all validations
                ValidateAll();
                // Validate inputs
                //if (string.IsNullOrWhiteSpace(FirstName) ||
                //    string.IsNullOrWhiteSpace(LastName) ||
                //    Gender == null ||
                //     string.IsNullOrWhiteSpace(PhoneNumber))
                ////string.IsNullOrWhiteSpace(SelectedPicturePath))

                //{
                //    MessageBox.Show("Please fill in all required fields.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                //    return;
                //}
                if (HasErrors)
                {
                    MessageBox.Show("Please correct all validation errors.",
                        "Validation Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

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

                // Process the profile picture **only if a file is selected**
                if (!string.IsNullOrEmpty(SelectedPicturePath))
                {
                    // Generate filename
                    string fileName = SanitizeFilename(newMember.FullName) + Path.GetExtension(SelectedPicturePath);
                    string folderPath = Path.Combine(AppContext.BaseDirectory, "ProfilePictures");
                    Directory.CreateDirectory(folderPath);
                    string newFilePath = Path.Combine(folderPath, fileName);

                    // Copy the selected image
                    File.Copy(SelectedPicturePath, newFilePath, true);

                    // Update PicturePath
                    newMember.PicturePath = newFilePath;
                }
                else
                {
                    // Set to null or a default value if no image is selected
                    newMember.PicturePath = null; // Or a default image path
                }

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

            _errors.Clear();
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(string.Empty));

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