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
using Microsoft.EntityFrameworkCore;
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
                // Get connection string from configuration
                var connectionString = context.Configuration.GetConnectionString("DefaultConnection");

                services.AddDbContext<AttendanceDbContext>(options =>
                    options.UseSqlServer(connectionString));


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

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Temporary debug code
            var env = _host.Services.GetRequiredService<IHostEnvironment>();
            MessageBox.Show($"Current environment: {env.EnvironmentName}");

            var config = _host.Services.GetRequiredService<IConfiguration>();
            MessageBox.Show($"Connection string: {config.GetConnectionString("DefaultConnection")}");

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
            await context.Database.MigrateAsync();

            // Seed roles
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            await CreateRoleIfNotExists(roleManager, "Admin");
            await CreateRoleIfNotExists(roleManager, "User");
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