using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using AccraRoadAttendance.Models;

namespace AccraRoadAttendance.Data
{
    public class AttendanceDbContext : IdentityDbContext<Member>
    {
        public DbSet<Member> Members { get; set; }
        public DbSet<Attendance> Attendances { get; set; }

        public AttendanceDbContext(DbContextOptions<AttendanceDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure Member entity
            builder.Entity<Member>()
                .HasIndex(m => m.Email) // Ensure Email is unique
                .IsUnique();

            // Configure Attendance entity
            builder.Entity<Attendance>()
                .HasOne(a => a.Member)
                .WithMany() // If you want to track attendance history per Member, change to `.WithMany(m => m.Attendances)`
                .HasForeignKey(a => a.MemberId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure index for better performance
            builder.Entity<Attendance>()
                .HasIndex(a => a.Date)
                .HasDatabaseName("IX_Attendance_Date");
        }
    }
}
