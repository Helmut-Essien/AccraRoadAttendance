using AccraRoadAttendance.Data;
using AccraRoadAttendance.Models;
using AccraRoadAttendance.Services;
using MaterialDesignThemes.Wpf;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AccraRoadAttendance.Views.Pages.Users
{
    public partial class UsersManagement : UserControl
    {
        private readonly AttendanceDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly INavigationService _navigationService;
        private System.Windows.Threading.DispatcherTimer _searchTimer;
        private CancellationTokenSource _searchCts;
        private readonly ILogger<UsersManagement> _logger;
        private Member _selectedMember; // Add this at the class level

        public UsersManagement(AttendanceDbContext context, UserManager<User> userManager, RoleManager<IdentityRole> roleManager, ILogger<UsersManagement> logger)
        {
            InitializeComponent();
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Initialize debounce timer (300ms delay)
            _searchTimer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(300)
            };
            _searchTimer.Tick += SearchTimer_Tick;

            _searchCts = new CancellationTokenSource();

            _ = LoadData();

            // Subscribe to Unloaded for cleanup
            this.Unloaded += UsersManagement_Unloaded;
        }

        private async Task LoadData()
        {
            // Clear existing data
            usersDataGrid.ItemsSource = null;

            //// Load roles
            //roleComboBox.ItemsSource = await _roleManager.Roles.ToListAsync();
            // Load roles once
            var allRoles = await _roleManager.Roles.ToListAsync();
            roleComboBox.ItemsSource = allRoles;

            // Load existing users
            var users = await _context.Users
                .Include(u => u.Member)
                //.AsNoTracking()
                .ToListAsync();

            //var userViewModels = new List<UserViewModel>();
            var userViewModels = new ObservableCollection<UserViewModel>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                //var allRoles = await _roleManager.Roles.ToListAsync();

                userViewModels.Add(new UserViewModel
                {
                    User = user,
                    Roles = allRoles,
                    SelectedRole = allRoles.FirstOrDefault(r => r.Name == roles.FirstOrDefault())
                });
            }

            usersDataGrid.ItemsSource = userViewModels;
            // Force UI update
            usersDataGrid.Items.Refresh();
        }

        private async void SearchTimer_Tick(object sender, EventArgs e)
        {
            _searchTimer.Stop(); // Prevent re-triggering

            if (_searchCts.IsCancellationRequested)
                return;

            var token = _searchCts.Token;
            var searchText = searchTextBox.Text.ToLower();

            try
            {
                //var members = await _context.Members
                //    .Where(m => !_context.Users.Any(u => u.MemberId == m.Id) &&
                //               (m.FirstName.ToLower().Contains(searchText) ||
                //                m.LastName.ToLower().Contains(searchText) ||
                //                (m.OtherNames != null && m.OtherNames.ToLower().Contains(searchText)) ||
                //                m.PhoneNumber.Contains(searchText) ||
                //                m.Email.Contains(searchText)))
                //    .ToListAsync(token);
                var userMemberIds = await _context.Users.Select(u => u.MemberId).ToListAsync(token);
                var members = await _context.Members
                    .Where(m => !userMemberIds.Contains(m.Id) &&
                               (m.FirstName.ToLower().Contains(searchText) ||
                                m.LastName.ToLower().Contains(searchText) ||
                                (m.OtherNames != null && m.OtherNames.ToLower().Contains(searchText)) ||
                                m.PhoneNumber.Contains(searchText) ||
                                m.Email.Contains(searchText)))
                    .ToListAsync(token);

                membersListBox.ItemsSource = members;
            }
            catch (OperationCanceledException)
            {
                // Search was canceled (e.g., user typed more); ignore and let the new search handle it
            }
            catch (Exception ex)
            {
                // Handle other errors (e.g., log or show message)
                System.Diagnostics.Debug.WriteLine($"Search error: {ex.Message}");
            }
        }

        private async void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            //var searchText = searchTextBox.Text.ToLower();
            //var members = await _context.Members
            //    .Where(m => !_context.Users.Any(u => u.MemberId == m.Id) &&
            //               (m.FirstName.ToLower().Contains(searchText) ||
            //                m.LastName.ToLower().Contains(searchText) ||
            //                (m.OtherNames != null && m.OtherNames.ToLower().Contains(searchText)) ||
            //                m.PhoneNumber.Contains(searchText) ||
            //                m.Email.Contains(searchText)))
            //    .ToListAsync();

            //membersListBox.ItemsSource = members;

            // Cancel any ongoing search
            _searchCts?.Cancel();
            _searchCts?.Dispose();
            _searchCts = new CancellationTokenSource();

            // Restart debounce timer
            _searchTimer.Stop();
            _searchTimer.Start();
        }

        private void MembersListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (membersListBox.SelectedItem is Member selectedMember)
            {
                _selectedMember = selectedMember; // Store the selected member
                userFormPanel.Visibility = Visibility.Visible;
                emailTextBox.Text = _selectedMember.Email ?? string.Empty;
                emailTextBox.IsEnabled = string.IsNullOrEmpty(_selectedMember.Email);
                //membersListBox.SelectedItem = selectedMember;

                // Clear search suggestions
                membersListBox.Visibility = Visibility.Collapsed;
                searchTextBox.Text = selectedMember.FullName ?? string.Empty;
            }
        }

        private async void AddUser_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //if (membersListBox.SelectedItem is not Member selectedMember ||
                //    roleComboBox.SelectedItem is not IdentityRole selectedRole)
                //{
                //    MessageBox.Show("Please select a member and role");
                //    return;
                //}
                if (_selectedMember == null)
                {
                    MessageBox.Show("Please select a member");
                    return;
                }
                if ( roleComboBox.SelectedItem is not IdentityRole selectedRole)
                {
                    MessageBox.Show("Please select role");
                    return;
                }

                if (string.IsNullOrWhiteSpace(passwordBox.Password))
                {
                    MessageBox.Show("Password is required");
                    return;
                }

                var email = emailTextBox.Text.Trim();
                if (string.IsNullOrWhiteSpace(email))
                {
                    MessageBox.Show("Email is required");
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"Attempting to create user with email: {email}");
                System.Diagnostics.Debug.WriteLine($"Selected member ID: {_selectedMember.Id}");
                System.Diagnostics.Debug.WriteLine($"Selected role: {selectedRole.Name}");

                var user = new User
                {
                    UserName = email,
                    Email = email,
                    MemberId = _selectedMember.Id
                };

                System.Diagnostics.Debug.WriteLine("User object created:");
                System.Diagnostics.Debug.WriteLine($"UserName: {user.UserName}");
                System.Diagnostics.Debug.WriteLine($"Email: {user.Email}");
                System.Diagnostics.Debug.WriteLine($"MemberId: {user.MemberId}");

                var result = await _userManager.CreateAsync(user, passwordBox.Password);

                if (result.Succeeded)
                {
                    System.Diagnostics.Debug.WriteLine("User created successfully, adding role...");
                    var roleResult = await _userManager.AddToRoleAsync(user, selectedRole.Name);

                    if (roleResult.Succeeded)
                    {
                        System.Diagnostics.Debug.WriteLine("Role added successfully");
                        MessageBox.Show("User created successfully");
                        ClearForm();
                        await LoadData();
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Failed to add role:");
                        foreach (var error in roleResult.Errors)
                        {
                            System.Diagnostics.Debug.WriteLine($"Code: {error.Code}, Description: {error.Description}");
                        }
                        MessageBox.Show($"Error adding role: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("User creation failed:");
                    foreach (var error in result.Errors)
                    {
                        System.Diagnostics.Debug.WriteLine($"Code: {error.Code}, Description: {error.Description}");
                    }
                    MessageBox.Show($"Error creating user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            catch (DbUpdateException dbEx)
            {
                System.Diagnostics.Debug.WriteLine("Database update error:");
                System.Diagnostics.Debug.WriteLine(dbEx.Message);
                if (dbEx.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine("Inner exception:");
                    System.Diagnostics.Debug.WriteLine(dbEx.InnerException.Message);
                }
                MessageBox.Show("A database error occurred while creating the user");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("General error:");
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                MessageBox.Show($"An unexpected error occurred: {ex.Message}");
            }
        }

        //private async void SaveRole_Click(object sender, RoutedEventArgs e)
        //{
        //    if (((FrameworkElement)sender).DataContext is UserViewModel viewModel &&
        //        viewModel.SelectedRole != null)
        //    {
        //        var currentRoles = await _userManager.GetRolesAsync(viewModel.User);
        //        await _userManager.RemoveFromRolesAsync(viewModel.User, currentRoles);
        //        await _userManager.AddToRoleAsync(viewModel.User, viewModel.SelectedRole.Name);
        //        MessageBox.Show("Role updated successfully");
        //    }
        //}



        private async void SaveRole_Click(object sender, RoutedEventArgs e)
        {
            if (((FrameworkElement)sender).DataContext is UserViewModel viewModel &&
                viewModel.SelectedRole != null)
            {
                _logger.LogInformation("Starting role update for user {UserId}", viewModel.User.Id);
                _logger.LogInformation("Selected role in UI: {SelectedRole}", viewModel.SelectedRole.Name);

                // Get properly tracked user instance
                var user = await _userManager.FindByIdAsync(viewModel.User.Id);
                if (user == null)
                {
                    _logger.LogWarning("User {UserId} not found in database", viewModel.User.Id);
                    MessageBox.Show("User not found");
                    return;
                }

                // Get current roles
                var currentRoles = await _userManager.GetRolesAsync(user);
                _logger.LogInformation("Current roles for user {UserId}: {Roles}",
                    user.Id, string.Join(", ", currentRoles));

                // Remove existing roles
                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!removeResult.Succeeded)
                {
                    _logger.LogError("Failed to remove roles for user {UserId}. Errors: {Errors}",
                        user.Id, string.Join(", ", removeResult.Errors.Select(e => e.Description)));
                    MessageBox.Show($"Error removing roles: {string.Join(", ", removeResult.Errors.Select(e => e.Description))}");
                    return;
                }
                _logger.LogInformation("Successfully removed all roles for user {UserId}", user.Id);

                // Add new role
                var addResult = await _userManager.AddToRoleAsync(user, viewModel.SelectedRole.Name);
                if (!addResult.Succeeded)
                {
                    _logger.LogError("Failed to add role {Role} to user {UserId}. Errors: {Errors}",
                        viewModel.SelectedRole.Name, user.Id,
                        string.Join(", ", addResult.Errors.Select(e => e.Description)));
                    MessageBox.Show($"Error adding role: {string.Join(", ", addResult.Errors.Select(e => e.Description))}");
                    return;
                }
                _logger.LogInformation("Successfully added role {Role} to user {UserId}",
                    viewModel.SelectedRole.Name, user.Id);

                // Verify changes
                var updatedRoles = await _userManager.GetRolesAsync(user);
                _logger.LogInformation("Roles after update for user {UserId}: {Roles}",
                    user.Id, string.Join(", ", updatedRoles));

                if (updatedRoles.Count == 1 && updatedRoles[0] == viewModel.SelectedRole.Name)
                {
                    _logger.LogInformation("Role update successful for user {UserId}", user.Id);
                    MessageBox.Show("Role updated successfully");
                    await LoadData();
                }
                else
                {
                    _logger.LogWarning("Role update verification failed for user {UserId}. " +
                        "Expected: {ExpectedRole}, Actual: {ActualRoles}",
                        user.Id, viewModel.SelectedRole.Name, string.Join(", ", updatedRoles));
                    MessageBox.Show($"Role update failed verification. Current roles: {string.Join(", ", updatedRoles)}");
                }
            }
        }

        private async void DeleteUser_Click(object sender, RoutedEventArgs e)
        {
            if (((FrameworkElement)sender).DataContext is UserViewModel viewModel)
            {
                if (MessageBox.Show("Delete this user?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    // Get properly tracked user
                    var user = await _userManager.FindByIdAsync(viewModel.User.Id);
                    if (user == null)
                    {
                        MessageBox.Show("User not found");
                        return;
                    }

                    var result = await _userManager.DeleteAsync(user);
                    if (result.Succeeded)
                    {
                        MessageBox.Show("User deleted successfully");
                        await LoadData();
                    }
                    else
                    {
                        MessageBox.Show($"Error deleting user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }
            }
        }

        private void TogglePasswordVisibility_Checked(object sender, RoutedEventArgs e)
        {
            // Copy password from PasswordBox to TextBox
            passwordTextBox.Text = passwordBox.Password;
            // Hide PasswordBox, show TextBox
            passwordBox.Visibility = Visibility.Collapsed;
            passwordTextBox.Visibility = Visibility.Visible;
            // Change icon to "EyeOff"
            (togglePasswordVisibility.Content as PackIcon).Kind = PackIconKind.EyeOff;
            // Set focus to TextBox
            passwordTextBox.Focus();
        }

        private void TogglePasswordVisibility_Unchecked(object sender, RoutedEventArgs e)
        {
            // Copy text from TextBox to PasswordBox
            passwordBox.Password = passwordTextBox.Text;
            // Hide TextBox, show PasswordBox
            passwordTextBox.Visibility = Visibility.Collapsed;
            passwordBox.Visibility = Visibility.Visible;
            // Change icon to "Eye"
            (togglePasswordVisibility.Content as PackIcon).Kind = PackIconKind.Eye;
            // Set focus to PasswordBox
            passwordBox.Focus();
        }

        private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
        {
            // Optional: Sync TextBox if needed during key input
            if (passwordTextBox.Visibility == Visibility.Visible)
            {
                passwordTextBox.Text = passwordBox.Password;
            }
        }

        private void PasswordTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            // Optional: Sync PasswordBox if needed during key input
            if (passwordBox.Visibility == Visibility.Visible)
            {
                passwordBox.Password = passwordTextBox.Text;
            }
        }

        //private async void DeleteUser_Click(object sender, RoutedEventArgs e)
        //{
        //    if (((FrameworkElement)sender).DataContext is UserViewModel viewModel)
        //    {
        //        if (MessageBox.Show("Delete this user?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
        //        {
        //            await _userManager.DeleteAsync(viewModel.User);
        //            LoadData();
        //            MessageBox.Show("User deleted successfully");
        //        }
        //    }
        //}

        private void ClearForm()
        {
            membersListBox.SelectedItem = null;
            userFormPanel.Visibility = Visibility.Collapsed;
            //passwordBox.Clear();
            passwordBox.Password = string.Empty;
            passwordTextBox.Text = string.Empty;
            searchTextBox.Text = string.Empty;
            roleComboBox.Text = string.Empty;
            emailTextBox.Clear();
        }

        //public class UserViewModel
        //{
        //    public User User { get; set; }
        //    public List<IdentityRole> Roles { get; set; }
        //    public IdentityRole SelectedRole { get; set; }
        //}

        

        public class UserViewModel : INotifyPropertyChanged
        {
            private User _user;
            private IdentityRole _selectedRole;

            public User User
            {
                get => _user;
                set
                {
                    _user = value;
                    OnPropertyChanged(nameof(User));
                }
            }

            public List<IdentityRole> Roles { get; set; }

            public IdentityRole SelectedRole
            {
                get => _selectedRole;
                set
                {
                    _selectedRole = value;
                    OnPropertyChanged(nameof(SelectedRole));
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void UsersManagement_Unloaded(object sender, RoutedEventArgs e)
        {
            _searchTimer.Stop();
            _searchTimer.Tick -= SearchTimer_Tick; // Unsubscribe to prevent leaks
            _searchCts?.Cancel();
            _searchCts?.Dispose();
            _searchCts = null; // Optional: null out to aid GC
        }

        // Optional: Add destructor as a fallback (add this at the class level)
        //~UsersManagement()
        //{
        //    _searchCts?.Dispose();
        //}
    }

}