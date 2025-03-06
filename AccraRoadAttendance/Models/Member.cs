﻿// AccraRoadAttendance.Models.Member.cs
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


        // New properties from the PDF form
        [MaxLength(100)]
        public string? Nationality { get; set; }

        [MaxLength(100)]
        [Display(Name = "Educational Level")]
        public string? EducationalLevel { get; set; }

        [MaxLength(200)]
        public string? Address { get; set; }

        [Display(Name = "Has Family Member In Church")]
        public bool HasFamilyMemberInChurch { get; set; }

        [Display(Name = "Marital Status")]
        public MaritalStatus maritalStatus { get; set; }

        [Display(Name = "Occupation Type")]
        public OccupationType occupationType { get; set; }

        [Display(Name = "Is Baptized")]
        public bool IsBaptized { get; set; }

        [Column(TypeName = "date")]
        public DateTime? BaptismDate { get; set; }

        [MaxLength(100)]
        public string? Hometown { get; set; }

        [MaxLength(100)]
        [Display(Name = "Next Of Kin Name")]
        public string? NextOfKinName { get; set; }

        [MaxLength(50)]
        [Display(Name = "Next Of Kin Contact")]
        public string? NextOfKinContact { get; set; }

        [MaxLength(100)]
        [Display(Name = "Family Member Name")]
        public string? FamilyMemberName { get; set; }

        [MaxLength(50)]
        [Display(Name = "Family Member Contact")]
        public string? FamilyMemberContact { get; set; }

        [MaxLength(100)]
        [Display(Name = "Role In Church")]
        public string? MemberRole { get; set; }

        [MaxLength(500)]
        public string? Skills { get; set; }

        // Enums
        public enum Gender
        {
            Male,
            Female,
           
        }

        public enum MaritalStatus
        {
            Married,
            Single,
            Widowed,
            Divorced
        }

        public enum OccupationType
        {
            [Display(Name = "Self Employed")]
            SelfEmployed,
            [Display(Name = "Salary Worker")]
            SalaryWorker,
            [Display(Name = "Student")]
            Student,
            [Display(Name = "Unemployed")]
            Unemployed
        }









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


        //// Define Gender FIRST
        //public enum Gender
        //{
        //    Male,
        //    Female,
        //    Other // Added for inclusivity
        //}

        // Navigation property
        public List<Attendance> Attendances { get; set; } = new();


    }



}