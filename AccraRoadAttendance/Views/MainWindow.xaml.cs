using AccraRoadAttendance.Data;
using AccraRoadAttendance.Models;
using AccraRoadAttendance.Views.Pages.Members;
using Microsoft.EntityFrameworkCore;
using System;
using System.Windows;
using System.Windows.Controls;

namespace AccraRoadAttendance.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly AttendanceDbContext _context;

        public MainWindow(AttendanceDbContext context)
        {
            InitializeComponent();
            _context = context;
        }

        // Method to navigate to Members
        public void NavigateToMembers()
        {
            var membersView = new Members(_context, this);
            MainContent.Content = membersView;
        }

        // Method to navigate to AddMembers
        public void NavigateToAddMembers()
        {
            var addMembersView = new AddMembers(_context);
            MainContent.Content = addMembersView;
        }

        // Method to navigate to EditMembers
        public void NavigateToEditMembers(Member member)
        {
            if (member == null)
            {
                MessageBox.Show("Member is null before navigation.");
                return;
            }
            var editMemberView = new EditMembers(_context, this, member);
            MainContent.Content = editMemberView;
        }

        private void Navigate(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var viewName = button?.CommandParameter as string;

            if (viewName != null)
            {
                switch (viewName)
                {
                    case "Members":
                        NavigateToMembers();
                        break;
                        // Add other cases for other views here
                }
            }
        }

        private void ExitApp(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void ToggleTheme(object sender, RoutedEventArgs e)
        {
            // Logic to toggle light/dark theme
        }
    }
}