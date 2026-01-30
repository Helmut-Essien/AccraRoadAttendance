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
using System.Linq;

namespace AccraRoadAttendance.Data
{
    public class OnlineAttendanceDbContextFactory : IDesignTimeDbContextFactory<OnlineAttendanceDbContext>
    {
        public OnlineAttendanceDbContext CreateDbContext(string[] args)
        {
            // 1. Get environment: prefer command-line arg, then real env var, then safe CI default
            string? environmentName = null;

            // Check for --environment XXX from dotnet ef -- --environment Production
            var envArg = args.FirstOrDefault(a => a.StartsWith("--environment", StringComparison.OrdinalIgnoreCase));
            if (envArg != null)
            {
                // Handle both --environment=Production and --environment Production
                var parts = envArg.Split(new[] { '=' }, 2);
                environmentName = parts.Length > 1 ? parts[1].Trim() : args.SkipWhile(a => a != envArg).Skip(1).FirstOrDefault();
            }

            // Fallback to real environment variable (useful locally or if set in CI)
            environmentName ??= Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                             ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
                             ?? "Production";  // ← key: safe default for GitHub Actions

            var basePath = Directory.GetCurrentDirectory();

            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            // Only add env-specific file if it's not Production (or if explicitly requested)
            if (!string.Equals(environmentName, "Production", StringComparison.OrdinalIgnoreCase))
            {
                configBuilder.AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: true);
            }

            var configuration = configBuilder.Build();

            // Use --connection override if provided (highest priority)
            string? connectionString = null;
            var connArg = args.FirstOrDefault(a => a.StartsWith("--connection", StringComparison.OrdinalIgnoreCase));
            if (connArg != null)
            {
                var parts = connArg.Split(new[] { '=' }, 2);
                connectionString = parts.Length > 1 ? parts[1].Trim('"') : args.SkipWhile(a => a != connArg).Skip(1).FirstOrDefault();
            }

            // Fallback to config file
            connectionString ??= configuration.GetConnectionString("OnlineConnection")
                ?? throw new InvalidOperationException(
                    $"No 'OnlineConnection' connection string found. "
                    + $"Environment: {environmentName}, Path: {basePath}, Args: {string.Join(" ", args)}");

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
