using AccraRoadAttendance.Data;
using AccraRoadAttendance.Models;
using AccraRoadAttendance.Services;
using AccraRoadAttendance.Views.Pages.Attendance;
using AccraRoadAttendance.Views.Pages.Dashboard;
using AccraRoadAttendance.Views.Pages.Members;
using AccraRoadAttendance.Views.Pages.Reports;
using AccraRoadAttendance.Views.Pages.Users;
using DocumentFormat.OpenXml.Bibliography;
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
        private readonly INavigationService _navigationService;

        public MainWindow(INavigationService navigationService)
        {
            InitializeComponent();
            _navigationService = navigationService;
            ((NavigationService)_navigationService).SetContentFrame(MainContent);
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _navigationService.NavigateTo<Dashboard>();
            //var login = new Login();
            //login.Show();
        }

        private void Navigate(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var viewName = button?.CommandParameter as string;

            switch (viewName)
            {
                case "Dashboard":
                    _navigationService.NavigateTo<Dashboard>();
                    break;
                case "Members":
                    _navigationService.NavigateTo<Members>();
                    break;
                case "MarkAttendance":
                    _navigationService.NavigateTo<MarkAttendance>();
                    break;
                case "Reports":
                    _navigationService.NavigateTo<ReportsPage>();
                    break;
                case "Users":
                    _navigationService.NavigateTo<UsersManagement>();
                    break;

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