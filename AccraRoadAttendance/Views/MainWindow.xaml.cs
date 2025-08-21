using AccraRoadAttendance.Data;
using AccraRoadAttendance.Models;
using AccraRoadAttendance.Services;
using AccraRoadAttendance.Views.Pages.Attendance;
using AccraRoadAttendance.Views.Pages.Dashboard;
using AccraRoadAttendance.Views.Pages.Members;
using AccraRoadAttendance.Views.Pages.Reports;
using AccraRoadAttendance.Views.Pages.Users;
using DocumentFormat.OpenXml.Bibliography;
using MaterialDesignThemes.Wpf;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AccraRoadAttendance.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private readonly INavigationService _navigationService;
        private readonly CurrentUserService _currentUserService;
        private readonly IServiceProvider _serviceProvider;
        private readonly SyncService _syncService;
        public event PropertyChangedEventHandler? PropertyChanged;

        private string _syncStatusMessage = "Initializing...";
        public string SyncStatusMessage
        {
            get => _syncStatusMessage;
            set
            {
                _syncStatusMessage = value;
                OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

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
                    
                    if (_currentUserService.IsInRole("Admin"))
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

        private async void TestGoogleDrive(object sender, RoutedEventArgs e)
        {


            //var dialogHost = this.FindName("SyncDialogHost") as DialogHost;
            //if (dialogHost == null) return;

            //// Store the dialog content separately
            //var dialogContent = (FrameworkElement)dialogHost.DialogContent;

            ////IProgress<string> progress = new Progress<string>(msg =>
            ////{
            ////    //var dialogContent = dialogHost.DialogContent as FrameworkElement;
            ////    var txt = dialogContent?.FindName("SyncStatusText") as TextBlock;
            ////    txt?.Dispatcher.Invoke(() => txt.Text = msg);
            ////});
            //IProgress<string> progress = new Progress<string>(msg =>
            //{
            //    var txt = dialogContent.FindName("SyncStatusText") as TextBlock;
            //    if (txt != null) txt.Text = msg; // No Dispatcher.Invoke needed
            //});

            //// Explicitly specify the DialogOpenedEventHandler type
            //await DialogHost.Show(
            //    dialogContent,
            //    dialogHost.Identifier,
            //    new DialogOpenedEventHandler(async (sender, args) =>
            //    {
            //        try
            //        {
            //            progress.Report("Starting synchronization…");
            //            await Task.Delay(1000);
            //            await Task.Run(() => _syncService.SyncData(progress));
            //            progress.Report("Synchronization complete.");
            //            await Task.Delay(500);
            //        }
            //        catch (Exception ex)
            //        {
            //            MessageBox.Show($"Sync failed: {ex.Message}", "Sync Error",
            //                            MessageBoxButton.OK, MessageBoxImage.Error);
            //        }
            //        finally
            //        {
            //            args.Session.Close();  // Close the dialog
            //        }
            //    })
            //);
            var dialogHost = this.FindName("SyncDialogHost") as DialogHost;
            if (dialogHost == null) return;

            //var dialogContent = (FrameworkElement)dialogHost.DialogContent;
            // Set DataContext for binding (if not already set)
            if (SyncDialogHost.DialogContent is FrameworkElement dialogContent)
            {
                dialogContent.DataContext = this;
            }

            IProgress<string> progress = new Progress<string>(msg =>
            {
                //var txt = dialogContent?.FindName("SyncStatusText") as TextBlock;
                //txt?.Dispatcher.Invoke(() => txt.Text = msg);
                // Update the bound property
                SyncStatusMessage = msg;
            });

            await DialogHost.Show(
                dialogHost.DialogContent,
                dialogHost.Identifier,
                new DialogOpenedEventHandler(async (sender, args) =>
                {
                    try
                    {
                        progress.Report("Starting synchronization…");
                        await Task.Delay(1000);
                        await Task.Run(() => _syncService.SyncDataAsync(progress));
                        //progress.Report("Synchronization complete.");
                        await Task.Delay(1000);
                        
                    }
                    catch (Exception ex)
                    {
                        progress.Report("Synchronization failed.");
                        await Task.Delay(3000);
                    }
                    finally
                    {
                        args.Session.Close();
                        _navigationService.NavigateTo<Dashboard>();
                    }
                })
            );

        }

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