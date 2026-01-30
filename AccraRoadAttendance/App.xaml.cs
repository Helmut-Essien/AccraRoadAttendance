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
using Microsoft.Extensions.Logging;
using System.Configuration;
using System.IO;
using System.Windows;
using System.Windows.Navigation;
using System.Windows.Threading;
using SplashScreen = AccraRoadAttendance.Views.SplashScreen;

namespace AccraRoadAttendance
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private LogoSplashWindow _logoSplash;


        private IHost _host;

        public App()
        {
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            // 1) Show splash as early as possible
            _logoSplash = new Views.LogoSplashWindow();
            _logoSplash.Show();
        }
        //    try
        //    {


        //}
        //catch (Exception ex)
        //{
        //    MessageBox.Show($"Error building host: {ex.Message}", "Configuration Error", MessageBoxButton.OK, MessageBoxImage.Error);
        //    throw;
        //}

        private void BuildHost()
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
                    //services.AddScoped<GoogleDriveService>();
                    //services.AddSingleton<GoogleDriveService>(serviceProvider =>
                    //{
                    //    var logger = serviceProvider.GetRequiredService<ILogger<GoogleDriveService>>();
                    //    return GoogleDriveService.CreateAsync(logger).GetAwaiter().GetResult();
                    //});
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


        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            // Fade out LogoSplash
            if (_logoSplash != null)
            {
                _logoSplash.FadeOutAndCloseAsync();
            }
            //MessageBox.Show($"An unhandled exception occurred: {e.Exception.Message}\n{e.Exception.StackTrace}",
            //    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            MessageBox.Show($"An unhandled exception occurred: {e.Exception.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            this.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            // 2) Let WPF render the splash
            await Dispatcher.Yield(DispatcherPriority.Background);

            try
            {
                // 3) Run heavy startup work in background
                await Task.Run(async () =>
                {
                    BuildHost();
                    await InitializeDatabaseAsync();
                });

                // Temporary debug code
                var env = _host.Services.GetRequiredService<IHostEnvironment>();
                //MessageBox.Show($"Current environment: {env.EnvironmentName}");

                var config = _host.Services.GetRequiredService<IConfiguration>();
                //MessageBox.Show($"Connection string: {config.GetConnectionString("DefaultConnection")}");
                //MessageBox.Show($"Online connection string: {config.GetConnectionString("OnlineConnection")}");

                //await InitializeDatabaseAsync();

                // Create a DI scope for all scoped services
                var scope = _host.Services.CreateScope();
                var services = scope.ServiceProvider;

                //var loginWindow = _host.Services.GetRequiredService<Login>();
                //loginWindow.Show();


                var splashWindow = services.GetRequiredService<SplashScreen>();
                Application.Current.MainWindow = splashWindow;


                // Fade out LogoSplash
                if (_logoSplash != null)
                {
                    await _logoSplash.FadeOutAndCloseAsync();
                }

                splashWindow.Show();


                this.ShutdownMode = ShutdownMode.OnLastWindowClose;
                //// Show MainWindow
                //var mainWindow = services.GetRequiredService<MainWindow>();
                //mainWindow.Closed += (s, args) => scope.Dispose();
                //mainWindow.Show();

            }
            catch (Exception ex)
            {
                await FadeOutSplashAndShowErrorAsync(ex);
                Shutdown();
            }


            //    var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            //    mainWindow.Show();
        }


        //private async Task InitializeDatabaseAsync()
        //{
        //    using var scope = _host.Services.CreateScope();
        //    var context = scope.ServiceProvider.GetRequiredService<AttendanceDbContext>();
        //    try
        //    {

        //        // If it doesn't exist, create it and apply migrations
        //        await context.Database.MigrateAsync();


        //        // Seed roles
        //        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        //        await CreateRoleIfNotExists(roleManager, "Admin");
        //        await CreateRoleIfNotExists(roleManager, "User");
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"Database initialization failed: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
        //        throw;
        //    }
        //}
        private async Task FadeOutSplashAndShowErrorAsync(System.Exception ex)
        {
            if (_logoSplash != null)
                await _logoSplash.FadeOutAndCloseAsync();

            MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private async Task InitializeDatabaseAsync()
        {
            using var scope = _host.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AttendanceDbContext>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

            //// 1. Apply migrations
            //await context.Database.MigrateAsync();

            //// 2. Ensure roles exist
            //await CreateRoleIfNotExists(roleManager, "Admin");
            //await CreateRoleIfNotExists(roleManager, "User");

            //// 3. Seed Admin user
            //const string adminEmail = "admin@example.com";
            //const string adminPassword = "Admin@123";

            //if (await userManager.FindByEmailAsync(adminEmail) is null)
            //{
            //    var adminUser = new User
            //    {
            //        UserName = adminEmail,
            //        Email = adminEmail,
            //        EmailConfirmed = true,
            //        // Notice: MemberId left null so no Member record is created
            //    };

            //    var createResult = await userManager.CreateAsync(adminUser, adminPassword);
            //    if (createResult.Succeeded)
            //    {
            //        await userManager.AddToRoleAsync(adminUser, "Admin");
            //    }
            //    else
            //    {
            //        // Handle errors (e.g. log them)
            //        var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
            //        throw new Exception($"Failed to create admin user: {errors}");
            //    }
            try
            {
                // 1. Drop existing database
                //await context.Database.EnsureDeletedAsync();
                // Add 1s delay to allow file release
                System.Threading.Thread.Sleep(1000);
                // 1. Apply migrations
                await context.Database.MigrateAsync();

                // 2. Ensure roles exist
                await CreateRoleIfNotExists(roleManager, "Admin");
                await CreateRoleIfNotExists(roleManager, "User");

                // 3. Seed Admin user
                const string adminEmail = "admin@example.com";
                const string adminPassword = "Admin@123";

                if (await userManager.FindByEmailAsync(adminEmail) is null)
                {
                    var adminUser = new User
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        EmailConfirmed = true,
                        // Notice: MemberId left null so no Member record is created
                    };

                    var createResult = await userManager.CreateAsync(adminUser, adminPassword);
                    if (createResult.Succeeded)
                    {
                        await userManager.AddToRoleAsync(adminUser, "Admin");
                    }
                    else
                    {
                        var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
                        throw new Exception($"Failed to create admin user: {errors}");
                    }
                }
            }
            catch (Exception ex)
            {
                var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var connectionString = config.GetConnectionString("DefaultConnection");
                string dbInfo = "Database location: Unable to determine.";
                try
                {
                    var builder = new SqlConnectionStringBuilder(connectionString);
                    if (!string.IsNullOrEmpty(builder.AttachDBFilename))
                    {
                        dbInfo = $"Database file: {builder.AttachDBFilename}";
                    }
                    else
                    {
                        dbInfo = "Not a local database file.";
                    }
                }
                catch
                {
                    // If parsing fails, retain the default message
                    throw;  // don’t call MessageBox here
                }
                //MessageBox.Show($"Database initialization failed: {ex.Message}\n\n{dbInfo}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

        protected override async void OnExit(ExitEventArgs e)
        {
            if (_host != null)
            {
                await _host.StopAsync(TimeSpan.FromSeconds(5));
                _host.Dispose();
            }

            base.OnExit(e);
        }
    }
}







//using AccraRoadAttendance.Data;
//using AccraRoadAttendance.Models;
//using AccraRoadAttendance.Services;
//using AccraRoadAttendance.Views;
//using AccraRoadAttendance.Views.Pages.Attendance;
//using AccraRoadAttendance.Views.Pages.Dashboard;
//using AccraRoadAttendance.Views.Pages.Members;
//using AccraRoadAttendance.Views.Pages.Reports;
//using AccraRoadAttendance.Views.Pages.Users;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;
//using Microsoft.Extensions.Logging;
//using System;
//using System.IO;
//using System.Threading.Tasks;
//using System.Windows;
//using System.Windows.Threading;
//using SplashScreen = AccraRoadAttendance.Views.SplashScreen;

//namespace AccraRoadAttendance
//{
//    public partial class App : Application
//    {
//        private LogoSplashWindow? _logoSplash;
//        private IHost? _host;

//        public App()
//        {
//            DispatcherUnhandledException += App_DispatcherUnhandledException;
//            // Show logo splash immediately (non-blocking)
//            _logoSplash = new LogoSplashWindow();
//            _logoSplash.Show();
//        }

//        protected override async void OnStartup(StartupEventArgs e)
//        {
//            base.OnStartup(e);
//            ShutdownMode = ShutdownMode.OnExplicitShutdown; // We'll control shutdown

//            try
//            {
//                // Yield to let splash render
//                await Dispatcher.Yield(DispatcherPriority.Background);

//                // Build and start host (includes DB init)
//                await InitializeHostAndDatabaseAsync();

//                // Create scope for UI services
//                using var scope = _host!.Services.CreateScope();
//                var services = scope.ServiceProvider;

//                // Get and show the splash (progress screen) or login
//                var splashWindow = services.GetRequiredService<SplashScreen>();
//                Current.MainWindow = splashWindow;

//                // Fade out logo splash
//                if (_logoSplash != null)
//                {
//                    await _logoSplash.FadeOutAndCloseAsync();
//                    _logoSplash = null;
//                }

//                splashWindow.Show();

//                // Let WPF manage shutdown once main window is set
//                ShutdownMode = ShutdownMode.OnLastWindowClose;
//            }
//            catch (Exception ex)
//            {
//                await HandleStartupErrorAsync(ex);
//                Shutdown(-1); // Error exit code
//            }
//        }

//        private async Task InitializeHostAndDatabaseAsync()
//        {
//            _host = Host.CreateDefaultBuilder()
//                .ConfigureAppConfiguration((hostingContext, config) =>
//                {
//                    config.SetBasePath(Directory.GetCurrentDirectory())
//                          .AddJsonFile("appsettings.json", optional: false)
//                          .AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", optional: true)
//                          .AddEnvironmentVariables();
//                })
//                .ConfigureServices((context, services) =>
//                {
//                    var configuration = context.Configuration;

//                    // Local DB
//                    var localConn = configuration.GetConnectionString("DefaultConnection");
//                    services.AddDbContext<AttendanceDbContext>(options =>
//                        options.UseSqlServer(localConn));

//                    // Online DB (sync only)
//                    var onlineConn = configuration.GetConnectionString("OnlineConnection");
//                    services.AddDbContext<OnlineAttendanceDbContext>(options =>
//                        options.UseSqlServer(onlineConn));

//                    // Identity
//                    services.AddIdentityCore<User>(options =>
//                    {
//                        options.User.RequireUniqueEmail = true;
//                    })
//                    .AddRoles<IdentityRole>()
//                    .AddEntityFrameworkStores<AttendanceDbContext>();

//                    services.AddScoped<RoleManager<IdentityRole>>();
//                    services.AddScoped<UserManager<User>>();
//                    services.AddScoped<CurrentUserService>();
//                    //services.AddScoped<GoogleDriveService>();
//                    //services.AddSingleton<GoogleDriveService>(serviceProvider =>
//                    //{
//                    //    var logger = serviceProvider.GetRequiredService<ILogger<GoogleDriveService>>();
//                    //    return GoogleDriveService.CreateAsync(logger).GetAwaiter().GetResult();
//                    //});
//                    services.AddScoped<SyncService>();

//                    // Windows & Pages (Transient = new instance each resolve)
//                    services.AddTransient<MainWindow>();
//                    services.AddTransient<SplashScreen>();
//                    services.AddTransient<Login>();
//                    services.AddTransient<AddMembers>();
//                    services.AddTransient<EditMembers>();
//                    services.AddTransient<Members>();
//                    services.AddTransient<MarkAttendance>();
//                    services.AddTransient<ReportsPage>();
//                    services.AddTransient<MemberDetails>();
//                    services.AddTransient<Dashboard>();
//                    services.AddTransient<UsersManagement>();

//                    services.AddSingleton<INavigationService, NavigationService>();
//                })
//                .Build();

//            // Start host (runs hosted services if any)
//            await _host.StartAsync();

//            // Database initialization
//            using var scope = _host.Services.CreateScope();
//            var dbContext = scope.ServiceProvider.GetRequiredService<AttendanceDbContext>();
//            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
//            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

//            try
//            {
//                // Apply migrations (safe to call multiple times)
//                await dbContext.Database.MigrateAsync();

//                // Ensure roles
//                await CreateRoleIfNotExists(roleManager, "Admin");
//                await CreateRoleIfNotExists(roleManager, "User");

//                // Seed admin user (idempotent)
//                const string adminEmail = "admin@example.com";
//                const string adminPassword = "Admin@123";

//                if (await userManager.FindByEmailAsync(adminEmail) == null)
//                {
//                    var adminUser = new User
//                    {
//                        UserName = adminEmail,
//                        Email = adminEmail,
//                        EmailConfirmed = true
//                    };

//                    var result = await userManager.CreateAsync(adminUser, adminPassword);
//                    if (result.Succeeded)
//                    {
//                        await userManager.AddToRoleAsync(adminUser, "Admin");
//                    }
//                    else
//                    {
//                        var errors = string.Join("; ", result.Errors.Select(er => er.Description));
//                        throw new InvalidOperationException($"Failed to create admin user: {errors}");
//                    }
//                }
//            }
//            catch (Exception dbEx)
//            {
//                throw new InvalidOperationException("Database initialization failed. Check connection string and SQL Server access.", dbEx);
//            }
//        }

//        private async Task HandleStartupErrorAsync(Exception ex)
//        {
//            if (_logoSplash != null)
//            {
//                await _logoSplash.FadeOutAndCloseAsync();
//                _logoSplash = null;
//            }

//            MessageBox.Show(
//                $"Startup failed: {ex.Message}\n\n{ex.StackTrace}",
//                "Critical Error",
//                MessageBoxButton.OK,
//                MessageBoxImage.Error
//            );
//        }

//        private static async Task CreateRoleIfNotExists(RoleManager<IdentityRole> roleManager, string roleName)
//        {
//            if (!await roleManager.RoleExistsAsync(roleName))
//            {
//                await roleManager.CreateAsync(new IdentityRole(roleName));
//            }
//        }

//        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
//        {
//            // Log or handle gracefully
//            MessageBox.Show(
//                $"Unhandled exception: {e.Exception.Message}",
//                "Unexpected Error",
//                MessageBoxButton.OK,
//                MessageBoxImage.Error
//            );

//            e.Handled = true;
//            // Optional: Shutdown(-1);
//        }

//        protected override async void OnExit(ExitEventArgs e)
//        {
//            if (_host != null)
//            {
//                await _host.StopAsync(TimeSpan.FromSeconds(5));
//                _host.Dispose();
//            }

//            base.OnExit(e);
//        }
//    }
//}
