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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
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
                    //          services.AddDbContext<AttendanceDbContext>(options =>
                    //              //options.UseSqlServer("Server=FINSERVE\\SQLEXPRESS;Database=AttendanceDb;Integrated Security=True;Trusted_Connection=True;TrustServerCertificate=True;"));

                    //                                  options.UseSqlServer(@"Data Source=(LocalDB)\MSSQLLocalDB;
                    //AttachDbFilename=|DataDirectory|\AttendanceDb.mdf;
                    //Integrated Security=True;
                    //Connect Timeout=30;
                    //Initial Catalog=AttendanceDb"));

                    services.AddDbContext<AttendanceDbContext>(options =>
                    {
                        // Build a path to LocalAppData (user-specific writable directory)
                        string dbFolder = Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                            "AccraRoadAttendance"
                        );

                        string dbPath = Path.Combine(dbFolder, "AttendanceDb.mdf");

                        options.UseSqlServer(
                            $@"Data Source=(LocalDB)\MSSQLLocalDB;
                            Initial Catalog=AttendanceDb;
                            AttachDbFilename={dbPath};
                            Integrated Security=True;
                            Connect Timeout=30;
                            MultipleActiveResultSets=True");
                    });


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

            await InitializeDatabaseAsync();

            //var loginWindow = _host.Services.GetRequiredService<Login>();
            //loginWindow.Show();

            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        private async Task InitializeDatabaseAsync()
        {
            // Ensure database directory exists
            string dbFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "AccraRoadAttendance"
            );

            if (!Directory.Exists(dbFolder))
            {
                Directory.CreateDirectory(dbFolder);
            }

            // Clean up previous database files
            string[] dbFiles = {
        Path.Combine(dbFolder, "AttendanceDb.mdf"),
        Path.Combine(dbFolder, "AttendanceDb_log.ldf")
    };

            foreach (var file in dbFiles)
            {
                if (File.Exists(file))
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch (IOException)
                    {
                        // If file is locked, wait and retry
                        await Task.Delay(500);
                        File.Delete(file);
                    }
                }
            }

            // Ensure database is created and migrations are applied
            using var scope = _host.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AttendanceDbContext>();
            // Ensure clean connection state
            if (context.Database.GetDbConnection().State == System.Data.ConnectionState.Open)
            {
                await context.Database.CloseConnectionAsync();
            }

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