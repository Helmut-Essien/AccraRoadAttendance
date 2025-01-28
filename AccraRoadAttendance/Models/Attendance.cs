using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }

    public enum ServiceType
    {
        SundayService,
        WednesdayPrayer,
        ThursdayBibleStudy,
        SpecialEvent
    }

    public enum AttendanceStatus
    {
        Present,
        Absent,
        Excused
    }


}
