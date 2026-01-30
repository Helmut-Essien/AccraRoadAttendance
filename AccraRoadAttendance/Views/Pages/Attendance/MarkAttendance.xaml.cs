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
using static AccraRoadAttendance.Models.Member;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using System.Reflection.Metadata;
using System.Windows.Threading;

namespace AccraRoadAttendance.Views.Pages.Attendance
{
    public partial class MarkAttendance : UserControl, INotifyPropertyChanged
    {
        private readonly AttendanceDbContext _context;
        private List<Models.Attendance> attendanceRecords;
        private List<Member> allMembers;
        private List<Member> displayedMembers;
        private int currentPage = 1;
        private int pageSize = 7;

        public MarkAttendance(AttendanceDbContext context)
        {
            InitializeComponent();
            _context = context;
            DataContext = this;
            ServiceDatePicker.SelectedDate = DateTime.Today;

            // Initialize OccupationType ComboBox
            var ServiceTypeItems = Enum.GetValues(typeof(ServiceType))
                .Cast<ServiceType>()
                .Select(ot => new {
                    Value = ot,
                    DisplayName = GetEnumDisplayName(ot)
                }).ToList();

            ServiceTypeComboBox.ItemsSource = ServiceTypeItems;
            ServiceTypeComboBox.DisplayMemberPath = "DisplayName";
            ServiceTypeComboBox.SelectedValuePath = "Value";

            // Subscribe to DataGrid events
            AttendanceDataGrid.Loaded += DataGrid_Loaded;
            AttendanceDataGrid.SizeChanged += DataGrid_SizeChanged;

            // Subscribe to UserControl Loaded event
            this.Loaded += MarkAttendance_Loaded;
            //LoadMembers();
        }

        private static string GetEnumDisplayName(Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = (DisplayAttribute)Attribute.GetCustomAttribute(
                field, typeof(DisplayAttribute));
            return attribute?.Name ?? value.ToString();
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private ServiceType? _serviceType;
        public ServiceType? serviceType
        {
            get => _serviceType;
            set
            {
                _serviceType = value;
                OnPropertyChanged(nameof(serviceType));
            }
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

        // Helper method to find the ScrollViewer in the DataGrid
        private ScrollViewer GetScrollViewer(DependencyObject depObj)
        {
            if (depObj is ScrollViewer scrollViewer)
                return scrollViewer;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);
                var result = GetScrollViewer(child);
                if (result != null)
                    return result;
            }
            return null;
        }

       

        private void CalculatePageSize()
        {
            var scrollViewer = GetScrollViewer(AttendanceDataGrid);
            if (scrollViewer != null)
            {
                double viewportHeight = scrollViewer.ActualHeight;
                double rowHeight = 0;

                if (AttendanceDataGrid.Items.Count > 0)
                {
                    var firstItem = AttendanceDataGrid.Items[0];
                    var row = (DataGridRow)AttendanceDataGrid.ItemContainerGenerator.ContainerFromItem(firstItem);
                    if (row != null)
                    {
                        rowHeight = row.ActualHeight;
                    }
                }

                if (rowHeight == 0)
                {
                    rowHeight = 30; // Default row height
                }

                if (viewportHeight > 0 && rowHeight > 0)
                {
                    int newPageSize = (int)(viewportHeight / rowHeight);
                    pageSize = Math.Max(1, newPageSize);
                }
                else
                {
                    pageSize = 1; // Reasonable default instead of 1
                }
            }
            else
            {
                pageSize = 1; // Default if ScrollViewer not found
            }
        }


        // Adjust CurrentPage to stay valid after pageSize changes
        private void AdjustCurrentPage()
        {
            if (allMembers == null) return;

            

            int totalPages = (int)Math.Ceiling((double)allMembers.Count / pageSize);
            if (totalPages == 0)
            {
                CurrentPage = 1; // No pages available
            }
            else
            {
                CurrentPage = Math.Max(1, Math.Min(CurrentPage, totalPages)); // Keep within bounds
            }
        }

        private void DataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            CalculatePageSize();
            if (allMembers != null)
            {
                AdjustCurrentPage();
                RefreshDataGrid();
                UpdatePagination();
            }
        }

        private void DataGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            CalculatePageSize();
            if (allMembers != null)
            {
                AdjustCurrentPage();
                RefreshDataGrid();
                UpdatePagination();
            }
        }
        private async void MarkAttendance_Loaded(object sender, RoutedEventArgs e)
        {
            
            await LoadMembersAsync(); // Load the members asynchronously
            RefreshDataGrid(); // Set the initial ItemsSource to trigger rendering

            // Schedule pagination calculation after UI rendering
            _ = Dispatcher.InvokeAsync(() =>
            {
                CalculatePageSize();    // Calculate page size based on rendered dimensions
                AdjustCurrentPage();    // Adjust the current page if necessary
                RefreshDataGrid();      // Refresh the DataGrid with the correct page
                UpdatePagination();     // Update pagination controls
            }, DispatcherPriority.Render);
        }


        private void RefreshDataGrid()
        {
            if (allMembers == null) return;

            displayedMembers = allMembers
                .Skip((CurrentPage - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Filter existing attendance records for the displayed members
            var displayedAttendanceRecords = attendanceRecords
                .Where(ar => displayedMembers.Any(dm => dm.Id == ar.MemberId))
                .ToList();

            AttendanceDataGrid.ItemsSource = displayedAttendanceRecords;
            UpdateTotals();
        }

        private void UpdatePagination()
        {
            if (allMembers == null) return;

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







        
        private async Task LoadMembersAsync()
        {
            try
            {
                allMembers = await _context.Members.ToListAsync();
                // Initialize attendance records for all active members
                attendanceRecords = allMembers.Where(m => m.IsActive).Select(m => new Models.Attendance
                {
                    MemberId = m.Id,
                    Member = m,
                    ServiceDate = DateTime.Today,
                    Status = AttendanceStatus.Absent,
                    RecordedAt = DateTime.UtcNow,
                    Notes = string.Empty
                }).ToList();
                CurrentPage = 1;
                //RefreshDataGrid();
                //UpdatePagination();
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

        //private async void SaveAttendance_Click(object sender, RoutedEventArgs e)
        //{
        //    if (ServiceTypeComboBox.SelectedItem == null)
        //    {
        //        MessageBox.Show("Please select a service type.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        //        return;
        //    }

        //    if (!ServiceDatePicker.SelectedDate.HasValue)
        //    {
        //        MessageBox.Show("Please select a service date.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        //        return;
        //    }

        //    var visitorsWindow = new VisitorsInputWindow();
        //    if (visitorsWindow.ShowDialog() == true)
        //    {
        //        try
        //        {
        //            var serviceDate = ServiceDatePicker.SelectedDate.Value;
        //            var serviceType = (ServiceType)ServiceTypeComboBox.SelectedValue; ;
        //            var serviceTheme = ServiceThemeTextBox.Text;

        //            using var transaction = await _context.Database.BeginTransactionAsync();
        //            try
        //            {
        //                // Check for existing attendance records
        //                var existingRecords = await _context.Attendances
        //                    .Where(a => a.ServiceDate.Date == serviceDate.Date &&
        //                                a.ServiceType == serviceType)
        //                    .ToListAsync();

        //                if (existingRecords.Any())
        //                {
        //                    var result = MessageBox.Show(
        //                        "Attendance records already exist for this service. Do you want to update them?",
        //                        "Attendance Exists",
        //                        MessageBoxButton.YesNo,
        //                        MessageBoxImage.Question);

        //                    if (result == MessageBoxResult.No)
        //                        return;

        //                    _context.Attendances.RemoveRange(existingRecords);
        //                }

        //                // Check for existing ChurchAttendanceSummary
        //                var existingSummary = await _context.ChurchAttendanceSummaries
        //                .FirstOrDefaultAsync(s => s.SummaryDate.Date == serviceDate.Date &&
        //                                      s.ServiceType == serviceType);

        //                // Update attendance records
        //                foreach (var record in attendanceRecords)
        //                {
        //                    record.ServiceDate = serviceDate;
        //                    record.ServiceType = serviceType;
        //                    record.RecordedAt = DateTime.UtcNow;
        //                    _context.Attendances.Add(record);
        //                }

        //                // Save ChurchAttendanceSummary
        //                var allMembers = await _context.Members.ToListAsync(); // Fetch all members
        //                if (existingSummary != null)
        //                {
        //                    // Update existing summary
        //                    existingSummary.TotalPresent = attendanceRecords.Count(r => r.Status == AttendanceStatus.Present);
        //                    existingSummary.TotalMalePresent = attendanceRecords.Count(r => r.Status == AttendanceStatus.Present && r.Member.Sex == Member.Gender.Male);
        //                    existingSummary.TotalFemalePresent = attendanceRecords.Count(r => r.Status == AttendanceStatus.Present && r.Member.Sex == Member.Gender.Female);
        //                    existingSummary.TotalMembers = allMembers.Count;
        //                    existingSummary.Visitors = visitorsWindow.Visitors;
        //                    existingSummary.Children = visitorsWindow.Children;
        //                    existingSummary.OfferingAmount = visitorsWindow.OfferingAmount;
        //                    existingSummary.ServiceTheme = serviceTheme;
        //                    existingSummary.SummaryLastModified = DateTime.UtcNow; // Update timestamp if needed
        //                }
        //                else
        //                {
        //                    // Create new summary
        //                    var summary = new ChurchAttendanceSummary
        //                    {
        //                        SummaryDate = serviceDate,
        //                        ServiceType = serviceType,
        //                        TotalPresent = attendanceRecords.Count(r => r.Status == AttendanceStatus.Present),
        //                        TotalMalePresent = attendanceRecords.Count(r => r.Status == AttendanceStatus.Present && r.Member.Sex == Member.Gender.Male),
        //                        TotalFemalePresent = attendanceRecords.Count(r => r.Status == AttendanceStatus.Present && r.Member.Sex == Member.Gender.Female),
        //                        TotalMembers = allMembers.Count,
        //                        Visitors = visitorsWindow.Visitors,
        //                        Children = visitorsWindow.Children,
        //                        OfferingAmount = visitorsWindow.OfferingAmount,
        //                        ServiceTheme = serviceTheme
        //                    };
        //                    _context.ChurchAttendanceSummaries.Add(summary);
        //                }
        //                await _context.SaveChangesAsync();
        //                await transaction.CommitAsync();
        //                MessageBox.Show("Attendance saved successfully!", "Success", MessageBoxButton.OK);
        //            }
        //            catch (Exception ex)
        //            {
        //                await transaction.RollbackAsync();
        //                MessageBox.Show($"An error occurred while saving attendance: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            MessageBox.Show($"An unexpected error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        //        }
        //    }
        //}


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
                    var serviceType = (ServiceType)ServiceTypeComboBox.SelectedValue;
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

                            // Update existing records instead of removing them
                            foreach (var record in attendanceRecords)
                            {
                                var existingRecord = existingRecords.FirstOrDefault(er => er.MemberId == record.MemberId);

                                if (existingRecord != null)
                                {
                                    // Update existing record
                                    existingRecord.Status = record.Status;
                                    existingRecord.Notes = record.Notes;
                                    existingRecord.RecordedAt = DateTime.UtcNow;
                                    _context.Attendances.Update(existingRecord);
                                }
                                else
                                {
                                    // Add new record for members not in existing records
                                    record.ServiceDate = serviceDate;
                                    record.ServiceType = serviceType;
                                    record.RecordedAt = DateTime.UtcNow;
                                    _context.Attendances.Add(record);
                                }
                            }

                            // Remove records for members that no longer exist or are inactive
                            var currentMemberIds = attendanceRecords.Select(ar => ar.MemberId).ToList();
                            var recordsToRemove = existingRecords
                                .Where(er => !currentMemberIds.Contains(er.MemberId))
                                .ToList();

                            if (recordsToRemove.Any())
                            {
                                _context.Attendances.RemoveRange(recordsToRemove);
                            }
                        }
                        else
                        {
                            // No existing records, add all new records
                            foreach (var record in attendanceRecords)
                            {
                                record.ServiceDate = serviceDate;
                                record.ServiceType = serviceType;
                                record.RecordedAt = DateTime.UtcNow;
                                _context.Attendances.Add(record);
                            }
                        }

                        // Check for existing ChurchAttendanceSummary
                        var existingSummary = await _context.ChurchAttendanceSummaries
                            .FirstOrDefaultAsync(s => s.SummaryDate.Date == serviceDate.Date &&
                                                  s.ServiceType == serviceType);

                        // Calculate totals
                        var allMembers = await _context.Members.ToListAsync();
                        int totalPresent = attendanceRecords.Count(r => r.Status == AttendanceStatus.Present);
                        int totalMalePresent = attendanceRecords.Count(r => r.Status == AttendanceStatus.Present && r.Member.Sex == Member.Gender.Male);
                        int totalFemalePresent = attendanceRecords.Count(r => r.Status == AttendanceStatus.Present && r.Member.Sex == Member.Gender.Female);

                        if (existingSummary != null)
                        {
                            // Update existing summary
                            existingSummary.TotalPresent = totalPresent;
                            existingSummary.TotalMalePresent = totalMalePresent;
                            existingSummary.TotalFemalePresent = totalFemalePresent;
                            existingSummary.TotalMembers = allMembers.Count;
                            existingSummary.Visitors = visitorsWindow.Visitors;
                            existingSummary.Children = visitorsWindow.Children;
                            existingSummary.OfferingAmount = visitorsWindow.OfferingAmount;
                            existingSummary.ServiceTheme = serviceTheme;
                            existingSummary.SummaryLastModified = DateTime.UtcNow;

                            _context.ChurchAttendanceSummaries.Update(existingSummary);
                        }
                        else
                        {
                            // Create new summary
                            var summary = new ChurchAttendanceSummary
                            {
                                SummaryDate = serviceDate,
                                ServiceType = serviceType,
                                TotalPresent = totalPresent,
                                TotalMalePresent = totalMalePresent,
                                TotalFemalePresent = totalFemalePresent,
                                TotalMembers = allMembers.Count,
                                Visitors = visitorsWindow.Visitors,
                                Children = visitorsWindow.Children,
                                OfferingAmount = visitorsWindow.OfferingAmount,
                                ServiceTheme = serviceTheme
                            };
                            _context.ChurchAttendanceSummaries.Add(summary);
                        }

                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        MessageBox.Show("Attendance saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
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