using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AccraRoadAttendance.Data
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AttendanceDbContext>
    {
        public AttendanceDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AttendanceDbContext>();
            optionsBuilder.UseSqlServer("Server=FINSERVE\\SQLEXPRESS;Database=AttendanceDb;Integrated Security=True;Trusted_Connection=True;TrustServerCertificate=True;");

            return new AttendanceDbContext(optionsBuilder.Options);
        }
    }
}