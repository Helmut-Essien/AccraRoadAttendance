using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using AccraRoadAttendance.Models;

namespace AccraRoadAttendance.Data
{
    public class AttendanceContext : IdentityDbContext<Member>
    {
        public DbSet<Member> Members { get; set; }

        public AttendanceContext(DbContextOptions<AttendanceContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Custom configurations if needed
            // Example: builder.Entity<Member>().Property(m => m.AdditionalProperty).IsRequired();
        }
    }
}