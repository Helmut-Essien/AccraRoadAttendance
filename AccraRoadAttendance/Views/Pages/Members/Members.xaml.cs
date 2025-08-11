using AccraRoadAttendance.Data;
using AccraRoadAttendance.Models;
using AccraRoadAttendance.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace AccraRoadAttendance.Views.Pages.Members
{
    public partial class Members : UserControl, INotifyPropertyChanged, IParameterReceiver
    {
        private readonly AttendanceDbContext _context;
        private readonly INavigationService _navigationService;
        private List<Member> allMembers; // All members in the system
        private List<Member> displayedMembers; // Members displayed on the current page
        private int currentPage = 1;
        private int pageSize = 2;

        public Members(AttendanceDbContext context, INavigationService navigationService)
        {
            InitializeComponent();
            _context = context;
            _navigationService = navigationService;
            DataContext = this;
            IsPaginationVisible = false;
            //LoadMembers();

            // Subscribe to DataGrid events
            membersDataGrid.Loaded += DataGrid_Loaded;
            membersDataGrid.SizeChanged += DataGrid_SizeChanged;

            // Subscribe to UserControl Loaded event
            this.Loaded += Members_Loaded;
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

        // Calculate the pageSize based on DataGrid's available space
        private void CalculatePageSize()
        {
            var scrollViewer = GetScrollViewer(membersDataGrid);
            if (scrollViewer != null)
            {
                double viewportHeight = scrollViewer.ActualHeight;
                double rowHeight = 0;

                if (membersDataGrid.Items.Count > 0)
                {
                    var firstItem = membersDataGrid.Items[0];
                    var row = (DataGridRow)membersDataGrid.ItemContainerGenerator.ContainerFromItem(firstItem);
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
                    pageSize = 1; // Reasonable default
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

        private async void Members_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadMembersAsync();
            RefreshDataGrid(); // Set initial ItemsSource to trigger rendering

            // Schedule pagination calculation after UI rendering
            _ = Dispatcher.InvokeAsync(() =>
            {
                CalculatePageSize();    // Calculate page size based on rendered dimensions
                AdjustCurrentPage();    // Adjust the current page if necessary
                RefreshDataGrid();      // Refresh the DataGrid with the correct page
                UpdatePagination();     // Update pagination controls
            }, DispatcherPriority.Render);
        }

        //private async void LoadMembers()
        //{
        //    try
        //    {
        //        allMembers = await _context.Members.ToListAsync();
        //        CurrentPage = 1;
        //        RefreshDataGrid();
        //        UpdatePagination();
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"An error occurred while loading members: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        //    }
        //}
        private async Task LoadMembersAsync()
        {
            try
            {
                allMembers = await _context.Members.ToListAsync();
                CurrentPage = 1;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while loading members: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void RefreshDataGrid()
        {
            //int totalPages = (int)Math.Ceiling((double)allMembers.Count / pageSize);
            if (allMembers == null) return;

            displayedMembers = allMembers
                .Skip((CurrentPage - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            membersDataGrid.ItemsSource = displayedMembers;
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



        private void AddMember_Click(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show("Add Member clicked!");
            _navigationService.NavigateTo<AddMembers>();
        }

        private void EditMember_Click(object sender, RoutedEventArgs e)
        {
            var member = (sender as Button)?.CommandParameter as Member;
            if (member != null)
            {
                _navigationService.NavigateTo<EditMembers>(member);
            }
            else
            {
                MessageBox.Show("No member selected for editing.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MemberDetails_Click(object sender, RoutedEventArgs e)
        {
            var member = (sender as Button)?.CommandParameter as Member;
            if (member != null)
            {
                _navigationService.NavigateTo<MemberDetails>(member);
            }
            else
            {
                MessageBox.Show("No member selected for viewing.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private async void DeleteMember_Click(object sender, RoutedEventArgs e)
        {
            var member = (sender as Button)?.CommandParameter as Member;
            if (member != null)
            {
                if (MessageBox.Show($"Delete Member: {member.FirstName} {member.LastName}?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    try
                    {
                        //_context.Members.Remove(member);
                        //await _context.SaveChangesAsync();
                        //allMembers.Remove(member);
                        //LoadMembers(); // Reload instead of just removing from list to ensure data consistency
                        _context.Members.Remove(member);
                        await _context.SaveChangesAsync();
                        await LoadMembersAsync();
                        AdjustCurrentPage();
                        RefreshDataGrid();
                        UpdatePagination();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"An error occurred while deleting the member: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("No member selected for deletion.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchMembers_TextChanged(object sender, TextChangedEventArgs e)
        {
            var query = (sender as TextBox)?.Text.ToLower();
            if (!string.IsNullOrEmpty(query))
            {
                allMembers = _context.Members
                    .Where(m => m.FirstName.ToLower().Contains(query) ||
                                m.LastName.ToLower().Contains(query) ||
                                m.OtherNames.ToLower().Contains(query))
                    .ToList();
                CurrentPage = 1; // Reset to first page when searching
                AdjustCurrentPage();
                RefreshDataGrid();
                UpdatePagination();
            }
            else
            {
                LoadMembersAsync().ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        MessageBox.Show($"An error occurred while loading members: {t.Exception?.InnerException?.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else

                    {
                        AdjustCurrentPage();
                        RefreshDataGrid();
                        UpdatePagination();
                    }
                }, TaskScheduler.FromCurrentSynchronizationContext());
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
    }
}