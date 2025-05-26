using AccraRoadAttendance.Data;
using AccraRoadAttendance.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace AccraRoadAttendance.Services
{
    public class SyncService
    {
        private readonly AttendanceDbContext _localContext;
        private readonly OnlineAttendanceDbContext _onlineContext;
        private readonly GoogleDriveService _googleDriveService;
        private readonly ILogger<SyncService> _logger;
        private DateTime _lastSyncTime;

        public SyncService(AttendanceDbContext localContext, OnlineAttendanceDbContext onlineContext, GoogleDriveService googleDriveService, ILogger<SyncService> logger)
        {
            _localContext = localContext ?? throw new ArgumentNullException(nameof(localContext));
            _onlineContext = onlineContext ?? throw new ArgumentNullException(nameof(onlineContext));
            _googleDriveService = googleDriveService ?? throw new ArgumentNullException(nameof(googleDriveService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _lastSyncTime = LoadLastSyncTime();
            MessageBox.Show($"Initial lastSyncTime: {_lastSyncTime}", "Debug");
            _logger.LogInformation("Initial lastSyncTime: {LastSyncTime:u}", _lastSyncTime);
        }

        public void SyncData()
        {
            try
            {
                PushLocalChanges();
                PullOnlineChanges();
                SaveLastSyncTime(DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                // Log the exception (e.g., using ILogger)
                //throw new InvalidOperationException("Synchronization failed.", ex);
                throw new InvalidOperationException( ex.Message);
            }
        }

         // Pushing Local Changes
        private void PushLocalChanges()
        {
            // Order matters: Members first due to Attendance dependency
            MessageBox.Show("Pushing Local Changes", "Syncing", MessageBoxButton.OK, MessageBoxImage.Information);
            PushMembers();
            PushAttendances();
            PushSummaries();
            MessageBox.Show("Local Changes Pushed", "Syncing", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        //private void PushMembers()
        //{
        //    var localMembers = _localContext.Members
        //        .Where(m => !m.SyncStatus || m.LastModified > _lastSyncTime)
        //        .ToList();
        //    // Debug: Show number of members to push and _lastSyncTime
        //    MessageBox.Show($"Members to push: {localMembers.Count}, lastSyncTime: {_lastSyncTime}", "Debug");
        //    if (localMembers.Any())
        //    {
        //        // Debug: Show LastModified of the first member
        //        MessageBox.Show($"First member LastModified: {localMembers.First().LastModified}", "Debug");
        //    }

        //    foreach (var member in localMembers)
        //    {
        //        try
        //        {
        //            var onlineMember = _onlineContext.Members.Find(member.Id);
        //            if (onlineMember == null || onlineMember.LastModified < member.LastModified)
        //            {
        //                // Handle image upload if necessary
        //                if (!string.IsNullOrEmpty(member.PicturePath) && !member.PicturePath.StartsWith("https://drive.google.com"))
        //                {
        //                    member.PicturePath = _googleDriveService.UploadImage(member.PicturePath);
        //                }

        //                if (onlineMember == null)
        //                {
        //                    _onlineContext.Members.Add(member);
        //                }
        //                else
        //                {
        //                    _onlineContext.Entry(onlineMember).CurrentValues.SetValues(member);
        //                }

        //                _onlineContext.SaveChanges();
        //                member.SyncStatus = true;

        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            // Log error and continue with next member
        //            Console.WriteLine($"Failed to sync member {member.Id}: {ex.Message}");
        //            MessageBox.Show(ex.Message, "Sync Error", MessageBoxButton.OK, MessageBoxImage.Error);
        //            continue;
        //        }
        //    }
        //    _localContext.SaveChanges();
        //    MessageBox.Show("Local Members Pushed", "Syncing", MessageBoxButton.OK, MessageBoxImage.Information);
        //}

        private void PushMembers()
        {
            var localMembers = _localContext.Members
                .Where(m => !m.SyncStatus || m.LastModified > _lastSyncTime)
                .ToList();

            _logger.LogInformation("Pushing local members: count={Count}, since={LastSyncTime:u}",
                             localMembers.Count, _lastSyncTime);

            if (localMembers.Any())
            {
                _logger.LogInformation("First member LastModified: {FirstLastModified:u}",
                                 localMembers.First().LastModified);
            }

            foreach (var member in localMembers)
            {
                try
                {
                    _logger.LogInformation("Processing member {MemberId}", member.Id);

                    var onlineMember = _onlineContext.Members.Find(member.Id);
                    bool needsSync = onlineMember == null || onlineMember.LastModified < member.LastModified;

                    _logger.LogInformation("Found online? {Exists}, onlineLastModified={OnlineLastModified:u}, needsSync={NeedsSync}",
                                     onlineMember != null, onlineMember?.LastModified, needsSync);

                    if (needsSync)
                    {
                        if (!string.IsNullOrEmpty(member.PicturePath)
                            && !member.PicturePath.StartsWith("https://drive.google.com"))
                        {
                            _logger.LogInformation("Uploading image for member {MemberId}", member.Id);
                            member.PicturePath = _googleDriveService.UploadImage(member.PicturePath);
                            _logger.LogInformation("Image uploaded: {PicturePath}", member.PicturePath);
                        }

                        if (onlineMember == null)
                        {
                            _logger.LogInformation("Adding new member {MemberId} to online DB", member.Id);
                            _onlineContext.Members.Add(member);
                        }
                        else
                        {
                            _logger.LogInformation("Updating online member {MemberId}", member.Id);
                            _onlineContext.Entry(onlineMember).CurrentValues.SetValues(member);
                        }

                        _onlineContext.SaveChanges();
                        _logger.LogInformation("Successfully synced member {MemberId}", member.Id);
                        member.SyncStatus = true;
                    }
                    else
                    {
                        _logger.LogInformation("Skipping sync for member {MemberId}", member.Id);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogInformation(ex, "Failed to sync member {MemberId}", member.Id);
                    continue;
                }
            }

            _localContext.SaveChanges();
            _logger.LogInformation("Local members pushed.");
        }


        private void PushAttendances()
        {
            var localAttendances = _localContext.Attendances
                .Where(a => !a.AttendanceSyncStatus || a.AttendanceLastModified > _lastSyncTime)
                .ToList();

            foreach (var attendance in localAttendances)
            {
                try
                {
                    var onlineAttendance = _onlineContext.Attendances.Find(attendance.Id);
                    if (onlineAttendance == null || onlineAttendance.AttendanceLastModified < attendance.AttendanceLastModified)
                    {
                        if (onlineAttendance == null)
                        {
                            _onlineContext.Attendances.Add(attendance);
                        }
                        else
                        {
                            _onlineContext.Entry(onlineAttendance).CurrentValues.SetValues(attendance);
                        }

                        _onlineContext.SaveChanges();
                        attendance.AttendanceSyncStatus = true;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to sync attendance {attendance.Id}: {ex.Message}");
                    continue;
                }
            }
            _localContext.SaveChanges();
            MessageBox.Show("Local Attendances Pushed", "Syncing", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void PushSummaries()
        {
            var localSummaries = _localContext.ChurchAttendanceSummaries
                .Where(s => !s.SummarySyncStatus || s.SummaryLastModified > _lastSyncTime)
                .ToList();

            foreach (var summary in localSummaries)
            {
                try
                {
                    var onlineSummary = _onlineContext.ChurchAttendanceSummaries
                        .Find(summary.SummaryDate, summary.ServiceType);
                    if (onlineSummary == null || onlineSummary.SummaryLastModified < summary.SummaryLastModified)
                    {
                        if (onlineSummary == null)
                        {
                            _onlineContext.ChurchAttendanceSummaries.Add(summary);
                        }
                        else
                        {
                            _onlineContext.Entry(onlineSummary).CurrentValues.SetValues(summary);
                        }

                        _onlineContext.SaveChanges();
                        summary.SummarySyncStatus = true;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to sync summary {summary.SummaryDate}-{summary.ServiceType}: {ex.Message}");
                    continue;
                }
            }
            _localContext.SaveChanges();
            MessageBox.Show("Local Summaries Pushed", "Syncing", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        //Pulling Online Changes
        private void PullOnlineChanges()
        {
            // Order matters: Members first due to Attendance dependency
            MessageBox.Show("Pulling Online Changes", "Syncing", MessageBoxButton.OK, MessageBoxImage.Information);
            PullMembers();
            PullAttendances();
            PullSummaries();
            MessageBox.Show("Online Changes Pulled", "Syncing", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void PullMembers()
        {
            var onlineMembers = _onlineContext.Members
                .Where(m => m.LastModified > _lastSyncTime)
                .ToList();
            // Debug: Show number of members to pull and _lastSyncTime
            MessageBox.Show($"Members to pull: {onlineMembers.Count}, lastSyncTime: {_lastSyncTime}", "Debug");
            if (onlineMembers.Any())
            {
                // Debug: Show LastModified of the first member
                MessageBox.Show($"First online member LastModified: {onlineMembers.First().LastModified}", "Debug");
            }

            foreach (var onlineMember in onlineMembers)
            {
                try
                {
                    var localMember = _localContext.Members.Find(onlineMember.Id);
                    if (localMember == null || localMember.LastModified < onlineMember.LastModified)
                    {
                        // Handle image download if necessary
                        if (!string.IsNullOrEmpty(onlineMember.PicturePath) && onlineMember.PicturePath.StartsWith("https://drive.google.com"))
                        {
                            onlineMember.PicturePath = _googleDriveService.DownloadImage(onlineMember.PicturePath);
                        }

                        if (localMember == null)
                        {
                            _localContext.Members.Add(onlineMember);
                        }
                        else
                        {
                            _localContext.Entry(localMember).CurrentValues.SetValues(onlineMember);
                        }

                        _localContext.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to pull member {onlineMember.Id}: {ex.Message}");
                    continue;
                }
            }
            MessageBox.Show("Online Members Pulled", "Syncing", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void PullAttendances()
        {
            var onlineAttendances = _onlineContext.Attendances
                .Where(a => a.AttendanceLastModified > _lastSyncTime)
                .ToList();

            foreach (var onlineAttendance in onlineAttendances)
            {
                try
                {
                    var localAttendance = _localContext.Attendances.Find(onlineAttendance.Id);
                    if (localAttendance == null || localAttendance.AttendanceLastModified < onlineAttendance.AttendanceLastModified)
                    {
                        if (localAttendance == null)
                        {
                            _localContext.Attendances.Add(onlineAttendance);
                        }
                        else
                        {
                            _localContext.Entry(localAttendance).CurrentValues.SetValues(onlineAttendance);
                        }

                        _localContext.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to pull attendance {onlineAttendance.Id}: {ex.Message}");
                    continue;
                }
            }
            MessageBox.Show("Online Attendances Pulled", "Syncing", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void PullSummaries()
        {
            var onlineSummaries = _onlineContext.ChurchAttendanceSummaries
                .Where(s => s.SummaryLastModified > _lastSyncTime)
                .ToList();

            foreach (var onlineSummary in onlineSummaries)
            {
                try
                {
                    var localSummary = _localContext.ChurchAttendanceSummaries
                        .Find(onlineSummary.SummaryDate, onlineSummary.ServiceType);
                    if (localSummary == null || localSummary.SummaryLastModified < onlineSummary.SummaryLastModified)
                    {
                        if (localSummary == null)
                        {
                            _localContext.ChurchAttendanceSummaries.Add(onlineSummary);
                        }
                        else
                        {
                            _localContext.Entry(localSummary).CurrentValues.SetValues(onlineSummary);
                        }

                        _localContext.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to pull summary {onlineSummary.SummaryDate}-{onlineSummary.ServiceType}: {ex.Message}");
                    continue;
                }
            }
            MessageBox.Show("Online Summaries Pulled", "Syncing", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // Helper Methods
        private DateTime LoadLastSyncTime()
        {
            // Load the last sync time from the local context
            MessageBox.Show("Loading Last Sync Time", "Syncing", MessageBoxButton.OK, MessageBoxImage.Information);
            var syncMetadata = _localContext.SyncMetadata.FirstOrDefault(sm => sm.Key == "LastSyncTime");
            //if (syncMetadata != null && DateTime.TryParse(syncMetadata.Value, out DateTime lastSyncTime))
            //{
            //    return lastSyncTime;

            //}
            //return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            if (syncMetadata != null)
            {
                // Debug: Show raw SyncMetadata value
                MessageBox.Show($"SyncMetadata Value: {syncMetadata.Value}", "Debug");
                if (DateTime.TryParse(syncMetadata.Value, out DateTime lastSyncTime))
                {
                    // Debug: Show parsed lastSyncTime
                    MessageBox.Show($"Parsed lastSyncTime: {lastSyncTime}", "Debug");
                    return lastSyncTime;
                }
                else
                {
                    // Debug: Indicate parsing failure
                    MessageBox.Show("Failed to parse SyncMetadata Value", "Debug");
                }
            }
            else
            {
                // Debug: Indicate no metadata found
                MessageBox.Show("No SyncMetadata found for LastSyncTime", "Debug");
            }
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        }

        private void SaveLastSyncTime(DateTime time)
        {
            // Save the last sync time to the local context
            MessageBox.Show("Saving Last Sync Time", "Syncing", MessageBoxButton.OK, MessageBoxImage.Information);
            var syncMetadata = _localContext.SyncMetadata.FirstOrDefault(sm => sm.Key == "LastSyncTime");
            if (syncMetadata == null)
            {
                syncMetadata = new SyncMetadata { Key = "LastSyncTime", Value = time.ToString("o") };
                _localContext.SyncMetadata.Add(syncMetadata);
            }
            else
            {
                syncMetadata.Value = time.ToString("o");
            }
            _localContext.SaveChanges();
            _lastSyncTime = time;
            MessageBox.Show("Last Sync Time Saved", "Syncing", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}