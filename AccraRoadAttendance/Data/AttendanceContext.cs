// AccraRoadAttendance.Data.AttendanceContext.cs
using Microsoft.EntityFrameworkCore;
using AccraRoadAttendance.Models;

namespace AccraRoadAttendance.Data
{
    public class AttendanceContext : DbContext
    {
        public DbSet<Member> Members { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=attendance.db");
        }
    }
}