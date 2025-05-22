using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using AccraRoadAttendance.Models;
using Microsoft.AspNetCore.Identity;

namespace AccraRoadAttendance.Data
{
    public class AttendanceDbContext : IdentityDbContext<User>
    {
        public new DbSet<Member> Members { get; set; }
        public new DbSet<User> Users { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<ChurchAttendanceSummary> ChurchAttendanceSummaries { get; set; }
        public DbSet<SyncMetadata> SyncMetadata { get; set; }

        public AttendanceDbContext(DbContextOptions<AttendanceDbContext> options)
            : base(options)
        {
        }



        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Rename Identity tables for SQLite compatibility
            builder.Entity<IdentityRole>().ToTable("AspNetRoles");
            builder.Entity<User>().ToTable("AspNetUsers");
           
            builder.Entity<IdentityUserClaim<string>>().ToTable("AspNetUserClaims");
            builder.Entity<IdentityUserLogin<string>>().ToTable("AspNetUserLogins");
            builder.Entity<IdentityUserToken<string>>().ToTable("AspNetUserTokens");
            builder.Entity<IdentityRoleClaim<string>>().ToTable("AspNetRoleClaims");
            builder.Entity<IdentityUserRole<string>>().ToTable("AspNetUserRoles");


            // Configure User entity
            builder.Entity<User>(entity =>
            {
                entity.HasOne(u => u.Member)
                      .WithOne(m => m.User)
                      .HasForeignKey<User>(u => u.MemberId)
                      .IsRequired();
                entity.HasIndex(u => u.Email).IsUnique();
            });

            // Configure Member entity
            builder.Entity<Member>(entity =>
            {
                entity.HasIndex(m => m.Email).IsUnique();
                entity.HasIndex(m => m.PhoneNumber).IsUnique();

                entity.HasMany(m => m.Attendances)
                    .WithOne(a => a.Member)
                    .HasForeignKey(a => a.MemberId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(m => m.Sex)
                    .HasConversion<string>();

                //entity.Property(m => m.LastModified)
                //     .HasColumnType("datetime")
                //     .IsRequired()
                //     .HasDefaultValueSql("GETUTCDATE()");

                //entity.Property(m => m.SyncStatus)
                //      .IsRequired()
                //      .HasDefaultValue(false);
            });

            // Configure Attendance entity
            builder.Entity<Attendance>(entity =>
            {
                entity.HasIndex(a => a.ServiceDate)
                    .HasDatabaseName("IX_Attendance_ServiceDate");

                entity.HasIndex(a => new { a.ServiceDate, a.ServiceType })
                    .HasDatabaseName("IX_ServiceDate_ServiceType");

                entity.Property(a => a.ServiceType)
                    .HasConversion<string>();

                entity.Property(a => a.Status)
                    .HasConversion<string>();

                //entity.Property(m => m.LastModified)
                //     .HasColumnType("datetime")
                //     .IsRequired()
                //     .HasDefaultValueSql("GETUTCDATE()");

                //entity.Property(m => m.SyncStatus)
                //      .IsRequired()
                //      .HasDefaultValue(false);
            });

            // Configure ChurchAttendanceSummary entity
            builder.Entity<ChurchAttendanceSummary>(entity =>
            {
                entity.HasKey(c => new { c.SummaryDate, c.ServiceType });

                entity.Property(c => c.ServiceType)
                    .HasConversion<string>();

                entity.HasIndex(c => c.SummaryDate)
                    .HasDatabaseName("IX_Summary_Date");

                entity.HasIndex(c => c.ServiceType)
                    .HasDatabaseName("IX_Summary_ServiceType");

                //entity.Property(m => m.LastModified)
                //     .HasColumnType("datetime")
                //     .IsRequired()
                //     .HasDefaultValueSql("GETUTCDATE()");

                //entity.Property(m => m.SyncStatus)
                //      .IsRequired()
                //      .HasDefaultValue(false);
            });

            //Configure SyncMetadata entity
            builder.Entity<SyncMetadata>(entity =>
            {
                entity.HasKey(sm => sm.Key);
                entity.Property(sm => sm.Value).IsRequired();
            });
        }
    }

}
