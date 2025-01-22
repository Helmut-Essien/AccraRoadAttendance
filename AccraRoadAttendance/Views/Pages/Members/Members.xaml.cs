using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AccraRoadAttendance.Views.Pages.Members
{
    public partial class Members : UserControl
    {
        private List<Member> allMembers; // All members in the system
        private List<Member> displayedMembers; // Members displayed on the current page
        private int currentPage = 1;
        private const int pageSize = 15;

        public Members()
        {
            InitializeComponent();
            LoadMembers();
        }

        private void LoadMembers()
        {
            // Simulate loading members from a database or file
            allMembers = Enumerable.Range(1, 100).Select(i => new Member
            {
                Id = i,
                Name = $"Member {i}",
                Email = $"member{i}@example.com",
                PhoneNumber = $"123-456-789{i % 10}"
            }).ToList();

            RefreshDataGrid();
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
            // Logic for adding a new member
        }

        private void EditMember_Click(object sender, RoutedEventArgs e)
        {
            var member = (sender as Button)?.Tag as Member;
            MessageBox.Show($"Edit Member: {member?.Name}");
            // Logic for editing the member
        }

        private void DeleteMember_Click(object sender, RoutedEventArgs e)
        {
            var member = (sender as Button)?.Tag as Member;
            if (MessageBox.Show($"Delete Member: {member?.Name}?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                allMembers.Remove(member);
                RefreshDataGrid();
            }
        }

        private void SearchMembers_TextChanged(object sender, TextChangedEventArgs e)
        {
            var query = (sender as TextBox)?.Text.ToLower();
            if (!string.IsNullOrEmpty(query))
            {
                allMembers = allMembers.Where(m => m.Name.ToLower().Contains(query)).ToList();
            }
            else
            {
                LoadMembers();
            }
            RefreshDataGrid();
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

    public class Member
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
    }
}
