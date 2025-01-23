using AccraRoadAttendance.Data;
using AccraRoadAttendance.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AccraRoadAttendance.Views.Pages.Members
{
    public partial class Members : UserControl
    {
        private MainWindow _mainWindow;
        private readonly AttendanceDbContext _context;
        private List<Models.Member> allMembers; // All members in the system
        private List<Models.Member> displayedMembers; // Members displayed on the current page
        private int currentPage = 1;
        private AttendanceDbContext context;
        private const int pageSize = 15;

        public Members(AttendanceDbContext context, MainWindow mainWindow)
        {
            InitializeComponent();
            _context = context;
            _mainWindow = mainWindow;
            LoadMembers();
        }

        public Members(AttendanceDbContext context)
        {
            this.context = context;
        }

        private async void LoadMembers()
        {
            try
            {
                // Load members from the database asynchronously
                allMembers = await _context.Members.ToListAsync();
                RefreshDataGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while loading members: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshDataGrid()
        {
            displayedMembers = allMembers
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            membersDataGrid.ItemsSource = displayedMembers;
        }

        private void AddMember_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Add Member clicked!");

            // Navigate to the AddMembers user control
            using (var scope = _mainWindow._serviceProvider.CreateScope()) // Get the service provider from MainWindow
            {
                var context = scope.ServiceProvider.GetRequiredService<AttendanceDbContext>(); // Get DbContext
                var addMembersView = new AddMembers(_context); // Pass DbContext to AddMembers
                _mainWindow.MainContent.Content = addMembersView; // Set the content in MainWindow
            }

            //// Get the main window and ensure it's our MainWindow
            //var mainWindow = Application.Current.MainWindow as MainWindow;
            //if (mainWindow != null)
            //{
            //    // Get the service provider from the application
            //    var app = Application.Current as App;
            //    if (app != null && app._host != null)
            //    {
            //        using (var scope = app._host.Services.CreateScope())
            //        {
            //            // Retrieve AddMembers UserControl from the service container
            //            var addMembersView = scope.ServiceProvider.GetRequiredService<AddMembers>();
            //            mainWindow.MainContent.Content = addMembersView;
            //        }
            //    }
            //    else
            //    {
            //        MessageBox.Show("Could not access the service container.");
            //    }
            //}
            //else
            //{
            //    MessageBox.Show("Could not find the MainWindow to update the content.");
            //}
        }

        private void EditMember_Click(object sender, RoutedEventArgs e)
        {
            var member = (sender as Button)?.Tag as Member;
            MessageBox.Show($"Edit Member: ");
            // Logic for editing the member
        }

        private void DeleteMember_Click(object sender, RoutedEventArgs e)
        {
            //var member = (sender as Button)?.Tag as Member;
            //if (MessageBox.Show($"Delete Member: {member?.Name}?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            //{
            //    allMembers.Remove(member);
            //    RefreshDataGrid();
            //}
        }

        private void SearchMembers_TextChanged(object sender, TextChangedEventArgs e)
        {
            //var query = (sender as TextBox)?.Text.ToLower();
            //if (!string.IsNullOrEmpty(query))
            //{
            //    allMembers = allMembers.Where(m => m.Name.ToLower().Contains(query)).ToList();
            //}
            //else
            //{
            //    LoadMembers();
            //}
            //RefreshDataGrid();
        }

        private void PreviousPage_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage > 1)
            {
                currentPage--;
                RefreshDataGrid();
            }
        }

        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage * pageSize < allMembers.Count)
            {
                currentPage++;
                RefreshDataGrid();
            }
        }

        private void NavigateToAddMembers(object sender, RoutedEventArgs e)
        {
            _mainWindow.NavigateToAddMembers();
        }
    }

    
}
