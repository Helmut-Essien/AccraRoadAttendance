using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace AccraRoadAttendance.Models
{
    public class Attendance
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Member")]
        public required string MemberId { get; set; }
        public required Member Member { get; set; }

        [Required]
        [Column(TypeName = "date")]
        public DateTime ServiceDate { get; set; }

        [Required]
        public ServiceType ServiceType { get; set; } // Sunday, Midweek, Special

        [Required]
        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public AttendanceStatus Status { get; set; } // Present, Absent, Excused

        [MaxLength(500)]
        public string? Notes { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime AttendanceLastModified { get; set; } = DateTime.UtcNow;

        public bool AttendanceSyncStatus { get; set; } = false;
    }

    public enum ServiceType
    {
        [Display(Name = "Sunday Service")]
        SundayService,

        [Display(Name = "Wednesday Prayer")]
        WednesdayPrayer,

        [Display(Name = "Thursday Bible Study")]
        ThursdayBibleStudy,

        [Display(Name = "Special Event")]
        SpecialEvent
    }

    public enum AttendanceStatus
    {
        Present,
        Absent,
        Excused
    }



    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum enumValue)
        {
            var memberInfo = enumValue.GetType().GetMember(enumValue.ToString());
            if (memberInfo.Length > 0)
            {
                var displayAttribute = memberInfo[0].GetCustomAttribute<DisplayAttribute>();
                if (displayAttribute != null)
                    return displayAttribute.Name;
            }
            return enumValue.ToString();
        }

        public static ServiceType? GetEnumValueFromDisplayName(string displayName)
        {
            foreach (ServiceType value in Enum.GetValues(typeof(ServiceType)))
            {
                if (value.GetDisplayName().Equals(displayName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return value;
                }
            }
            return null;
        }



    }
}
    




 
