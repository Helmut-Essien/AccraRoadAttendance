using AccraRoadAttendance.Services;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AccraRoadAttendance.Views
{
    /// <summary>
    /// Interaction logic for SplashScreen.xaml
    /// </summary>
    public partial class SplashScreen : Window
    {
        private readonly SyncService _syncService;
        private readonly IServiceProvider _serviceProvider;

        public SplashScreen(SyncService syncService, IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _syncService = syncService;
            _serviceProvider = serviceProvider;
            Loaded += OnLoaded;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Trigger fade-in animation
            var fadeIn = (Storyboard)this.Resources["FadeInStoryboard"];
            fadeIn.Begin(this);

            // Small delay to ensure the animation starts smoothly
            await Task.Delay(50);

            // Create a progress reporter to update the UI
            var progress = new Progress<string>(message => StatusText.Text = message);

            // Initial status
            StatusText.Text = "Checking internet connection...";
            await Task.Delay(500); // Short delay to let the user read the message

            if (IsInternetAvailable())
            {
                StatusText.Text = "Internet connected. Synchronizing data...";
                try
                {
                    // Run sync on a background thread with progress reporting
                    await Task.Run(() => _syncService.SyncData(progress));
                    //_syncService.SyncData();
                    // Optionally update UI to show sync success
                    //StatusText.Text = "Synchronization complete.";
                }
                catch (Exception ex)
                {
                    //StatusText.Text = "Sync failed.";
                    ////MessageBox.Show($"Sync failed: {ex.Message}", "Sync Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    MessageBox.Show(ex.ToString(), "Sync Error", MessageBoxButton.OK, MessageBoxImage.Error);

                }
            }
            else
            {
                StatusText.Text = "No internet! Skipping sync.";
                MessageBox.Show("No internet connection. Data may not be up to date.", "Offline Mode", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            // Wait to show the final message, then transition
            await Task.Delay(1000);
            await ShowLoginAndCloseAsync();

            // Trigger fade-out and close
            //await FadeOutAndCloseAsync();


            //var login = _serviceProvider.GetRequiredService<Login>();
            //Application.Current.MainWindow = login;
            //login.Show();
            ////Close();
            //// 3) Now it’s safe to let WPF auto‐shutdown when last window closes
            //Application.Current.ShutdownMode = ShutdownMode.OnLastWindowClose;

        }

        //public Task FadeOutAndCloseAsync()
        //{
        //    var tcs = new TaskCompletionSource<object>();

        //    Dispatcher.Invoke(() =>
        //    {
        //        var fadeOut = (Storyboard)this.Resources["FadeOutStoryboard"];
        //        fadeOut.Completed += (s, e) =>
        //        {
        //            this.Close();
        //            tcs.SetResult(null);
        //        };
        //        fadeOut.Begin(this);
        //    });

        //    return tcs.Task;
        //}
        public async Task FadeOutAsync()
        {
            var tcs = new TaskCompletionSource<object>();
            Dispatcher.Invoke(() =>
            {
                var fadeOut = (Storyboard)this.Resources["FadeOutStoryboard"];
                fadeOut.Completed += (s, e) => tcs.SetResult(null);
                fadeOut.Begin(this);
            });
            await tcs.Task;
        }

        private async Task ShowLoginAndCloseAsync()
        {
            // Create login window first
            var login = _serviceProvider.GetRequiredService<Login>();

            // Fade out splash (without closing yet)
            await FadeOutAsync();

            // Show login BEFORE closing splash
            login.Show();
            login.Activate();
            Application.Current.MainWindow = login;

            // Now close splash
            this.Close();
        }
        private bool IsInternetAvailable()
        {
            try
            {
                using (var ping = new Ping())
                {
                    var reply = ping.Send("www.google.com", 1000);
                    return reply.Status == IPStatus.Success;
                }
            }
            catch
            {
                return false;
            }
        }

        
    }
}
