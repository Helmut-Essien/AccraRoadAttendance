using AccraRoadAttendance.Data;
using AccraRoadAttendance.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AccraRoadAttendance.Services
{
    public class SyncService
    {
        private readonly AttendanceDbContext _localContext;
        private readonly OnlineAttendanceDbContext _onlineContext;
        private readonly GoogleDriveService _googleDriveService;
        private DateTime _lastSyncTime;

        public SyncService(AttendanceDbContext localContext, OnlineAttendanceDbContext onlineContext, GoogleDriveService googleDriveService)
        {
            _localContext = localContext ?? throw new ArgumentNullException(nameof(localContext));
            _onlineContext = onlineContext ?? throw new ArgumentNullException(nameof(onlineContext));
            _googleDriveService = googleDriveService ?? throw new ArgumentNullException(nameof(googleDriveService));
            _lastSyncTime = LoadLastSyncTime();
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
                throw new InvalidOperationException("Synchronization failed.", ex);
            }
        }

         // Pushing Local Changes
        private void PushLocalChanges()
        {
            // Order matters: Members first due to Attendance dependency
            PushMembers();
            PushAttendances();
            PushSummaries();
        }

        private void PushMembers()
        {
            var localMembers = _localContext.Members
                .Where(m => !m.SyncStatus || m.LastModified > _lastSyncTime)
                .ToList();

            foreach (var member in localMembers)
            {
                try
                {
                    var onlineMember = _onlineContext.Members.Find(member.Id);
                    if (onlineMember == null || onlineMember.LastModified < member.LastModified)
                    {
                        // Handle image upload if necessary
                        if (!string.IsNullOrEmpty(member.PicturePath) && !member.PicturePath.StartsWith("https://drive.google.com"))
                        {
                            member.PicturePath = _googleDriveService.UploadImage(member.PicturePath);
                        }

                        if (onlineMember == null)
                        {
                            _onlineContext.Members.Add(member);
                        }
                        else
                        {
                            _onlineContext.Entry(onlineMember).CurrentValues.SetValues(member);
                        }

                        _onlineContext.SaveChanges();
                        member.SyncStatus = true;
                    }
                }
                catch (Exception ex)
                {
                    // Log error and continue with next member
                    Console.WriteLine($"Failed to sync member {member.Id}: {ex.Message}");
                    continue;
                }
            }
            _localContext.SaveChanges();
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
        }

        //Pulling Online Changes
        private void PullOnlineChanges()
        {
            // Order matters: Members first due to Attendance dependency
            PullMembers();
            PullAttendances();
            PullSummaries();
        }

        private void PullMembers()
        {
            var onlineMembers = _onlineContext.Members
                .Where(m => m.LastModified > _lastSyncTime)
                .ToList();

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
        }

        // Helper Methods
        private DateTime LoadLastSyncTime()
        {
            var syncMetadata = _localContext.SyncMetadata.FirstOrDefault(sm => sm.Key == "LastSyncTime");
            if (syncMetadata != null && DateTime.TryParse(syncMetadata.Value, out DateTime lastSyncTime))
            {
                return lastSyncTime;
            }
            return DateTime.MinValue;
        }

        private void SaveLastSyncTime(DateTime time)
        {
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
        }
    }
}