﻿using AccraRoadAttendance.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using AccraRoadAttendance.Models;
using static AccraRoadAttendance.Models.Member;
using AccraRoadAttendance.Services;
using System.ComponentModel.DataAnnotations;

namespace AccraRoadAttendance.Views.Pages.Members
{
    public partial class EditMembers : UserControl, INotifyPropertyChanged, INotifyDataErrorInfo, IParameterReceiver
    {
        private Member _currentMember;
        private readonly AttendanceDbContext _context;
        private readonly INavigationService _navigationService;
        private readonly Dictionary<string, List<string>> _errors = new();
        private bool _pictureChanged = false;

        public EditMembers(AttendanceDbContext context, INavigationService navigationService)
        {
            InitializeComponent();
            _context = context;
            _navigationService = navigationService;
            DataContext = this;
            InitializeComboBoxes();
            InitializeVisibility();
        }

        #region Initialization Methods

        private void InitializeComboBoxes()
        {
            // Initialize Gender ComboBox
            var genderItems = Enum.GetValues(typeof(Gender))
                .Cast<Gender>()
                .Select(g => new
                {
                    Value = g,
                    DisplayName = GetEnumDisplayName(g)
                }).ToList();
            GenderComboBox.ItemsSource = genderItems;
            GenderComboBox.DisplayMemberPath = "DisplayName";
            GenderComboBox.SelectedValuePath = "Value";

            // Initialize MaritalStatus ComboBox
            var maritalStatusItems = Enum.GetValues(typeof(MaritalStatus))
                .Cast<MaritalStatus>()
                .Select(s => new
                {
                    Value = s,
                    DisplayName = GetEnumDisplayName(s)
                }).ToList();
            MaritalStatusComboBox.ItemsSource = maritalStatusItems;
            MaritalStatusComboBox.DisplayMemberPath = "DisplayName";
            MaritalStatusComboBox.SelectedValuePath = "Value";

            // Initialize OccupationType ComboBox
            var occupationTypeItems = Enum.GetValues(typeof(OccupationType))
                .Cast<OccupationType>()
                .Select(ot => new
                {
                    Value = ot,
                    DisplayName = GetEnumDisplayName(ot)
                }).ToList();
            OccupationTypeComboBox.ItemsSource = occupationTypeItems;
            OccupationTypeComboBox.DisplayMemberPath = "DisplayName";
            OccupationTypeComboBox.SelectedValuePath = "Value";

            // Initialize Education ComboBox
            var educationItems = Enum.GetValues(typeof(EducationalLevel))
                .Cast<EducationalLevel>()
                .Select(el => new
                {
                    Value = el,
                    DisplayName = GetEnumDisplayName(el)
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

        #endregion

        #region IParameterReceiver Implementation

        public void ReceiveParameter(object parameter)
        {
            if (parameter is Member member)
            {
                LoadMember(member);
            }
        }

        public void LoadMember(Member member)
        {
            _currentMember = member ?? throw new ArgumentNullException(nameof(member));
            FirstName = member.FirstName;
            LastName = member.LastName;
            OtherNames = member.OtherNames;
            Gender = member.Sex;
            PhoneNumber = member.PhoneNumber;
            Email = member.Email;
            DateOfBirth = member.DateOfBirth;
            Nationality = member.Nationality;
            maritalStatus = member.maritalStatus;
            occupationType = member.occupationType;
            Address = member.Address;
            NextOfKinName = member.NextOfKinName;
            NextOfKinContact = member.NextOfKinContact;
            Hometown = member.Hometown;
            Skills = member.Skills;
            educationalLevel = member.educationalLevel;
            IsBaptized = member.IsBaptized;
            BaptismDate = member.BaptismDate;
            HasFamilyMemberInChurch = member.HasFamilyMemberInChurch;
            FamilyMemberName = member.FamilyMemberName;
            FamilyMemberContact = member.FamilyMemberContact;
            SelectedPicturePath = member.PicturePath;
            ImagePreview.Source = string.IsNullOrEmpty(SelectedPicturePath) ? null : new BitmapImage(new Uri(SelectedPicturePath));
            _pictureChanged = false;

            // Notify UI of all property changes
            OnPropertyChanged(string.Empty);
        }

        #endregion

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

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
                _errors[propertyName] = errors;
            else
                _errors.Remove(propertyName);
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
            OnPropertyChanged(nameof(HasErrors));
        }

        #endregion

        #region Property Implementations with Validation

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

        private Gender? _gender;
        public Gender? Gender
        {
            get => _gender;
            set
            {
                if (_gender == value) return;
                _gender = value;
                ValidateGender();
                OnPropertyChanged(nameof(Gender));
            }
        }

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

        private DateTime? _dateOfBirth;
        public DateTime? DateOfBirth
        {
            get => _dateOfBirth;
            set
            {
                if (_dateOfBirth == value) return;
                _dateOfBirth = value;
                ValidateDateOfBirth();
                OnPropertyChanged(nameof(DateOfBirth));
            }
        }

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

        private MaritalStatus? _maritalStatus;
        public MaritalStatus? maritalStatus
        {
            get => _maritalStatus;
            set
            {
                if (_maritalStatus == value) return;
                _maritalStatus = value;
                ValidateMaritalStatus();
                OnPropertyChanged(nameof(maritalStatus));
            }
        }

        private OccupationType? _occupationType;
        public OccupationType? occupationType
        {
            get => _occupationType;
            set
            {
                if (_occupationType == value) return;
                _occupationType = value;
                ValidateOccupation();
                OnPropertyChanged(nameof(occupationType));
            }
        }

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

        private EducationalLevel? _educationalLevel;
        public EducationalLevel? educationalLevel
        {
            get => _educationalLevel;
            set
            {
                if (_educationalLevel == value) return;
                _educationalLevel = value;
                ValidateEducationLevel();
                OnPropertyChanged(nameof(educationalLevel));
            }
        }

        private bool _isBaptized;
        public bool IsBaptized
        {
            get => _isBaptized;
            set
            {
                if (_isBaptized == value) return;
                _isBaptized = value;
                ValidateBaptism();
                OnPropertyChanged(nameof(IsBaptized));
            }
        }

        private DateTime? _baptismDate;
        public DateTime? BaptismDate
        {
            get => _baptismDate;
            set
            {
                if (_baptismDate == value) return;
                _baptismDate = value;
                ValidateBaptism();
                OnPropertyChanged(nameof(BaptismDate));
            }
        }

        private bool _hasFamilyMemberInChurch;
        public bool HasFamilyMemberInChurch
        {
            get => _hasFamilyMemberInChurch;
            set
            {
                if (_hasFamilyMemberInChurch == value) return;
                _hasFamilyMemberInChurch = value;
                ValidateFamilyMember();
                OnPropertyChanged(nameof(HasFamilyMemberInChurch));
            }
        }

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

        private string _selectedPicturePath;
        public string SelectedPicturePath
        {
            get => _selectedPicturePath;
            set
            {
                if (_selectedPicturePath == value) return;
                _selectedPicturePath = value;
                ValidateProfilePicture();
                OnPropertyChanged(nameof(SelectedPicturePath));
            }
        }

        #endregion

        #region Validation Methods

        private void ValidateAll()
        {
            ValidateFirstName();
            ValidateLastName();
            ValidateGender();
            ValidatePhoneNumber();
            ValidateEmail();
            ValidateDateOfBirth();
            ValidateNationality();
            ValidateMaritalStatus();
            ValidateOccupation();
            ValidateAddress();
            ValidateNextOfKinName();
            ValidateNextOfKinContact();
            ValidateEducationLevel();
            ValidateBaptism();
            ValidateFamilyMember();
            ValidateProfilePicture();
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
            if (!string.IsNullOrWhiteSpace(Email) && !Regex.IsMatch(Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                errors.Add("Invalid Email format.");
            UpdateErrors(nameof(Email), errors);
        }

        private void ValidateDateOfBirth()
        {
            var errors = new List<string>();
            if (DateOfBirth == null || DateOfBirth > DateTime.Today)
                errors.Add("Date of Birth must be a valid past date.");
            UpdateErrors(nameof(DateOfBirth), errors);
        }

        private void ValidateNationality()
        {
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(Nationality))
                errors.Add("Nationality is required.");
            UpdateErrors(nameof(Nationality), errors);
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

        private void ValidateAddress()
        {
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(Address))
                errors.Add("Address is required.");
            UpdateErrors(nameof(Address), errors);
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

        private void ValidateEducationLevel()
        {
            var errors = new List<string>();
            if (educationalLevel == null)
                errors.Add("Education Level is required.");
            UpdateErrors(nameof(educationalLevel), errors);
        }

        private void ValidateBaptism()
        {
            var errors = new List<string>();
            if (IsBaptized && BaptismDate == null)
                errors.Add("Baptism Date is required if marked as baptized.");
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

        private void ValidateProfilePicture()
        {
            var errors = new List<string>();
            if (string.IsNullOrEmpty(SelectedPicturePath))
                errors.Add("Profile Picture is required.");
            UpdateErrors(nameof(SelectedPicturePath), errors);
        }

        #endregion

        #region Event Handlers

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
                _pictureChanged = true;
                ValidateProfilePicture();
            }
        }

        private async void UpdateMember_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ValidateAll();
                if (HasErrors)
                {
                    MessageBox.Show("Please correct all validation errors.", "Validation Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

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
                memberToUpdate.Sex = Gender.Value;
                memberToUpdate.PhoneNumber = PhoneNumber;
                memberToUpdate.Email = Email;
                memberToUpdate.DateOfBirth = DateOfBirth;
                memberToUpdate.Nationality = Nationality;
                memberToUpdate.maritalStatus = maritalStatus.Value;
                memberToUpdate.occupationType = occupationType.Value;
                memberToUpdate.Address = Address;
                memberToUpdate.NextOfKinName = NextOfKinName;
                memberToUpdate.NextOfKinContact = NextOfKinContact;
                memberToUpdate.Hometown = Hometown;
                memberToUpdate.Skills = Skills;
                memberToUpdate.educationalLevel = educationalLevel.Value;
                memberToUpdate.IsBaptized = IsBaptized;
                memberToUpdate.BaptismDate = IsBaptized ? BaptismDate : null;
                memberToUpdate.HasFamilyMemberInChurch = HasFamilyMemberInChurch;
                memberToUpdate.FamilyMemberName = HasFamilyMemberInChurch ? FamilyMemberName : null;
                memberToUpdate.FamilyMemberContact = HasFamilyMemberInChurch ? FamilyMemberContact : null;

                // Handle picture update
                if (_pictureChanged && !string.IsNullOrEmpty(SelectedPicturePath))
                {
                    string fileName = SanitizeFilename($"{FirstName} {LastName}") + Path.GetExtension(SelectedPicturePath);
                    string folderPath = Path.Combine(AppContext.BaseDirectory, "ProfilePictures");
                    Directory.CreateDirectory(folderPath);
                    string newFilePath = Path.Combine(folderPath, fileName);
                    File.Copy(SelectedPicturePath, newFilePath, true);
                    memberToUpdate.PicturePath = newFilePath;
                }

                await _context.SaveChangesAsync();
                MessageBox.Show("Member updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                _navigationService.NavigateTo<Members>();
            }
            catch (DbUpdateException dbEx)
            {
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
            _navigationService.NavigateTo<Members>();
        }

        #endregion

        #region Helper Methods

        private static string GetEnumDisplayName(Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = (DisplayAttribute)Attribute.GetCustomAttribute(field, typeof(DisplayAttribute));
            return attribute?.Name ?? value.ToString();
        }

        private string SanitizeFilename(string input)
        {
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

        #endregion
    }
}