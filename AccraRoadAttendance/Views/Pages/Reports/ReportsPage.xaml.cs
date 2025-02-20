using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AccraRoadAttendance.Data;
using AccraRoadAttendance.Models;
using MaterialDesignThemes.Wpf;
using Microsoft.EntityFrameworkCore;

namespace AccraRoadAttendance.Views.Pages.Reports
{
    public partial class ReportsPage : UserControl
    {
        private readonly AttendanceDbContext _context;

        public ReportsPage(AttendanceDbContext context)
        {
            InitializeComponent();
            _context = context;
        }

        private void ReportTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ReportTypeComboBox.SelectedItem == null) return;

            var selectedReport = (ReportTypeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            ParametersPanel.Children.Clear();

            // Common parameters
            var startDateLabel = new TextBlock
            {
                Text = "Start Date:",
                Margin = new Thickness(0, 0, 10, 0),
                FontSize = 14,
                FontWeight = FontWeights.Medium
            };
            var startDatePicker = new DatePicker
            {
                Name = "StartDatePicker",
                SelectedDate = DateTime.Today.AddMonths(-1)
            };
            startDatePicker.Style = (Style)Application.Current.FindResource("MaterialDesignDatePicker");
            HintAssist.SetHint(startDatePicker, "Start Date");

            var endDateLabel = new TextBlock
            {
                Text = "End Date:",
                Margin = new Thickness(20, 0, 10, 0),
                FontSize = 14,
                FontWeight = FontWeights.Medium
            };
            var endDatePicker = new DatePicker
            {
                Name = "EndDatePicker",
                SelectedDate = DateTime.Today
            };
            endDatePicker.Style = (Style)Application.Current.FindResource("MaterialDesignDatePicker");
            HintAssist.SetHint(endDatePicker, "End Date");

            switch (selectedReport)
            {
                case "Individual Attendance":
                    var memberLabel = new TextBlock
                    {
                        Text = "Select Member:",
                        Margin = new Thickness(0, 0, 10, 0),
                        FontSize = 14,
                        FontWeight = FontWeights.Medium
                    };
                    var memberComboBox = new ComboBox
                    {
                        Name = "MemberComboBox",
                        ItemsSource = _context.Members.ToList(),
                        DisplayMemberPath = "FullName"
                    };
                    memberComboBox.Style = (Style)Application.Current.FindResource("MaterialDesignFloatingHintComboBox");
                    HintAssist.SetHint(memberComboBox, "Select Member");

                    ParametersPanel.Children.Add(memberLabel);
                    ParametersPanel.Children.Add(memberComboBox);
                    ParametersPanel.Children.Add(startDateLabel);
                    ParametersPanel.Children.Add(startDatePicker);
                    ParametersPanel.Children.Add(endDateLabel);
                    ParametersPanel.Children.Add(endDatePicker);
                    break;

                case "Church Attendance Summary":
                case "Service Type Report":
                case "Demographic Report":
                case "Offering Report":
                case "Visitor and Newcomer Report":
                    ParametersPanel.Children.Add(startDateLabel);
                    ParametersPanel.Children.Add(startDatePicker);
                    ParametersPanel.Children.Add(endDateLabel);
                    ParametersPanel.Children.Add(endDatePicker);
                    if (selectedReport == "Service Type Report")
                    {
                        var serviceTypeLabel = new TextBlock
                        {
                            Text = "Service Type:",
                            Margin = new Thickness(20, 0, 10, 0),
                            FontSize = 14,
                            FontWeight = FontWeights.Medium
                        };
                        var serviceTypeComboBox = new ComboBox
                        {
                            Name = "ServiceTypeComboBox",
                            ItemsSource = Enum.GetValues(typeof(ServiceType))
                        };
                        serviceTypeComboBox.Style = (Style)Application.Current.FindResource("MaterialDesignFloatingHintComboBox");
                        HintAssist.SetHint(serviceTypeComboBox, "Select Service Type");

                        ParametersPanel.Children.Add(serviceTypeLabel);
                        ParametersPanel.Children.Add(serviceTypeComboBox);
                    }
                    break;

                case "Absentee Report":
                    var thresholdLabel = new TextBlock
                    {
                        Text = "Absent Threshold (%):",
                        Margin = new Thickness(0, 0, 10, 0),
                        FontSize = 14,
                        FontWeight = FontWeights.Medium
                    };
                    var thresholdTextBox = new TextBox
                    {
                        Name = "ThresholdTextBox",
                        Text = "50",
                        Width = 50
                    };
                    thresholdTextBox.Style = (Style)Application.Current.FindResource("MaterialDesignTextBox");
                    HintAssist.SetHint(thresholdTextBox, "Absent Threshold (%)");

                    ParametersPanel.Children.Add(thresholdLabel);
                    ParametersPanel.Children.Add(thresholdTextBox);
                    ParametersPanel.Children.Add(startDateLabel);
                    ParametersPanel.Children.Add(startDatePicker);
                    ParametersPanel.Children.Add(endDateLabel);
                    ParametersPanel.Children.Add(endDatePicker);
                    break;
            }
        }

        //private void ReportTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    if (ReportTypeComboBox.SelectedItem == null) return;

        //    var selectedReport = (ReportTypeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
        //    ParametersPanel.Children.Clear();

        //    // Common parameters
        //    var startDateLabel = new TextBlock { Text = "Start Date:", Margin = new Thickness(0, 0, 10, 0) };
        //    var startDatePicker = new DatePicker { Name = "StartDatePicker", SelectedDate = DateTime.Today.AddMonths(-1) };
        //    var endDateLabel = new TextBlock { Text = "End Date:", Margin = new Thickness(20, 0, 10, 0) };
        //    var endDatePicker = new DatePicker { Name = "EndDatePicker", SelectedDate = DateTime.Today };

        //    switch (selectedReport)
        //    {
        //        case "Individual Attendance":
        //            var memberLabel = new TextBlock { Text = "Select Member:", Margin = new Thickness(0, 0, 10, 0) };
        //            var memberComboBox = new ComboBox { Name = "MemberComboBox", ItemsSource = _context.Members.ToList(), DisplayMemberPath = "FullName" };
        //            ParametersPanel.Children.Add(memberLabel);
        //            ParametersPanel.Children.Add(memberComboBox);
        //            ParametersPanel.Children.Add(startDateLabel);
        //            ParametersPanel.Children.Add(startDatePicker);
        //            ParametersPanel.Children.Add(endDateLabel);
        //            ParametersPanel.Children.Add(endDatePicker);
        //            break;

        //        case "Church Attendance Summary":
        //        case "Service Type Report":
        //        case "Demographic Report":
        //        case "Offering Report":
        //        case "Visitor and Newcomer Report":
        //            ParametersPanel.Children.Add(startDateLabel);
        //            ParametersPanel.Children.Add(startDatePicker);
        //            ParametersPanel.Children.Add(endDateLabel);
        //            ParametersPanel.Children.Add(endDatePicker);
        //            if (selectedReport == "Service Type Report")
        //            {
        //                var serviceTypeLabel = new TextBlock { Text = "Service Type:", Margin = new Thickness(20, 0, 10, 0) };
        //                var serviceTypeComboBox = new ComboBox { Name = "ServiceTypeComboBox", ItemsSource = Enum.GetValues(typeof(ServiceType)) };
        //                ParametersPanel.Children.Add(serviceTypeLabel);
        //                ParametersPanel.Children.Add(serviceTypeComboBox);
        //            }
        //            break;

        //        case "Absentee Report":
        //            var thresholdLabel = new TextBlock { Text = "Absent Threshold (%):", Margin = new Thickness(0, 0, 10, 0) };
        //            var thresholdTextBox = new TextBox { Name = "ThresholdTextBox", Text = "50", Width = 50 };
        //            ParametersPanel.Children.Add(thresholdLabel);
        //            ParametersPanel.Children.Add(thresholdTextBox);
        //            ParametersPanel.Children.Add(startDateLabel);
        //            ParametersPanel.Children.Add(startDatePicker);
        //            ParametersPanel.Children.Add(endDateLabel);
        //            ParametersPanel.Children.Add(endDatePicker);
        //            break;
        //    }
        //}

        private void GenerateReport_Click(object sender, RoutedEventArgs e)
        {
            var selectedReport = (ReportTypeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            if (string.IsNullOrEmpty(selectedReport))
            {
                MessageBox.Show("Please select a report type.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Retrieve DatePickers from ParametersPanel
            var startDatePicker = ParametersPanel.Children.OfType<DatePicker>()
                .FirstOrDefault(d => d.Name == "StartDatePicker");
            var endDatePicker = ParametersPanel.Children.OfType<DatePicker>()
                .FirstOrDefault(d => d.Name == "EndDatePicker");

            if (startDatePicker == null || endDatePicker == null)
            {
                MessageBox.Show("Date pickers not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!startDatePicker.SelectedDate.HasValue || !endDatePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("Please specify a valid date range.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var startDate = startDatePicker.SelectedDate.Value;
            var endDate = endDatePicker.SelectedDate.Value;

            try
            {
                switch (selectedReport)
                {
                    case "Individual Attendance":
                        var memberComboBox = this.FindName("MemberComboBox") as ComboBox;
                        var selectedMember = memberComboBox?.SelectedItem as Member;
                        if (selectedMember == null)
                        {
                            MessageBox.Show("Please select a member.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        var individualRecords = _context.Attendances
                            .Where(a => a.MemberId == selectedMember.Id && a.ServiceDate >= startDate && a.ServiceDate <= endDate)
                            .Select(a => new { a.ServiceDate, a.ServiceType, a.Status, a.Notes })
                            .OrderBy(a => a.ServiceDate)
                            .ToList();
                        ReportDataGrid.ItemsSource = individualRecords;
                        break;

                    case "Church Attendance Summary":
                        var summaries = _context.ChurchAttendanceSummaries
                            .Where(s => s.SummaryDate >= startDate && s.SummaryDate <= endDate)
                            .Select(s => new
                            {
                                s.SummaryDate,
                                s.ServiceType,
                                s.TotalPresent,
                                s.TotalMalePresent,
                                s.TotalFemalePresent,
                                s.Visitors,
                                s.Children,
                                s.OfferingAmount
                            })
                            .OrderBy(s => s.SummaryDate)
                            .ToList();
                        ReportDataGrid.ItemsSource = summaries;
                        break;

                    case "Service Type Report":
                        var serviceTypeComboBox = this.FindName("ServiceTypeComboBox") as ComboBox;
                        var selectedServiceType = serviceTypeComboBox?.SelectedItem as ServiceType?;
                        var serviceRecords = _context.ChurchAttendanceSummaries
                            .Where(s => s.SummaryDate >= startDate && s.SummaryDate <= endDate &&
                                        (!selectedServiceType.HasValue || s.ServiceType == selectedServiceType.Value))
                            .GroupBy(s => s.ServiceType)
                            .Select(g => new
                            {
                                ServiceType = g.Key,
                                TotalAttendance = g.Sum(s => s.TotalPresent),
                                AverageAttendance = g.Average(s => s.TotalPresent)
                            })
                            .ToList();
                        ReportDataGrid.ItemsSource = serviceRecords;
                        break;

                    case "Demographic Report":
                        var demographicRecords = _context.Attendances
                            .Where(a => a.ServiceDate >= startDate && a.ServiceDate <= endDate && a.Status == AttendanceStatus.Present)
                            .GroupBy(a => a.Member.Sex)
                            .Select(g => new
                            {
                                Gender = g.Key,
                                TotalPresent = g.Count()
                            })
                            .ToList();
                        ReportDataGrid.ItemsSource = demographicRecords;
                        break;

                    case "Offering Report":
                        var offeringRecords = _context.ChurchAttendanceSummaries
                            .Where(s => s.SummaryDate >= startDate && s.SummaryDate <= endDate)
                            .Select(s => new { s.SummaryDate, s.ServiceType, s.OfferingAmount })
                            .OrderBy(s => s.SummaryDate)
                            .ToList();
                        ReportDataGrid.ItemsSource = offeringRecords;
                        break;

                    case "Visitor and Newcomer Report":
                        var visitorRecords = _context.ChurchAttendanceSummaries
                            .Where(s => s.SummaryDate >= startDate && s.SummaryDate <= endDate)
                            .Select(s => new { s.SummaryDate, s.ServiceType, s.Visitors })
                            .OrderBy(s => s.SummaryDate)
                            .ToList();
                        ReportDataGrid.ItemsSource = visitorRecords;
                        break;

                    case "Absentee Report":
                        var thresholdTextBox = this.FindName("ThresholdTextBox") as TextBox;
                        if (!double.TryParse(thresholdTextBox?.Text, out double threshold) || threshold < 0 || threshold > 100)
                        {
                            MessageBox.Show("Please enter a valid threshold percentage (0-100).", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        var totalServices = _context.ChurchAttendanceSummaries
                            .Count(s => s.SummaryDate >= startDate && s.SummaryDate <= endDate);
                        var absenteeRecords = _context.Members
                            .Select(m => new
                            {
                                m.FullName,
                                AttendedCount = _context.Attendances.Count(a => a.MemberId == m.Id && a.ServiceDate >= startDate && a.ServiceDate <= endDate && a.Status == AttendanceStatus.Present),
                                TotalServices = totalServices
                            })
                            .Where(x => totalServices > 0 && ((double)x.AttendedCount / totalServices) * 100 < threshold)
                            .Select(x => new
                            {
                                x.FullName,
                                AttendancePercentage = totalServices > 0 ? ((double)x.AttendedCount / totalServices) * 100 : 0
                            })
                            .ToList();
                        ReportDataGrid.ItemsSource = absenteeRecords;
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating report: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}