﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccraRoadAttendance.Models
{
    public class ChurchAttendanceSummary
    {
        [Column(TypeName = "date")]
        public DateTime SummaryDate { get; set; }

        public ServiceType ServiceType { get; set; }

        public int TotalPresent { get; set; }
        public int TotalMalePresent { get; set; }
        public int TotalFemalePresent { get; set; }
        public int TotalMembers { get; set; }
        public int Visitors { get; set; }
        public int Children { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal OfferingAmount { get; set; }

        [MaxLength(500)]
        public string? ServiceTheme { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime SummaryLastModified { get; set; } = DateTime.UtcNow;

        public bool SummarySyncStatus { get; set; } = false;
    }
}
