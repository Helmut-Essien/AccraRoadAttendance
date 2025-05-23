using System.Configuration;
using System.IO;
using System.Windows;
using System.Windows.Navigation;
using AccraRoadAttendance.Data;
using AccraRoadAttendance.Models;
using AccraRoadAttendance.Services;
using AccraRoadAttendance.Views;
using AccraRoadAttendance.Views.Pages.Attendance;
using AccraRoadAttendance.Views.Pages.Dashboard;
using AccraRoadAttendance.Views.Pages.Members;
using AccraRoadAttendance.Views.Pages.Reports;
using AccraRoadAttendance.Views.Pages.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
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
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            try
            {

                _host = Host.CreateDefaultBuilder()
       .ConfigureAppConfiguration((context, config) =>
       {
           config.SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json"/*, optional: true*/)
               .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true)
               .AddEnvironmentVariables();
       })
           .ConfigureServices((context, services) =>
            {
                // Local DB Context
                // Get connection string from configuration
                var connectionString = context.Configuration.GetConnectionString("DefaultConnection");
                services.AddDbContext<AttendanceDbContext>(options =>
                    options.UseSqlServer(connectionString));

                //// Online DB Context
                //var connectionString1 = context.Configuration.GetConnectionString("OnlineConnection");
                //services.AddDbContext<AttendanceDbContext>(options =>
                //    options.UseSqlServer(connectionString1), ServiceLifetime.Transient);

                // Online DB Context (for syncing only)
                var onlineConnection = context.Configuration.GetConnectionString("OnlineConnection");
                services.AddDbContext<OnlineAttendanceDbContext>(options =>
                    options.UseSqlServer(onlineConnection));


                // Configure Identity
                //services.AddIdentityCore<User>()
                //    .AddEntityFrameworkStores<AttendanceDbContext>();

                // Configure Identity
                services.AddIdentityCore<User>(options =>
                    {
                        options.User.RequireUniqueEmail = true;
                    })
                    .AddRoles<IdentityRole>()
                    .AddEntityFrameworkStores<AttendanceDbContext>();
                    

                    // Add role management services
                    services.AddScoped<RoleManager<IdentityRole>>();
                    services.AddScoped<UserManager<User>>();
                    services.AddScoped<CurrentUserService>();
                    services.AddScoped<GoogleDriveService>();
                    services.AddScoped<SyncService>();




                // Register windows and pages with correct lifetimes
                services.AddTransient<MainWindow>();
                    services.AddTransient<SplashScreen>();
                    services.AddTransient<Login>();
                    services.AddTransient<AddMembers>();
                    services.AddTransient<EditMembers>();
                    services.AddTransient<Members>();
                    services.AddTransient<MarkAttendance>();
                    services.AddTransient<ReportsPage>();
                    services.AddTransient<MemberDetails>();
                    services.AddTransient<Dashboard>();
                    services.AddTransient<UsersManagement>();
                    

                    // Add navigation service
                    services.AddSingleton<INavigationService, Services.NavigationService>();
                })
                .Build();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error building host: {ex.Message}", "Configuration Error", MessageBoxButton.OK, MessageBoxImage.Error);
            throw;
        }
}

private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show($"An unhandled exception occurred: {e.Exception.Message}\n{e.Exception.StackTrace}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Temporary debug code
            var env = _host.Services.GetRequiredService<IHostEnvironment>();
            MessageBox.Show($"Current environment: {env.EnvironmentName}");

            var config = _host.Services.GetRequiredService<IConfiguration>();
            MessageBox.Show($"Connection string: {config.GetConnectionString("DefaultConnection")}");
            MessageBox.Show($"Online connection string: {config.GetConnectionString("OnlineConnection")}");

            await InitializeDatabaseAsync();

            // Create a DI scope for all scoped services
            var scope = _host.Services.CreateScope();
            var services = scope.ServiceProvider;

            //var loginWindow = _host.Services.GetRequiredService<Login>();
            //loginWindow.Show();

            // Show MainWindow
            var mainWindow = services.GetRequiredService<MainWindow>();
            mainWindow.Closed += (s, args) => scope.Dispose();
            mainWindow.Show();
        //    var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        //    mainWindow.Show();
        }


        private async Task InitializeDatabaseAsync()
        {
            using var scope = _host.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AttendanceDbContext>();
            try
            {

                // If it doesn't exist, create it and apply migrations
                await context.Database.MigrateAsync();


                // Seed roles
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                await CreateRoleIfNotExists(roleManager, "Admin");
                await CreateRoleIfNotExists(roleManager, "User");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database initialization failed: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

        private async Task CreateRoleIfNotExists(RoleManager<IdentityRole> roleManager, string roleName)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _host.Dispose();
            base.OnExit(e);
        }
    }
}