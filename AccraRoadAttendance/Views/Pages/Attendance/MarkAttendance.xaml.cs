using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using AccraRoadAttendance.Data;
using AccraRoadAttendance.Models;
using AccraRoadAttendance.Views.Pages.Members;

namespace AccraRoadAttendance.Views.Pages.Attendance
{
    public partial class MarkAttendance : UserControl
    {
        private readonly AttendanceDbContext _context;
        private List<Models.Attendance> attendanceRecords;

        public MarkAttendance(AttendanceDbContext context)
        {
            InitializeComponent();
            _context = context;
            ServiceDatePicker.SelectedDate = DateTime.Today;
            ServiceTypeComboBox.ItemsSource = Enum.GetValues(typeof(ServiceType));
            LoadMembers();
        }

        private List<Member> allMembers;

        private async void LoadMembers()
        {
            try
            {
                allMembers = await _context.Members.ToListAsync();
                // Initialize attendance records for active members
                attendanceRecords = allMembers.Where(m => m.IsActive).Select(m => new Models.Attendance
                {
                    MemberId = m.Id,
                    Member = m,
                    ServiceDate = DateTime.Today,
                    Status = AttendanceStatus.Absent,
                    RecordedAt = DateTime.UtcNow,
                    Notes = string.Empty
                }).ToList();

                AttendanceDataGrid.ItemsSource = attendanceRecords;
                UpdateTotals();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading members: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateTotals()
        {
            int totalPresent = attendanceRecords.Count(r => r.Status == AttendanceStatus.Present);
            int totalMalePresent = attendanceRecords.Count(r => r.Status == AttendanceStatus.Present && r.Member.Sex == Member.Gender.Male);
            int totalFemalePresent = attendanceRecords.Count(r => r.Status == AttendanceStatus.Present && r.Member.Sex == Member.Gender.Female);

            // Update the UI with totals
            TotalPresentText.Text = totalPresent.ToString();
            TotalMalePresentText.Text = totalMalePresent.ToString();
            TotalFemalePresentText.Text = totalFemalePresent.ToString();

            // Calculate TotalMembers (not displayed in the UI)
            int totalMembers = allMembers.Count; // Total members in the database (active + inactive)

            // Store TotalMembers in a variable for later use
            // It will be saved to the ChurchAttendanceSummary table
        }

        private void ServiceTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool isValid = ServiceTypeComboBox.SelectedItem != null && ServiceDatePicker.SelectedDate.HasValue;
            AttendanceDataGrid.IsEnabled = isValid;
            ServiceThemeTextBox.IsEnabled = isValid;
        }

        private void ComboBox_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            // Prevent ComboBox from processing scroll events
            e.Handled = true;

            // Forward the scroll event to the parent ScrollViewer
            if (sender is ComboBox comboBox && comboBox.Parent is UIElement parent)
            {
                var scrollEvent = new System.Windows.Input.MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
                {
                    RoutedEvent = UIElement.MouseWheelEvent,
                    Source = sender
                };
                parent.RaiseEvent(scrollEvent);
            }
        }

        private void AttendanceDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            // Update totals when a cell edit ends
            var allMembers = _context.Members.ToList(); // Fetch all members again
            UpdateTotals();
        }
        private void StatusComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Ensure the ComboBox is bound to an item in the DataGrid
            if (sender is ComboBox comboBox && comboBox.DataContext is Models.Attendance attendanceRecord)
            {
                // Update the totals whenever the Status changes
                
                var allMembers = _context.Members.ToList(); // Fetch all members again
                UpdateTotals();
            }
        }

        private async void SaveAttendance_Click(object sender, RoutedEventArgs e)
        {
            if (ServiceTypeComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a service type.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!ServiceDatePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("Please select a service date.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var visitorsWindow = new VisitorsInputWindow();
            if (visitorsWindow.ShowDialog() == true)
            {
                try
                {
                    var serviceDate = ServiceDatePicker.SelectedDate.Value;
                    var serviceType = (ServiceType)ServiceTypeComboBox.SelectedItem;
                    var serviceTheme = ServiceThemeTextBox.Text;

                    using var transaction = await _context.Database.BeginTransactionAsync();
                    try
                    {
                        // Check for existing attendance records
                        var existingRecords = await _context.Attendances
                            .Where(a => a.ServiceDate.Date == serviceDate.Date &&
                                        a.ServiceType == serviceType)
                            .ToListAsync();

                        if (existingRecords.Any())
                        {
                            var result = MessageBox.Show(
                                "Attendance records already exist for this service. Do you want to update them?",
                                "Attendance Exists",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Question);

                            if (result == MessageBoxResult.No)
                                return;

                            _context.Attendances.RemoveRange(existingRecords);
                        }

                        // Update attendance records
                        foreach (var record in attendanceRecords)
                        {
                            record.ServiceDate = serviceDate;
                            record.ServiceType = serviceType;
                            record.RecordedAt = DateTime.UtcNow;
                            _context.Attendances.Add(record);
                        }

                        // Save ChurchAttendanceSummary
                        var allMembers = await _context.Members.ToListAsync(); // Fetch all members
                        var summary = new ChurchAttendanceSummary
                        {
                            SummaryDate = serviceDate,
                            ServiceType = serviceType,
                            TotalPresent = attendanceRecords.Count(r => r.Status == AttendanceStatus.Present),
                            TotalMalePresent = attendanceRecords.Count(r => r.Status == AttendanceStatus.Present && r.Member.Sex == Member.Gender.Male),
                            TotalFemalePresent = attendanceRecords.Count(r => r.Status == AttendanceStatus.Present && r.Member.Sex == Member.Gender.Female),
                            TotalMembers = allMembers.Count, // Total members in the database (active + inactive)
                            Visitors = visitorsWindow.Visitors,
                            Children = visitorsWindow.Children,
                            OfferingAmount = visitorsWindow.OfferingAmount,
                            ServiceTheme = serviceTheme
                        };

                        _context.ChurchAttendanceSummaries.Add(summary);
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();
                        MessageBox.Show("Attendance saved successfully!", "Success", MessageBoxButton.OK);
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        MessageBox.Show($"An error occurred while saving attendance: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An unexpected error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}