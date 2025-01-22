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
        public DateTime Date { get; set; } // Date of the Sunday service

        [Required]
        public bool IsPresent { get; set; } // True if the member attended
    }


}
