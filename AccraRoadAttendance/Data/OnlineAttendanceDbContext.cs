using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccraRoadAttendance.Data
{
    // Add this class to your Data folder
    public class OnlineAttendanceDbContext : AttendanceDbContext
    {
        public OnlineAttendanceDbContext(DbContextOptions<AttendanceDbContext> options)
            : base(options) { }
    }
}
