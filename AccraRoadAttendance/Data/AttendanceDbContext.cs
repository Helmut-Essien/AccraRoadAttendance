﻿using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using AccraRoadAttendance.Models;
using Microsoft.AspNetCore.Identity;

namespace AccraRoadAttendance.Data
{
    public class AttendanceDbContext : IdentityDbContext<Member>
    {
        public DbSet<Member> Members { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<ChurchAttendanceSummary> ChurchAttendanceSummaries { get; set; }

        public AttendanceDbContext(DbContextOptions<AttendanceDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Rename Identity tables for SQLite compatibility
            builder.Entity<IdentityRole>().ToTable("AspNetRoles");
            builder.Entity<Member>().ToTable("AspNetUsers");
            builder.Entity<IdentityUserClaim<string>>().ToTable("AspNetUserClaims");
            builder.Entity<IdentityUserLogin<string>>().ToTable("AspNetUserLogins");
            builder.Entity<IdentityUserToken<string>>().ToTable("AspNetUserTokens");
            builder.Entity<IdentityRoleClaim<string>>().ToTable("AspNetRoleClaims");
            builder.Entity<IdentityUserRole<string>>().ToTable("AspNetUserRoles");

            // Configure Member entity
            builder.Entity<Member>(entity =>
            {
                entity.HasIndex(m => m.Email).IsUnique();

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
            });
        }
    }
}