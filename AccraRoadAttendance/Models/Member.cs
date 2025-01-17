// AccraRoadAttendance.Models.Member.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AccraRoadAttendance.Models
{
    public class Member
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Sex { get; set; }
        public string PicturePath { get; set; }
        public bool IsPresent { get; set; }
    }
}