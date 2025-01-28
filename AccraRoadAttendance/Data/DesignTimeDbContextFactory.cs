using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AccraRoadAttendance.Data
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AttendanceDbContext>
    {
        public AttendanceDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AttendanceDbContext>();
            optionsBuilder.UseSqlite("Data Source=attendance.db");

            return new AttendanceDbContext(optionsBuilder.Options);
        }
    }
}