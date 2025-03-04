// AccraRoadAttendance.Models.Member.cs
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AccraRoadAttendance.Models
{
    public class Member : IdentityUser
    {
        [MaxLength(50)]
        public required string FirstName { get; set; }

        [MaxLength(50)]
        public required string LastName { get; set; }

        [MaxLength(50)]
        public string? OtherNames { get; set; }

        [NotMapped]
        public string FullName => $"{FirstName} {OtherNames} {LastName}".Trim();

        [Required]
        public Gender Sex { get; set; }

        [MaxLength(200)]
        public string? PicturePath { get; set; } // Made nullable

        [Column(TypeName = "date")]
        public DateTime MembershipStartDate { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        [Column(TypeName = "date")]
        public DateTime? DateOfBirth { get; set; }

        // Not stored in DB - calculated property
        [NotMapped]
        public string AgeGroup => CalculateAgeGroup();

        private string CalculateAgeGroup()
        {
            if (!DateOfBirth.HasValue) return "Unknown";

            var age = DateTime.Today.Year - DateOfBirth.Value.Year;
            return age switch
            {
                <= 12 => "Child",
                <= 19 => "Teen",
                <= 35 => "YoungAdult",
                <= 55 => "Adult",
                _ => "Senior"
            };
        }


        // Define Gender FIRST
        public enum Gender
        {
            Male,
            Female,
            Other // Added for inclusivity
        }

        // Navigation property
        public List<Attendance> Attendances { get; set; } = new();


    }



}