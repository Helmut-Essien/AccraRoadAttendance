using AccraRoadAttendance.Data;
using AccraRoadAttendance.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Linq;

namespace AccraRoadAttendance.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private readonly AttendanceContext _context;
        private ObservableCollection<Member> _members;
        private string _searchText;

        public MainViewModel()
        {
            // Use a try-catch block to handle potential database creation or migration issues
            try
            {
                _context = new AttendanceContext();

                // Prefer using migrations for schema management
                _context.Database.Migrate();

                // Load members after ensuring the database is migrated
                LoadMembers();
            }
            catch (Exception ex)
            {
                // Log or handle the exception. Here, we're just showing an error message for simplicity.
                OnPropertyChanged(nameof(ErrorMessage)); // If you have an error message property
            }

            AddMemberCommand = new RelayCommand(AddMember);
            GenerateReportCommand = new RelayCommand(GenerateReport);
            TogglePresenceCommand = new RelayCommand<Member>(TogglePresence);
        }

        public ObservableCollection<Member> Members
        {
            get => _members;
            set => SetProperty(ref _members, value);
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                SetProperty(ref _searchText, value);
                FilterMembers();
            }
        }

        // Add this if you want to show error messages in UI
        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public IRelayCommand AddMemberCommand { get; }
        public IRelayCommand GenerateReportCommand { get; }
        public IRelayCommand<Member> TogglePresenceCommand { get; }

        // Move member loading to a separate method for clarity
        private void LoadMembers()
        {
            Members = new ObservableCollection<Member>(_context.Members.ToList());
        }

        private void FilterMembers()
        {
            var filteredMembers = _context.Members.Where(m =>
                string.IsNullOrEmpty(SearchText) ||
                m.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                .ToList();
            Members = new ObservableCollection<Member>(filteredMembers);
        }

        private void AddMember()
        {
            // TODO: Implement adding a new member
            // Example:
            // var newMember = new Member { Name = "New Member", IsPresent = false };
            // _context.Members.Add(newMember);
            // _context.SaveChanges();
            // Members.Add(newMember);
        }

        private void GenerateReport()
        {
            // TODO: Implement report generation
            // Example:
            // var report = _context.Members.Where(m => m.IsPresent).ToList();
            // // Do something with the report data, like save to file or display
        }

        private void TogglePresence(Member member)
        {
            member.IsPresent = !member.IsPresent;
            try
            {
                _context.SaveChanges();
                OnPropertyChanged(nameof(Members)); // Refresh the UI
            }
            catch (DbUpdateException ex)
            {
                // Log or handle the exception
                ErrorMessage = "Failed to update member presence: " + ex.Message;
            }
        }
    }
}