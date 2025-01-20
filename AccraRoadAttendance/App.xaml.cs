using System.Configuration;
using System.Windows;
using AccraRoadAttendance.Data;
using AccraRoadAttendance.Models;
using AccraRoadAttendance.Views;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SplashScreen = AccraRoadAttendance.Views.SplashScreen;

namespace AccraRoadAttendance
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IHost _host;

        public App()
        {
            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    var connectionString = "Data Source=attendance.db"; // Or from config files
                    services.AddDbContext<AttendanceContext>(options =>
                        options.UseSqlite(connectionString));
                    // Here's where you use AddIdentityCore
                    services.AddIdentityCore<Member>()
                        .AddEntityFrameworkStores<AttendanceContext>();

                    // Register MainWindow
                    services.AddTransient<MainWindow>();
                    services.AddTransient<SplashScreen>();
                    services.AddTransient<Login>();
                })
                .Build();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Use dependency injection to get the context
            var serviceProvider = _host.Services;
            try
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<AttendanceContext>();
                    context.Database.EnsureCreated();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database creation failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }

            // Show the SplashScreen
            var splashScreen = serviceProvider.GetRequiredService<MainWindow>();
            splashScreen.Show();

            // Here you might want to simulate some work or wait for a few seconds
            // This is a placeholder - replace with actual initialization logic or loading
            //System.Threading.Tasks.Task.Delay(2000).ContinueWith((t) =>
            //{
            //    Application.Current.Dispatcher.Invoke(() =>
            //    {
            //        // Close the splash screen
            //        splashScreen.Close();

            //        // Show the main window
            //        var mainWindow = serviceProvider.GetRequiredService<MainWindow>();
            //        mainWindow.Show();
            //    });
            //});
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _host.Dispose();
            base.OnExit(e);
        }
    }
}