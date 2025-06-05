using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace AccraRoadAttendance.Models
{
    public class User : IdentityUser
    {
        public string? MemberId { get; set; }
        public Member? Member { get; set; }

       
    }
}