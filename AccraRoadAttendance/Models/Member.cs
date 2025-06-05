// AccraRoadAttendance.Models.Member.cs
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AccraRoadAttendance.Models
{
    public class Member 
    {
        // 1) Declare the new GUID primary key:
        [Key]
        [Column("Id", TypeName = "uniqueidentifier")]
        public Guid Id { get; set; }

        [MaxLength(50)]
        public required string FirstName { get; set; }

        [MaxLength(50)]
        public required string LastName { get; set; }

        [MaxLength(50)]
        public string? OtherNames { get; set; }

        [NotMapped]
        public string FullName => $"{FirstName} {OtherNames} {LastName}".Trim();

        public string? PhoneNumber { get; set; }
        [MaxLength(50)]
        public string? Email { get; set; } // Made nullable
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
        public EducationalLevel? educationalLevel { get; set; }

        [MaxLength(200)]
        public string? Address { get; set; }

        [MaxLength(200)]
        public string? Location { get; set; }

        [Display(Name = "Has Family Member In Church")]
        public bool HasFamilyMemberInChurch { get; set; }

        [Display(Name = "Marital Status")]
        public MaritalStatus maritalStatus { get; set; }

        [Display(Name = "Occupation Type")]
        public OccupationType occupationType { get; set; }

        [Display(Name = "Is Baptized")]
        public bool IsBaptized { get; set; }

        [Display(Name = "Place of baptism")]
        public string? PlaceOfBaptism { get; set; }

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
        [Display(Name = "Mother's name")]
        public string? MotherName { get; set; }

        [MaxLength(50)]
        [Display(Name = "Mother's contact")]
        public string? MotherContact { get; set; }

        [MaxLength(100)]
        [Display(Name = "Father's name")]
        public string? FatherName { get; set; }

        [MaxLength(50)]
        [Display(Name = "Father's contact")]
        public string? FatherContact { get; set; }

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

        [Column(TypeName = "datetime")]
        public DateTime LastModified { get; set; } = DateTime.UtcNow;

        public bool SyncStatus { get; set; } = false;

        // Enums
        public enum Gender
        {
            [Display(Name = "Male")]
            Male,
            [Display(Name = "Female")]
            Female,
           
        }

        public enum MaritalStatus
        {
            [Display(Name = "Married")]
            Married,
            [Display(Name = "Single")]
            Single,
            [Display(Name = "Widowed")]
            Widowed,
            [Display(Name = "Divorced")]
            Divorced
        }

        [MaxLength(100)]
        [Display(Name = "Spouse name")]
        public string? SpouseName { get; set; }

        [MaxLength(50)]
        [Display(Name = "Spouse contact")]
        public string? SpouseContact { get; set; }

        public enum OccupationType
        {
            [Display(Name = "Self Employed")]
            SelfEmployed,
            [Display(Name = "Employed")]
            SalaryWorker,
            [Display(Name = "Student")]
            Student,
            [Display(Name = "Unemployed")]
            Unemployed,
            [Display(Name = "Apprentice")]
            Apprentice,
            [Display(Name = "Retired")]
            Retired
        }

        [MaxLength(100)]
        [Display(Name = "Occupation description")]
        public string? OccupationDescription { get; set; }

        public enum EducationalLevel
        {
            [Display(Name = "No Formal Education")]
            NoFormalEducation,
            [Display(Name = "Primary School")]
            PrimarySchool,
            [Display(Name = "Secondary School")]
            SecondarySchool,
            [Display(Name = "Tertiary")]
            Tertiary,
            [Display(Name = "Post Graduate")]
            PostGraduate
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




        // Navigation property
        public User? User { get; set; }
        public List<Attendance> Attendances { get; set; } = new();


    }



}