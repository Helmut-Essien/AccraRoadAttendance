using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using AccraRoadAttendance.Data;
using AccraRoadAttendance.Models;
using LiveCharts;
using LiveCharts.Wpf;
using Microsoft.EntityFrameworkCore;

namespace AccraRoadAttendance.Views.Pages.Dashboard
{
    public partial class Dashboard : UserControl, INotifyPropertyChanged
    {
        private readonly AttendanceDbContext _context;

        public Dashboard(AttendanceDbContext context)
        {
            InitializeComponent();
            _context = context;
            DataContext = this;
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
                if (lastSundaySummary != null)
                {
                    LastSundayDate = lastSundaySummary.SummaryDate.ToShortDateString();
                    LastSundayMen = lastSundaySummary.TotalMalePresent.ToString();
                    LastSundayWomen = lastSundaySummary.TotalFemalePresent.ToString();
                    LastSundayTotal = lastSundaySummary.TotalPresent.ToString();
                    LastSundayOffering = lastSundaySummary.OfferingAmount.ToString("C");
                }
                else
                {
                    LastSundayDate = "N/A";
                    LastSundayMen = "0";
                    LastSundayWomen = "0";
                    LastSundayTotal = "0";
                    LastSundayOffering = "0.00";
                }

                // Load absent members (2+ weeks)
                var twoWeeksAgo = DateTime.UtcNow.AddDays(-14);
                var absentMembers = _context.Members
                    .Select(m => new
                    {
                        m.FullName,
                        m.PhoneNumber,
                        LastAttendanceDate = _context.Attendances
                            .Where(a => a.MemberId == m.Id && a.Status == AttendanceStatus.Present)
                            .Max(a => (DateTime?)a.ServiceDate) ?? m.MembershipStartDate
                    })
                    .Where(m => m.LastAttendanceDate < twoWeeksAgo)
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

                // Set up the chart as a bar chart
                AttendanceChart.Series = new SeriesCollection
                {
                    new ColumnSeries
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

        private string _lastSundayDate;
        public string LastSundayDate
        {
            get => _lastSundayDate;
            set { _lastSundayDate = value; OnPropertyChanged(nameof(LastSundayDate)); }
        }

        private string _lastSundayMen;
        public string LastSundayMen
        {
            get => _lastSundayMen;
            set { _lastSundayMen = value; OnPropertyChanged(nameof(LastSundayMen)); }
        }

        private string _lastSundayWomen;
        public string LastSundayWomen
        {
            get => _lastSundayWomen;
            set { _lastSundayWomen = value; OnPropertyChanged(nameof(LastSundayWomen)); }
        }

        private string _lastSundayTotal;
        public string LastSundayTotal
        {
            get => _lastSundayTotal;
            set { _lastSundayTotal = value; OnPropertyChanged(nameof(LastSundayTotal)); }
        }

        private string _lastSundayOffering;
        public string LastSundayOffering
        {
            get => _lastSundayOffering;
            set { _lastSundayOffering = value; OnPropertyChanged(nameof(LastSundayOffering)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
