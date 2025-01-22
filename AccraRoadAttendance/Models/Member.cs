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

        [Required]
        public Gender Sex { get; set; } // Enum for better type safety

        
        [MaxLength(200)]
        public required string PicturePath { get; set; }
    }

    // Enum for Gender
    public enum Gender
    {
        Male,
        Female,
        
    }

}