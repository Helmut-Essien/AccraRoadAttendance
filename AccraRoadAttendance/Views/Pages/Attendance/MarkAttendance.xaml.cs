using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using AccraRoadAttendance.Data;
using AccraRoadAttendance.Models;
using AccraRoadAttendance.Views.Pages.Members;
using System.ComponentModel;

namespace AccraRoadAttendance.Views.Pages.Attendance
{
    public partial class MarkAttendance : UserControl, INotifyPropertyChanged
    {
        private readonly AttendanceDbContext _context;
        private List<Models.Attendance> attendanceRecords;
        private List<Member> allMembers;
        private List<Member> displayedMembers;
        private int currentPage = 1;
        private const int pageSize = 1;

        public MarkAttendance(AttendanceDbContext context)
        {
            InitializeComponent();
            _context = context;
            DataContext = this;
            ServiceDatePicker.SelectedDate = DateTime.Today;
            ServiceTypeComboBox.ItemsSource = Enum.GetValues(typeof(ServiceType));
            LoadMembers();
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool _isPaginationVisible;
        public bool IsPaginationVisible
        {
            get => _isPaginationVisible;
            set
            {
                _isPaginationVisible = value;
                OnPropertyChanged(nameof(IsPaginationVisible));
            }
        }


        private List<int> _pageNumbers;
        public List<int> PageNumbers
        {
            get => _pageNumbers;
            set
            {
                _pageNumbers = value;
                OnPropertyChanged(nameof(PageNumbers));
            }
        }

        private int _lastPageNumber;
        public int LastPageNumber
        {
            get => _lastPageNumber;
            set
            {
                _lastPageNumber = value;
                OnPropertyChanged(nameof(LastPageNumber));
            }
        }

        private int _currentPage = 1;
        public int CurrentPage
        {
            get => _currentPage;
            set
            {
                _currentPage = value;
                OnPropertyChanged(nameof(CurrentPage));
            }
        }
        private void RefreshDataGrid()
        {
            int totalPages = (int)Math.Ceiling((double)allMembers.Count / pageSize);
            displayedMembers = allMembers
                .Skip((CurrentPage - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Initialize attendance records for displayed members
            attendanceRecords = displayedMembers.Where(m => m.IsActive).Select(m => new Models.Attendance
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

        private void UpdatePagination()
        {
            int totalPages = (int)Math.Ceiling((double)allMembers.Count / pageSize);
            IsPaginationVisible = totalPages > 1;

            if (IsPaginationVisible)
            {
                int maxPagesToShow = 4;
                int startPage = 1;
                int endPage;

                if (totalPages > maxPagesToShow)
                {
                    // Logic to determine visible pages when there are many pages
                    if (CurrentPage <= maxPagesToShow / 2)
                    {
                        startPage = 1;
                        endPage = maxPagesToShow;
                    }
                    else if (CurrentPage > totalPages - maxPagesToShow / 2)
                    {
                        startPage = totalPages - maxPagesToShow + 1;
                        endPage = totalPages;
                    }
                    else
                    {
                        startPage = CurrentPage - maxPagesToShow / 2;
                        endPage = CurrentPage + maxPagesToShow / 2 - 1;
                    }

                    // Ensure endPage doesn't overlap with the last page button
                    if (endPage >= totalPages)
                    {
                        endPage = totalPages - 1;
                    }
                }
                else
                {
                    // Show pages 1 to (totalPages - 1) to avoid duplication with the last page button
                    endPage = totalPages - 1;
                }

                List<int> pages = new List<int>();

                // Add visible page numbers
                for (int i = startPage; i <= endPage; i++)
                {
                    pages.Add(i);
                }

                // Add ellipsis if there's a gap between the visible pages and the last page
                if (endPage < totalPages - 1)
                {
                    pages.Add(-1); // -1 represents the ellipsis
                }

                PageNumbers = pages;
                LastPageNumber = totalPages; // Last page is handled by the dedicated button
            }
        }

        private void PreviousPage_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
                RefreshDataGrid();
                UpdatePagination();
            }
        }

        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            int totalPages = (int)Math.Ceiling((double)allMembers.Count / pageSize);
            if (CurrentPage < totalPages)
            {
                CurrentPage++;
                RefreshDataGrid();
                UpdatePagination();
            }
        }

        private void PageNumberButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && int.TryParse(btn.Content.ToString(), out int pageNumber))
            {
                if (pageNumber != -1) // -1 represents the ellipsis, not clickable
                {
                    CurrentPage = pageNumber;
                    RefreshDataGrid();
                    UpdatePagination();
                }
            }
        }

        public void ReceiveParameter(object parameter)
        {
            throw new NotImplementedException();
        }







        //private async void LoadMembers()
        //{
        //    try
        //    {
        //        allMembers = await _context.Members.ToListAsync();
        //        // Initialize attendance records for active members
        //        attendanceRecords = allMembers.Where(m => m.IsActive).Select(m => new Models.Attendance
        //        {
        //            MemberId = m.Id,
        //            Member = m,
        //            ServiceDate = DateTime.Today,
        //            Status = AttendanceStatus.Absent,
        //            RecordedAt = DateTime.UtcNow,
        //            Notes = string.Empty
        //        }).ToList();

        //        AttendanceDataGrid.ItemsSource = attendanceRecords;
        //        UpdateTotals();
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"Error loading members: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        //    }
        //}

        private async void LoadMembers()
        {
            try
            {
                allMembers = await _context.Members.ToListAsync();
                CurrentPage = 1;
                RefreshDataGrid();
                UpdatePagination();
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