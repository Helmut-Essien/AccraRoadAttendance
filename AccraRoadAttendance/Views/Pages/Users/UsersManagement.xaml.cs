using AccraRoadAttendance.Data;
using AccraRoadAttendance.Models;
using AccraRoadAttendance.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AccraRoadAttendance.Views.Pages.Users
{
    public partial class UsersManagement : UserControl
    {
        private readonly AttendanceDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly INavigationService _navigationService;

        public UsersManagement(AttendanceDbContext context, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            InitializeComponent();
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            LoadData();
        }

        private async void LoadData()
        {
            // Clear existing data
            usersDataGrid.ItemsSource = null;

            // Load roles
            roleComboBox.ItemsSource = await _roleManager.Roles.ToListAsync();

            // Load existing users
            var users = await _context.Users
                .Include(u => u.Member)
                .AsNoTracking()
                .ToListAsync();

            //var userViewModels = new List<UserViewModel>();
            var userViewModels = new ObservableCollection<UserViewModel>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var allRoles = await _roleManager.Roles.ToListAsync();

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

        private async void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = searchTextBox.Text.ToLower();
            var members = await _context.Members
                .Where(m => !_context.Users.Any(u => u.MemberId == m.Id) &&
                           (m.FirstName.ToLower().Contains(searchText) ||
                            m.LastName.ToLower().Contains(searchText) ||
                            (m.OtherNames != null && m.OtherNames.ToLower().Contains(searchText)) ||
                            m.PhoneNumber.Contains(searchText) ||
                            m.Email.Contains(searchText)))
                .ToListAsync();

            membersListBox.ItemsSource = members;
        }

        private void MembersListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (membersListBox.SelectedItem is Member selectedMember)
            {
                userFormPanel.Visibility = Visibility.Visible;
                emailTextBox.Text = selectedMember.Email ?? string.Empty;
                emailTextBox.IsEnabled = string.IsNullOrEmpty(selectedMember.Email);
            }
        }

        private async void AddUser_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (membersListBox.SelectedItem is not Member selectedMember ||
                    roleComboBox.SelectedItem is not IdentityRole selectedRole)
                {
                    MessageBox.Show("Please select a member and role");
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
                System.Diagnostics.Debug.WriteLine($"Selected member ID: {selectedMember.Id}");
                System.Diagnostics.Debug.WriteLine($"Selected role: {selectedRole.Name}");

                var user = new User
                {
                    UserName = email,
                    Email = email,
                    MemberId = selectedMember.Id
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
                        LoadData();
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

        private async void SaveRole_Click(object sender, RoutedEventArgs e)
        {
            if (((FrameworkElement)sender).DataContext is UserViewModel viewModel &&
                viewModel.SelectedRole != null)
            {
                var currentRoles = await _userManager.GetRolesAsync(viewModel.User);
                await _userManager.RemoveFromRolesAsync(viewModel.User, currentRoles);
                await _userManager.AddToRoleAsync(viewModel.User, viewModel.SelectedRole.Name);
                MessageBox.Show("Role updated successfully");
            }
        }

        private async void DeleteUser_Click(object sender, RoutedEventArgs e)
        {
            if (((FrameworkElement)sender).DataContext is UserViewModel viewModel)
            {
                if (MessageBox.Show("Delete this user?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    await _userManager.DeleteAsync(viewModel.User);
                    LoadData();
                    MessageBox.Show("User deleted successfully");
                }
            }
        }

        private void ClearForm()
        {
            membersListBox.SelectedItem = null;
            userFormPanel.Visibility = Visibility.Collapsed;
            passwordBox.Clear();
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
    }
}