using AccraRoadAttendance.Data;
using AccraRoadAttendance.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AccraRoadAttendance.Views.Pages.Members
{
    public partial class Members : UserControl
    {
        private readonly MainWindow _mainWindow;
        private readonly AttendanceDbContext _context;
        private List<Member> allMembers; // All members in the system
        private List<Member> displayedMembers; // Members displayed on the current page
        private int currentPage = 1;
        private const int pageSize = 15;

        public Members(AttendanceDbContext context, MainWindow mainWindow)
        {
            InitializeComponent();
            _context = context;
            _mainWindow = mainWindow;
            LoadMembers();
        }

        private async void LoadMembers()
        {
            try
            {
                allMembers = await _context.Members.ToListAsync();
                RefreshDataGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while loading members: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshDataGrid()
        {
            displayedMembers = allMembers
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            membersDataGrid.ItemsSource = displayedMembers;
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
                        RefreshDataGrid();
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
                RefreshDataGrid();
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
            }
        }

        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage * pageSize < allMembers.Count)
            {
                currentPage++;
                RefreshDataGrid();
            }
        }
    }
}