using AccraRoadAttendance.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccraRoadAttendance.Data
{
    // Add this class to your Data folder
    public class OnlineAttendanceDbContext : IdentityDbContext<User>
    {
        public  DbSet<Member> Members { get; set; }
        public new DbSet<User> Users { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<ChurchAttendanceSummary> ChurchAttendanceSummaries { get; set; }
        public DbSet<SyncMetadata> SyncMetadata { get; set; }

        public OnlineAttendanceDbContext(DbContextOptions<OnlineAttendanceDbContext> options)
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

                // Generate a new GUID on insert if you don’t supply one:
                entity.Property(m => m.Id)
                .ValueGeneratedOnAdd();

                entity.HasIndex(m => m.Email).IsUnique();
                entity.HasIndex(m => m.PhoneNumber).IsUnique();

                entity.HasMany(m => m.Attendances)
                    .WithOne(a => a.Member)
                    .HasForeignKey(a => a.MemberId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(m => m.Sex)
                    .HasConversion<string>();


            });

            // Configure Attendance entity
            builder.Entity<Attendance>(entity =>
            {
                entity.Property(a => a.Id)
                      .ValueGeneratedOnAdd();

                entity.HasIndex(a => new { a.MemberId, a.ServiceDate, a.ServiceType })
                      .IsUnique()
                      .HasDatabaseName("IX_Attendances_MemberId_ServiceDate_ServiceType");

                entity.HasIndex(a => a.ServiceDate)
                      .HasDatabaseName("IX_Attendance_ServiceDate");

                entity.HasIndex(a => new { a.ServiceDate, a.ServiceType })
                      .HasDatabaseName("IX_ServiceDate_ServiceType");

                entity.Property(a => a.ServiceType)
                      .HasConversion<string>();

                entity.Property(a => a.Status)
                      .HasConversion<string>();
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

