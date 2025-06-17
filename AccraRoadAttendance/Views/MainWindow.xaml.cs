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

using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
        private readonly SyncService _syncService;

        public MainWindow(INavigationService navigationService, CurrentUserService currentUserService, IServiceProvider serviceProvider, SyncService syncService)
        {
            InitializeComponent();
            _navigationService = navigationService;
            _currentUserService = currentUserService;
            _serviceProvider = serviceProvider;
            _syncService = syncService;
            ((NavigationService)_navigationService).SetContentFrame(MainContent);
            DataContext = this;
            Loaded += OnLoaded;
            //_googleDriveService = googleDriveService;
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

            //_navigationService.NavigateTo<Dashboard>();
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

        private void TestGoogleDrive(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Starting Google Drive test...");
            try
            {
                //    // Open file picker to select multiple images
                //    var openFileDialog = new OpenFileDialog
                //    {
                //        Filter = "Image files (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png",
                //        InitialDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ProfilePictures"),
                //        Multiselect = true
                //    };
                //    Console.WriteLine("File picker opened.");

                //    if (openFileDialog.ShowDialog() != true)
                //    {
                //        Console.WriteLine("No images selected by user.");
                //        MessageBox.Show("No images selected.", "Test Cancelled", MessageBoxButton.OK, MessageBoxImage.Information);
                //        return;
                //    }
                //    Console.WriteLine($"Selected {openFileDialog.FileNames.Length} file(s) for upload.");

                //    // Ensure the local ProfilePictures folder exists
                //    Directory.CreateDirectory("ProfilePictures");
                //    Console.WriteLine("ProfilePictures directory created or already exists.");

                //    // Process each selected file
                //    foreach (string filePath in openFileDialog.FileNames)
                //    {
                //        Console.WriteLine($"Processing file: {filePath}");
                //        try
                //        {
                //            // Test UploadImage
                //            string driveUrl = _googleDriveService.UploadImage(filePath);
                //            Console.WriteLine($"Image '{Path.GetFileName(filePath)}' uploaded successfully. URL: {driveUrl}");
                //            MessageBox.Show($"Image '{Path.GetFileName(filePath)}' uploaded to Google Drive. URL: {driveUrl}",
                //                "Upload Success", MessageBoxButton.OK, MessageBoxImage.Information);

                //            // Optionally test download (uncomment to enable)

                //            Console.WriteLine($"Testing download for URL: {driveUrl}");
                //            string downloadedPath = _googleDriveService.DownloadImage(driveUrl);
                //            Console.WriteLine($"Image downloaded successfully to: {downloadedPath}");
                //            MessageBox.Show($"Image downloaded to: {downloadedPath}", "Download Success", MessageBoxButton.OK, MessageBoxImage.Information);

                //        }
                //        catch (Exception ex)
                //        {
                //            Console.WriteLine($"Failed to process file '{Path.GetFileName(filePath)}': {ex.Message}");
                //            MessageBox.Show($"Failed to upload image '{Path.GetFileName(filePath)}': {ex.Message}",
                //                "Upload Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                //        }
                //    }
                //    Console.WriteLine("Google Drive test completed.");

                _syncService.SyncData();
            }
            catch (Exception ex)
            {

                //Console.WriteLine(ex.Message);
                //MessageBox.Show(ex.Message, "Test Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                // Build a detailed error message including inner exceptions
                string errorMessage = ex.Message;
                Exception currentException = ex;
                int depth = 1;

                // Traverse all inner exceptions
                while (currentException.InnerException != null)
                {
                    currentException = currentException.InnerException;
                    errorMessage += $"\nInner Exception {depth}: {currentException.Message}";
                    depth++;
                }

                // Optionally include the stack trace for full context
                errorMessage += $"\nStack Trace: {ex.StackTrace}";

                // Log to console
                Console.WriteLine("Exception Details:");
                Console.WriteLine(errorMessage);

                // Show in MessageBox
                MessageBox.Show(errorMessage, "Test Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        //protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        //{
        //    base.OnMouseLeftButtonDown(e);
        //    DragMove();
        //}
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                // Toggle between Normal and Maximized
                this.WindowState = this.WindowState == WindowState.Maximized
                    ? WindowState.Normal
                    : WindowState.Maximized;
                e.Handled = true;
            }
            else
            {
                this.DragMove();
            }
        }

        private void TitleText_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }


    }
}