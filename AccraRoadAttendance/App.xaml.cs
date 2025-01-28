using System.Configuration;
using System.Windows;
using System.Windows.Navigation;
using AccraRoadAttendance.Data;
using AccraRoadAttendance.Models;
using AccraRoadAttendance.Services;
using AccraRoadAttendance.Views;
using AccraRoadAttendance.Views.Pages.Members;
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
        private readonly IHost _host;

        public App()
        {
            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    // Configure DbContext
                    services.AddDbContext<AttendanceDbContext>(options =>
                        options.UseSqlite("Data Source=attendance.db"));


                    // Configure Identity
                    services.AddIdentityCore<Member>()
                        .AddEntityFrameworkStores<AttendanceDbContext>();

                    // Register windows and pages with correct lifetimes
                    services.AddSingleton<MainWindow>();
                    services.AddTransient<SplashScreen>();
                    services.AddTransient<Login>();
                    services.AddTransient<AddMembers>();
                    services.AddTransient<EditMembers>();
                    services.AddTransient<Members>();

                    // Add navigation service
                    services.AddSingleton<INavigationService, Services.NavigationService>();
                })
                .Build();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            await InitializeDatabaseAsync();

            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        private async Task InitializeDatabaseAsync()
        {
            using var scope = _host.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AttendanceDbContext>();
            await context.Database.MigrateAsync();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _host.Dispose();
            base.OnExit(e);
        }
    }
}