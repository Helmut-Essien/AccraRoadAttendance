using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace AccraRoadAttendance.Data
{
    public class OnlineAttendanceDbContextFactory : IDesignTimeDbContextFactory<OnlineAttendanceDbContext>
    {
        public OnlineAttendanceDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.Development.json", optional: true)
                .Build();

            var connectionString = configuration.GetConnectionString("OnlineConnection");
            var optionsBuilder = new DbContextOptionsBuilder<OnlineAttendanceDbContext>();
            optionsBuilder.UseSqlServer(
                connectionString,
                sqlServerOptions => sqlServerOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null
                ));

            return new OnlineAttendanceDbContext(optionsBuilder.Options);
        }
    }
}
