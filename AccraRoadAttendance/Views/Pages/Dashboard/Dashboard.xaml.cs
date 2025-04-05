using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Threading;
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
                // Load total men and women
                var totalMen = _context.Members.Count(m => m.Sex == Member.Gender.Male);
                var totalWomen = _context.Members.Count(m => m.Sex == Member.Gender.Female);

                // Initialize display values
                DisplayTotalMembers = 0;
                DisplayMen = 0;
                DisplayWomen = 0;

                // Start animation
                int steps = 50;
                int duration = 100; // 1 second
                int stepTime = duration / steps; // 20ms
                int currentStep = 0;

                var timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromMilliseconds(stepTime);
                timer.Tick += (s, e) =>
                {
                    currentStep++;
                    if (currentStep <= steps)
                    {
                        double progress = (double)currentStep / steps;
                        DisplayTotalMembers = (int)(progress * totalMembers);
                        DisplayMen = (int)(progress * totalMen);
                        DisplayWomen = (int)(progress * totalWomen);
                    }
                    else
                    {
                        // Ensure final values are exact
                        DisplayTotalMembers = totalMembers;
                        DisplayMen = totalMen;
                        DisplayWomen = totalWomen;
                        timer.Stop();
                    }
                };
                timer.Start();

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

                // Load attendance trends for the past six Sundays
                var lastSixSundays = _context.ChurchAttendanceSummaries
                    .Where(s => s.ServiceType == ServiceType.SundayService)
                    .OrderByDescending(s => s.SummaryDate)
                    .Take(6)
                    .OrderBy(s => s.SummaryDate)
                    .ToList();

                var dates = lastSixSundays.Select(s => s.SummaryDate.ToString("dd/MM/yyyy")).ToArray();
                var counts = lastSixSundays.Select(s => (double)s.TotalPresent).ToArray();

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

        // Displayed values for animation
        private int _displayTotalMembers;
        public int DisplayTotalMembers
        {
            get => _displayTotalMembers;
            set
            {
                _displayTotalMembers = value;
                OnPropertyChanged(nameof(DisplayTotalMembers));
            }
        }

        private int _displayMen;
        public int DisplayMen
        {
            get => _displayMen;
            set
            {
                _displayMen = value;
                OnPropertyChanged(nameof(DisplayMen));
            }
        }

        private int _displayWomen;
        public int DisplayWomen
        {
            get => _displayWomen;
            set
            {
                _displayWomen = value;
                OnPropertyChanged(nameof(DisplayWomen));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
