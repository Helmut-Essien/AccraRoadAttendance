using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using AccraRoadAttendance.Data;
using AccraRoadAttendance.Models;
using MaterialDesignThemes.Wpf;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;

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

        //private void ReportTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    if (ReportTypeComboBox.SelectedItem == null) return;

        //    var selectedReport = (ReportTypeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
        //    ParametersPanel.Children.Clear();

        //    // Create a uniform margin for controls
        //    Thickness uniformMargin = new Thickness(10, 5, 10, 5);

        //    // Common parameters with improved styling
        //    var startDateLabel = new TextBlock
        //    {
        //        Text = "Start Date:",
        //        Margin = uniformMargin,
        //        FontSize = 14,
        //        FontWeight = FontWeights.Medium
        //    };
        //    var startDatePicker = new DatePicker
        //    {
        //        Name = "StartDatePicker",
        //        SelectedDate = DateTime.Today.AddMonths(-1),
        //        Margin = uniformMargin,
        //        Style = (Style)Application.Current.FindResource("MaterialDesignOutlinedDatePicker")
        //    };
        //    HintAssist.SetHint(startDatePicker, "Start Date");

        //    var endDateLabel = new TextBlock
        //    {
        //        Text = "End Date:",
        //        Margin = uniformMargin,
        //        FontSize = 14,
        //        FontWeight = FontWeights.Medium
        //    };
        //    var endDatePicker = new DatePicker
        //    {
        //        Name = "EndDatePicker",
        //        SelectedDate = DateTime.Today,
        //        Margin = uniformMargin,
        //        Style = (Style)Application.Current.FindResource("MaterialDesignDatePicker")
        //    };
        //    HintAssist.SetHint(endDatePicker, "End Date");

        //    switch (selectedReport)
        //    {
        //        case "Individual Attendance":
        //            // Label for member selection
        //            var memberLabel = new TextBlock
        //            {
        //                Text = "Select Member:",
        //                Margin = uniformMargin,
        //                FontSize = 14,
        //                FontWeight = FontWeights.Medium
        //            };
        //            // Create the ComboBox with search-enabled behavior
        //            var memberComboBox = new ComboBox
        //            {
        //                Name = "MemberComboBox",
        //                Margin = uniformMargin,
        //                DisplayMemberPath = "FullName",
        //                IsEditable = true,           // Allow text entry
        //                IsTextSearchEnabled = false, // We'll handle filtering manually
        //                StaysOpenOnEdit = true,      // Keeps dropdown open during typing
        //                Style = (Style)Application.Current.FindResource("MaterialDesignOutlinedComboBox")
        //            };
        //            HintAssist.SetHint(memberComboBox, "Select Member");

        //            // Retrieve members and set up filtering using a CollectionView
        //            var members = _context.Members.ToList();
        //            var memberView = CollectionViewSource.GetDefaultView(members);
        //            memberComboBox.ItemsSource = memberView;

        //            // Filter the CollectionView as the user types
        //            memberComboBox.AddHandler(TextBoxBase.TextChangedEvent, new TextChangedEventHandler((s, args) =>
        //            {
        //                string filter = memberComboBox.Text;
        //                if (string.IsNullOrWhiteSpace(filter))
        //                {
        //                    memberView.Filter = null;
        //                }
        //                else
        //                {
        //                    memberView.Filter = o =>
        //                    {
        //                        var member = o as Member;
        //                        return member.FullName.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0;
        //                    };
        //                }
        //                memberView.Refresh();
        //            }));

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
        //                var serviceTypeLabel = new TextBlock
        //                {
        //                    Text = "Service Type:",
        //                    Margin = new Thickness(20, 0, 10, 0),
        //                    FontSize = 14,
        //                    FontWeight = FontWeights.Medium
        //                };
        //                var serviceTypeComboBox = new ComboBox
        //                {
        //                    Name = "ServiceTypeComboBox",
        //                    ItemsSource = Enum.GetValues(typeof(ServiceType))
        //                };
        //                serviceTypeComboBox.Style = (Style)Application.Current.FindResource("MaterialDesignOutlinedComboBox");
        //                HintAssist.SetHint(serviceTypeComboBox, "Select Service Type");

        //                ParametersPanel.Children.Add(serviceTypeLabel);
        //                ParametersPanel.Children.Add(serviceTypeComboBox);
        //            }
        //            break;

        //        case "Absentee Report":
        //            var thresholdLabel = new TextBlock
        //            {
        //                Text = "Absent Threshold (%):",
        //                Margin = new Thickness(0, 0, 10, 0),
        //                FontSize = 14,
        //                FontWeight = FontWeights.Medium
        //            };
        //            var thresholdTextBox = new TextBox
        //            {
        //                Name = "ThresholdTextBox",
        //                Text = "50",
        //                Width = 50
        //            };
        //            thresholdTextBox.Style = (Style)Application.Current.FindResource("MaterialDesignTextBox");
        //            HintAssist.SetHint(thresholdTextBox, "Absent Threshold (%)");

        //            ParametersPanel.Children.Add(thresholdLabel);
        //            ParametersPanel.Children.Add(thresholdTextBox);
        //            ParametersPanel.Children.Add(startDateLabel);
        //            ParametersPanel.Children.Add(startDatePicker);
        //            ParametersPanel.Children.Add(endDateLabel);
        //            ParametersPanel.Children.Add(endDatePicker);
        //            break;
        //    }
        //}

        private DatePicker _startDatePicker;
        private DatePicker _endDatePicker;


        private void ReportTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ReportTypeComboBox.SelectedItem == null) return;

            var selectedReport = (ReportTypeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            ParametersPanel.Children.Clear();

            // We'll use a Grid for better alignment
            var parametersGrid = new Grid
            {
                Margin = new Thickness(16) // outer margin around the entire grid
            };

            // Define columns (you can adjust widths as needed)
            parametersGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            parametersGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            parametersGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Common date labels/pickers
            var startDateLabel = new TextBlock
            {
                Text = "Start Date:",
                Margin = new Thickness(0, 0, 0, 4),
                FontSize = 14,
                FontWeight = FontWeights.Medium
            };
            _startDatePicker = new DatePicker
            {
                Name = "StartDatePicker",
                SelectedDate = DateTime.Today.AddMonths(-1),
                Margin = new Thickness(0, 0, 20, 0),
                // Swap to a different MD style here if you like:
                Style = (Style)Application.Current.FindResource("MaterialDesignOutlinedDatePicker")
            };
            HintAssist.SetHint(_startDatePicker, "Enter Date");

            var endDateLabel = new TextBlock
            {
                Text = "End Date:",
                Margin = new Thickness(0, 0, 0, 4),
                FontSize = 14,
                FontWeight = FontWeights.Medium
            };
            _endDatePicker = new DatePicker
            {
                Name = "EndDatePicker",
                SelectedDate = DateTime.Today,
                Margin = new Thickness(0, 0, 20, 0),
                // Swap to a different MD style here if you like:
                Style = (Style)Application.Current.FindResource("MaterialDesignOutlinedDatePicker")
            };
            HintAssist.SetHint(_endDatePicker, "Enter Date");

            switch (selectedReport)
            {
                case "Individual Attendance":
                    // Column 0: Member label + combo
                    var memberStack = new StackPanel { Margin = new Thickness(0, 0, 50, 0) };

                    var memberLabel = new TextBlock
                    {
                        Text = "Select Member:",
                        Margin = new Thickness(0, 0, 0, 4),
                        FontSize = 14,
                        FontWeight = FontWeights.Medium
                    };

                    // The search-enabled ComboBox
                    var memberComboBox = new ComboBox
                    {
                        Name = "MemberComboBox",
                        IsEditable = true,
                        IsTextSearchEnabled = false,
                        StaysOpenOnEdit = true,
                        // Increase the width so full names are visible
                        Width = 250,
                        Style = (Style)Application.Current.FindResource("MaterialDesignOutlinedComboBox")
                    };
                    HintAssist.SetHint(memberComboBox, "Select Member");

                    // Retrieve and filter members as before
                    var members = _context.Members.ToList();
                    var memberView = CollectionViewSource.GetDefaultView(members);
                    memberComboBox.ItemsSource = memberView;
                    memberComboBox.DisplayMemberPath = "FullName";

                    // Add text-changed handler for filtering
                    memberComboBox.AddHandler(TextBoxBase.TextChangedEvent, new TextChangedEventHandler((s2, e2) =>
                    {
                        string filter = memberComboBox.Text;
                        if (string.IsNullOrWhiteSpace(filter))
                        {
                            memberView.Filter = null;
                        }
                        else
                        {
                            memberView.Filter = o =>
                            {
                                var m = o as Member;
                                return m.FullName.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0;
                            };
                        }
                        memberView.Refresh();
                    }));

                    memberStack.Children.Add(memberLabel);
                    memberStack.Children.Add(memberComboBox);

                    // Place this stack in column 0
                    Grid.SetColumn(memberStack, 0);
                    parametersGrid.Children.Add(memberStack);

                    // Column 1: Start Date
                    var startStack = new StackPanel();
                    startStack.Children.Add(startDateLabel);
                    startStack.Children.Add(_startDatePicker);
                    Grid.SetColumn(startStack, 1);
                    parametersGrid.Children.Add(startStack);

                    // Column 2: End Date
                    var endStack = new StackPanel();
                    endStack.Children.Add(endDateLabel);
                    endStack.Children.Add(_endDatePicker);
                    Grid.SetColumn(endStack, 2);
                    parametersGrid.Children.Add(endStack);

                    break;

                case "Church Attendance Summary":
                case "Service Type Report":
                case "Demographic Report":
                case "Offering Report":
                case "Visitor and Newcomer Report":
                    // We only need Start/End Date columns for these
                    var startStack2 = new StackPanel { Margin = new Thickness(0, 0, 50, 0) };
                    startStack2.Children.Add(startDateLabel);
                    startStack2.Children.Add(_startDatePicker);
                    Grid.SetColumn(startStack2, 0);
                    parametersGrid.Children.Add(startStack2);

                    var endStack2 = new StackPanel();
                    endStack2.Children.Add(endDateLabel);
                    endStack2.Children.Add(_endDatePicker);
                    Grid.SetColumn(endStack2, 1);
                    parametersGrid.Children.Add(endStack2);

                    if (selectedReport == "Service Type Report")
                    {
                        // Additional service type selection in column 2
                        var serviceTypeStack = new StackPanel();
                        var serviceTypeLabel = new TextBlock
                        {
                            Text = "Service Type:",
                            Margin = new Thickness(0, 0, 0, 4),
                            FontSize = 14,
                            FontWeight = FontWeights.Medium
                        };
                        var serviceTypeComboBox = new ComboBox
                        {
                            Name = "ServiceTypeComboBox",
                            Style = (Style)Application.Current.FindResource("MaterialDesignOutlinedComboBox"),
                            ItemsSource = Enum.GetValues(typeof(ServiceType)),
                            Width = 200
                        };
                        HintAssist.SetHint(serviceTypeComboBox, "Select Service Type");
                        serviceTypeStack.Children.Add(serviceTypeLabel);
                        serviceTypeStack.Children.Add(serviceTypeComboBox);
                        Grid.SetColumn(serviceTypeStack, 2);
                        parametersGrid.Children.Add(serviceTypeStack);
                    }
                    break;

                case "Absentee Report":
                    // Column 0: Threshold
                    var thresholdStack = new StackPanel { Margin = new Thickness(0, 0, 50, 0) };
                    var thresholdLabel = new TextBlock
                    {
                        Text = "Absent Threshold (%):",
                        Margin = new Thickness(0, 0, 0, 4),
                        FontSize = 14,
                        FontWeight = FontWeights.Medium
                    };
                    var thresholdTextBox = new TextBox
                    {
                        Name = "ThresholdTextBox",
                        Text = "50",
                        Width = 60,
                        Style = (Style)Application.Current.FindResource("MaterialDesignTextBox")
                    };
                    HintAssist.SetHint(thresholdTextBox, "Absent Threshold (%)");

                    thresholdStack.Children.Add(thresholdLabel);
                    thresholdStack.Children.Add(thresholdTextBox);
                    Grid.SetColumn(thresholdStack, 0);
                    parametersGrid.Children.Add(thresholdStack);

                    // Column 1: Start date
                    var startStack3 = new StackPanel { Margin = new Thickness(0, 0, 50, 0) };
                    startStack3.Children.Add(startDateLabel);
                    startStack3.Children.Add(_startDatePicker);
                    Grid.SetColumn(startStack3, 1);
                    parametersGrid.Children.Add(startStack3);

                    // Column 2: End date
                    var endStack3 = new StackPanel();
                    endStack3.Children.Add(endDateLabel);
                    endStack3.Children.Add(_endDatePicker);
                    Grid.SetColumn(endStack3, 2);
                    parametersGrid.Children.Add(endStack3);
                    break;
            }

            // Finally, add the parametersGrid to your panel
            ParametersPanel.Children.Add(parametersGrid);
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

        private Member _selectedMemberForReport;

        private void GenerateReport_Click(object sender, RoutedEventArgs e)
        {
            // Use the stored DatePicker references.
            if (_startDatePicker == null || _endDatePicker == null)
            {
                MessageBox.Show("Date pickers not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!_startDatePicker.SelectedDate.HasValue || !_endDatePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("Please specify a valid date range.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var startDate = _startDatePicker.SelectedDate.Value;
            var endDate = _endDatePicker.SelectedDate.Value;

            var selectedReport = (ReportTypeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            if (string.IsNullOrEmpty(selectedReport))
            {
                MessageBox.Show("Please select a report type.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                switch (selectedReport)
                {
                    case "Individual Attendance":
                        // Retrieve the Member ComboBox from the grid.
                        var memberComboBox = ParametersPanel.Children
                            .OfType<Grid>()
                            .SelectMany(g => g.Children.OfType<StackPanel>())
                            .SelectMany(sp => sp.Children.OfType<ComboBox>())
                            .FirstOrDefault(c => c.Name == "MemberComboBox");

                        if (memberComboBox == null || memberComboBox.SelectedItem == null)
                        {
                            MessageBox.Show("Please select a member.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        var selectedMember = memberComboBox.SelectedItem as Member;
                        _selectedMemberForReport = selectedMember;
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
                        var serviceTypeComboBox = ParametersPanel.Children
                            .OfType<Grid>()
                            .SelectMany(g => g.Children.OfType<StackPanel>())
                            .SelectMany(sp => sp.Children.OfType<ComboBox>())
                            .FirstOrDefault(c => c.Name == "ServiceTypeComboBox");
                        var selectedServiceType = serviceTypeComboBox?.SelectedItem as ServiceType?;
                        if (!selectedServiceType.HasValue)
                        {
                            MessageBox.Show("Please select a service type.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        var serviceRecords = _context.ChurchAttendanceSummaries
                            .Where(s => s.SummaryDate >= startDate && s.SummaryDate <= endDate &&
                                        s.ServiceType == selectedServiceType.Value)
                            .Select(s => new
                            {
                                s.SummaryDate,
                                s.ServiceType,
                                MalePresent = s.TotalMalePresent,
                                FemalePresent = s.TotalFemalePresent,
                                s.Children,
                                s.Visitors,
                                TotalPresent = s.TotalMalePresent + s.TotalFemalePresent + s.Children
                            })
                            .OrderBy(s => s.SummaryDate)
                            .ToList();

                        if (serviceRecords.Count == 0)
                        {
                            MessageBox.Show("No records found for the selected service type and date range.",
                                "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
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
                        var thresholdTextBox = ParametersPanel.Children
                            .OfType<Grid>()
                            .SelectMany(g => g.Children.OfType<StackPanel>())
                            .SelectMany(sp => sp.Children.OfType<TextBox>())
                            .FirstOrDefault(t => t.Name == "ThresholdTextBox");
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
                                AttendedCount = _context.Attendances.Count(a =>
                                    a.MemberId == m.Id &&
                                    a.ServiceDate >= startDate &&
                                    a.ServiceDate <= endDate &&
                                    a.Status == AttendanceStatus.Present),
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
        private void PrintToPdf_Click(object sender, RoutedEventArgs e)
        {
            // Check if there is data to print
            if (ReportDataGrid.ItemsSource == null)
            {
                MessageBox.Show("No report data to print. Please generate a report first.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedReport = (ReportTypeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            if (string.IsNullOrEmpty(selectedReport))
            {
                MessageBox.Show("Please select a report type.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var startDate = _startDatePicker.SelectedDate.Value;
            var endDate = _endDatePicker.SelectedDate.Value;
            var reportData = ReportDataGrid.ItemsSource.Cast<object>().ToList();

            string memberName = null;
            if (selectedReport == "Individual Attendance")
            {
                memberName = _selectedMemberForReport?.FullName;
            }


            // Open a save file dialog for the user to choose the PDF location
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "PDF files (*.pdf)|*.pdf",
                FileName = $"{selectedReport.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd}.pdf"
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                var generator = new ReportGenerator();
                generator.GenerateReport(selectedReport, reportData, startDate, endDate, saveFileDialog.FileName, memberName);
                MessageBox.Show($"Report saved to {saveFileDialog.FileName}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }

}