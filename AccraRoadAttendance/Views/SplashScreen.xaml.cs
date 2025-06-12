using AccraRoadAttendance.Services;
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
            if (IsInternetAvailable())
            {
                try
                {
                    _syncService.SyncData();
                    // Optionally update UI to show sync success
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Sync failed: {ex.Message}", "Sync Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    
                }
            }
            else
            {
                MessageBox.Show("No internet connection. Data may not be up to date.", "Offline Mode", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            var login = _serviceProvider.GetRequiredService<Login>();
            login.Show();
            Close();
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
