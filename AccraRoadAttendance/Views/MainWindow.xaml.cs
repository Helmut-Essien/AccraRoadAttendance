using AccraRoadAttendance.Data;
using AccraRoadAttendance.Models;
using AccraRoadAttendance.Services;
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
            _navigationService.NavigateTo<Members>();
        }

        private void Navigate(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var viewName = button?.CommandParameter as string;

            switch (viewName)
            {
                case "Members":
                    _navigationService.NavigateTo<Members>();
                    break;
                case "AddMembers":
                    _navigationService.NavigateTo<AddMembers>();
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