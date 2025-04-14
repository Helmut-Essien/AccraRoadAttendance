using AccraRoadAttendance.Data;
using AccraRoadAttendance.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
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
            // Load roles
            roleComboBox.ItemsSource = await _roleManager.Roles.ToListAsync();

            // Load existing users
            var users = await _context.Users
                .Include(u => u.Member)
                .ToListAsync();

            var userViewModels = new List<UserViewModel>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userViewModels.Add(new UserViewModel
                {
                    User = user,
                    Roles = _roleManager.Roles.ToList(),
                    SelectedRole = roles.FirstOrDefault()
                });
            }

            usersDataGrid.ItemsSource = userViewModels;
        }

        private async void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = searchTextBox.Text.ToLower();
            var members = await _context.Members
                .Where(m => !_context.Users.Any(u => u.MemberId == m.Id) &&
                           (m.FullName.ToLower().Contains(searchText) ||
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

            var user = new User
            {
                UserName = email,
                Email = email,
                MemberId = selectedMember.Id
            };

            var result = await _userManager.CreateAsync(user, passwordBox.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, selectedRole.Name);
                MessageBox.Show("User created successfully");
                ClearForm();
                LoadData();
            }
            else
            {
                MessageBox.Show($"Error creating user: {string.Join(", ", result.Errors)}");
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

        public class UserViewModel
        {
            public User User { get; set; }
            public List<IdentityRole> Roles { get; set; }
            public IdentityRole SelectedRole { get; set; }
        }
    }
}