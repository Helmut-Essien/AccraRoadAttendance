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
using Microsoft.Extensions.DependencyInjection;
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
        private readonly CurrentUserService _currentUserService;
        private readonly IServiceProvider _serviceProvider;

        public MainWindow(INavigationService navigationService, CurrentUserService currentUserService, IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _navigationService = navigationService;
            _currentUserService = currentUserService;
            _serviceProvider = serviceProvider;
            ((NavigationService)_navigationService).SetContentFrame(MainContent);
            DataContext = this;
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!_currentUserService.IsLoggedIn)
            {
                var login = _serviceProvider.GetRequiredService<Login>();
                login.Show();
                Close();
            }
            else
            {
                _navigationService.NavigateTo<Dashboard>();
            }
        }

        private async void Navigate(object sender, RoutedEventArgs e)
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
                    
                    if (await _currentUserService.IsInRoleAsync("Admin"))
                        _navigationService.NavigateTo<UsersManagement>();
                    else
                        MessageBox.Show("Access Denied: You do not have permission to manage users.");
                    break;

            }
        }

        private void Logout(object sender, RoutedEventArgs e)
        {
            _currentUserService.Logout();
            var login = _serviceProvider.GetRequiredService<Login>();
            login.Show();
            Close();
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