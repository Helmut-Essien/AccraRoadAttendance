using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using AccraRoadAttendance.Data;
using AccraRoadAttendance.Models;
using LiveCharts;
using LiveCharts.Wpf;
using Microsoft.EntityFrameworkCore;

namespace AccraRoadAttendance.Views.Pages.Dashboard
{
    public partial class Dashboard : UserControl
    {
        private readonly AttendanceDbContext _context;

        public Dashboard(AttendanceDbContext context)
        {
            InitializeComponent();
            _context = context;
            LoadDashboardData();
        }

        private void LoadDashboardData()
        {
            try
            {
                // Load total members
                var totalMembers = _context.Members.Count();
                TotalMembersCount.Text = totalMembers.ToString();

                // Load total men and women
                var totalMen = _context.Members.Count(m => m.Sex == Member.Gender.Male);
                var totalWomen = _context.Members.Count(m => m.Sex == Member.Gender.Female);
                TotalMenCount.Text = totalMen.ToString();
                TotalWomenCount.Text = totalWomen.ToString();

                // Load last Sunday attendance
                var lastSundaySummary = _context.ChurchAttendanceSummaries
                    .Where(s => s.ServiceType == ServiceType.SundayService)
                    .OrderByDescending(s => s.SummaryDate)
                    .FirstOrDefault();
                LastSundayAttendance.Text = lastSundaySummary?.TotalPresent.ToString() ?? "0";

                // Load absent members (2+ months)
                var twoMonthsAgo = DateTime.UtcNow.AddDays(-14);
                var absentMembers = _context.Members
                    .Select(m => new
                    {
                        m.FullName,
                        LastAttendanceDate = _context.Attendances
                            .Where(a => a.MemberId == m.Id)
                            .Max(a => (DateTime?)a.ServiceDate) ?? m.MembershipStartDate
                    })
                    .Where(m => m.LastAttendanceDate < twoMonthsAgo)
                    .ToList();
                AbsentMembersList.ItemsSource = absentMembers;

                // Load attendance trends for charting
                var attendanceData = _context.Attendances
                    .Where(a => a.Status == AttendanceStatus.Present)
                    .GroupBy(a => a.ServiceDate)
                    .Select(g => new
                    {
                        Date = g.Key,
                        Count = g.Count()
                    })
                    .OrderBy(a => a.Date)
                    .ToList();

                // Prepare data for LiveCharts
                var dates = attendanceData.Select(a => a.Date.ToString("dd/MM/yyyy")).ToArray();
                var counts = attendanceData.Select(a => (double)a.Count).ToArray();

                // Set up the chart
                AttendanceChart.Series = new SeriesCollection
                {
                    new LineSeries
                    {
                        Title = "Attendance",
                        Values = new ChartValues<double>(counts)
                    }
                };

                AttendanceChart.AxisX = new AxesCollection
                {
                    new Axis
                    {
                        Title = "Date",
                        Labels = dates
                    }
                };

                AttendanceChart.AxisY = new AxesCollection
                {
                    new Axis
                    {
                        Title = "Attendance Count",
                        LabelFormatter = value => value.ToString("N0")
                    }
                };
            }
            catch (Exception ex)
            {
                // Basic error handling - you might want to log this or show a message to the user
                Console.WriteLine($"Error loading dashboard data: {ex.Message}");
            }
        }
    }
}