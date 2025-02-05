using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using AccraRoadAttendance.Models;
using AccraRoadAttendance.Data;

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

            // Initialize ComboBox items
            ServiceTypeComboBox.ItemsSource = Enum.GetValues(typeof(ServiceType));

            LoadMembers();
        }

        private async void LoadMembers()
        {
            try
            {
                var members = await _context.Members
                    .Where(m => m.IsActive)  // Only load active members
                    .ToListAsync();

                // Initialize attendance records for all members
                attendanceRecords = members.Select(m => new Models.Attendance
                {
                    MemberId = m.Id,
                    Member = m,
                    ServiceDate = DateTime.Today,
                    Status = AttendanceStatus.Absent,  // Default to absent
                    RecordedAt = DateTime.UtcNow
                }).ToList();

                AttendanceDataGrid.ItemsSource = attendanceRecords;
                UpdateAttendanceSummary();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading members: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = SearchBox.Text.ToLower();
            var filteredRecords = attendanceRecords.Where(r =>
                r.Member.FirstName.ToLower().Contains(searchText) ||
                r.Member.LastName.ToLower().Contains(searchText)
            ).ToList();

            AttendanceDataGrid.ItemsSource = filteredRecords;
        }

        private void UpdateAttendanceSummary()
        {
            var presentRecords = attendanceRecords.Where(r => r.Status == AttendanceStatus.Present).ToList();
            TotalPresentText.Text = presentRecords.Count.ToString();
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

            try
            {
                var serviceDate = ServiceDatePicker.SelectedDate.Value;
                var serviceType = (ServiceType)ServiceTypeComboBox.SelectedItem;

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