using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace AccraRoadAttendance.Data
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AttendanceDbContext>
    {
        public AttendanceDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.Development.json", optional: true) // Add environment-specific
            .Build();

            var optionsBuilder = new DbContextOptionsBuilder<AttendanceDbContext>();
            optionsBuilder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));

            return new AttendanceDbContext(optionsBuilder.Options);
        }
    }
}