using AccraRoadAttendance.Data;
using AccraRoadAttendance.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AccraRoadAttendance.Views.Pages.Members
{
    public partial class Members : UserControl, INotifyPropertyChanged
    {
        private readonly MainWindow _mainWindow;
        private readonly AttendanceDbContext _context;
        private List<Member> allMembers; // All members in the system
        private List<Member> displayedMembers; // Members displayed on the current page
        private int currentPage = 1;
        private const int pageSize = 3;

        public Members(AttendanceDbContext context, MainWindow mainWindow)
        {
            InitializeComponent();
            _context = context;
            _mainWindow = mainWindow;
            DataContext = this; // Set DataContext to this instance
            
            IsPaginationVisible = false;
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

        private async void LoadMembers()
        {
            try
            {
                allMembers = await _context.Members.ToListAsync();
                currentPage = 1;
                RefreshDataGrid();
                UpdatePagination();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while loading members: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshDataGrid()
        {
            int totalPages = (int)Math.Ceiling((double)allMembers.Count / pageSize);
            displayedMembers = allMembers
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            membersDataGrid.ItemsSource = displayedMembers;
        }

        private void UpdatePagination()
        {
            int totalPages = (int)Math.Ceiling((double)allMembers.Count / pageSize);
            IsPaginationVisible = totalPages > 1;

            if (IsPaginationVisible)
            {
                int maxPagesToShow = 4; // Number of pages to show at once
                int startPage = 1;
                int endPage = totalPages;

                // Logic to determine which pages to display
                if (totalPages > maxPagesToShow)
                {
                    if (currentPage <= maxPagesToShow / 2)
                    {
                        startPage = 1;
                        endPage = maxPagesToShow;
                    }
                    else if (currentPage > totalPages - maxPagesToShow / 2)
                    {
                        startPage = totalPages - maxPagesToShow + 1;
                        endPage = totalPages;
                    }
                    else
                    {
                        startPage = currentPage - maxPagesToShow / 2;
                        endPage = currentPage + maxPagesToShow / 2 - 1;
                    }
                }

                List<int> pages = new List<int>();

                // Show pages
                for (int i = startPage; i <= endPage; i++)
                {
                    pages.Add(i);
                }

                // If there are more pages after the endPage, show ellipsis and last page
                if (endPage < totalPages)
                {
                    // Check if there's more than one page between endPage and totalPages
                    if (totalPages - endPage > 1)
                    {
                        pages.Add(-1); // -1 represents the ellipsis
                    }
                    // The last page will be handled by LastPageNumber binding
                }

                PageNumbers = pages;
                LastPageNumber = totalPages;
            }
        }


        private void AddMember_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Add Member clicked!");
            _mainWindow.NavigateToAddMembers();
        }

        private void EditMember_Click(object sender, RoutedEventArgs e)
        {
            var member = (sender as Button)?.CommandParameter as Member;
            if (member != null)
            {
                _mainWindow.NavigateToEditMembers(member);
            }
            else
            {
                MessageBox.Show("No member selected for editing.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                        _context.Members.Remove(member);
                        await _context.SaveChangesAsync();
                        allMembers.Remove(member);
                        LoadMembers(); // Reload instead of just removing from list to ensure data consistency
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
                currentPage = 1; // Reset to first page when searching
                RefreshDataGrid();
                UpdatePagination();
            }
            else
            {
                LoadMembers();
            }
        }

        private void PreviousPage_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage > 1)
            {
                currentPage--;
                RefreshDataGrid();
                UpdatePagination();
            }
        }

        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            int totalPages = (int)Math.Ceiling((double)allMembers.Count / pageSize);
            if (currentPage < totalPages)
            {
                currentPage++;
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
                    currentPage = pageNumber;
                    RefreshDataGrid();
                    UpdatePagination();
                }
            }
        }
    }
}