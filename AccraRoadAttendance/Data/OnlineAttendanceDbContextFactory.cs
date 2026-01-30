//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Design;
//using Microsoft.Extensions.Configuration;
//using System.IO;

//namespace AccraRoadAttendance.Data
//{
//    public class OnlineAttendanceDbContextFactory : IDesignTimeDbContextFactory<OnlineAttendanceDbContext>
//    {
//        public OnlineAttendanceDbContext CreateDbContext(string[] args)
//        {
//            var configuration = new ConfigurationBuilder()
//                .SetBasePath(Directory.GetCurrentDirectory())
//                .AddJsonFile("appsettings.json")
//                .AddJsonFile($"appsettings.Development.json", optional: true)
//                .Build();

//            var connectionString = configuration.GetConnectionString("OnlineConnection");
//            var optionsBuilder = new DbContextOptionsBuilder<OnlineAttendanceDbContext>();
//            optionsBuilder.UseSqlServer(
//                connectionString,
//                sqlServerOptions => sqlServerOptions.EnableRetryOnFailure(
//                    maxRetryCount: 5,
//                    maxRetryDelay: TimeSpan.FromSeconds(30),
//                    errorNumbersToAdd: null
//                ));

//            return new OnlineAttendanceDbContext(optionsBuilder.Options);
//        }
//    }
//}



using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace AccraRoadAttendance.Data
{
    public class OnlineAttendanceDbContextFactory : IDesignTimeDbContextFactory<OnlineAttendanceDbContext>
    {
        public OnlineAttendanceDbContext CreateDbContext(string[] args)
        {
            // Try to read environment from real env var (set in local VS, CI, etc.)
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                               ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
                               ?? "Production";  // ← safe default for CI/deploy

            var basePath = Directory.GetCurrentDirectory();

            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: true)
                // Optional: also support user secrets locally (very useful for dev)
                .AddUserSecrets<OnlineAttendanceDbContextFactory>(optional: true)
                .Build();

            var connectionString = configuration.GetConnectionString("OnlineConnection")
                ?? throw new InvalidOperationException(
                    $"Connection string 'OnlineConnection' not found in configuration "
                    + $"(environment: {environmentName}, base path: {basePath}).");

            var optionsBuilder = new DbContextOptionsBuilder<OnlineAttendanceDbContext>();
            optionsBuilder.UseSqlServer(
                connectionString,
                sqlServerOptions => sqlServerOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null));

            return new OnlineAttendanceDbContext(optionsBuilder.Options);
        }
    }
}