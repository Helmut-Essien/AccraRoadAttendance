﻿using AccraRoadAttendance.Data;
using AccraRoadAttendance.Models;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Member = AccraRoadAttendance.Models.Member;

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
            //MessageBox.Show($"Initial lastSyncTime: {_lastSyncTime}", "Debug");
            _logger.LogInformation("Initial lastSyncTime: {LastSyncTime:u}", _lastSyncTime);
        }

        public async Task SyncDataAsync(IProgress<string>? progress = null)
        {
            const int maxRetries = 2;
            const int delayMs = 2000; // 5 seconds
            int attempt = 0;
            TimeSpan delay = TimeSpan.FromSeconds(5);
            bool syncSucceeded = false;

            while (attempt < maxRetries)
            {
                try
                {
                    progress?.Report("Starting synchronization...");
                    _logger.LogInformation("Starting synchronization");

                    PushLocalChanges(progress);
                    PullOnlineChanges(progress);

                    SaveLastSyncTime(DateTime.UtcNow);
                    //progress?.Report("Synchronization complete.");
                    _logger.LogInformation("SyncData completed successfully at {Now:u}", DateTime.UtcNow);

                    syncSucceeded = true;
                    break; // <-- Add this line to exit the loop on s
                }
                catch (Exception ex)
                {
                    attempt++;
                    // Log the exception (e.g., using ILogger)
                    progress?.Report($"Synchronization failed.");
                    //progress?.Report($"Synchronization /*attempt {attempt}*/ failed.");
                    await Task.Delay(delayMs);
                    //progress?.Report($"Retrying in {delay.TotalSeconds}s ({attempt}/{maxRetries})...");

                    _logger.LogInformation(ex, "SyncData failed on attempt {Attempt}: {Message}", attempt, ex.Message);
                    //throw new InvalidOperationException("Synchronization failed.", ex);
                    //throw new InvalidOperationException("Synchronization failed.", ex); // Pass 'ex' as the inner exception
                    if (attempt < maxRetries)
                    {
                        for (int i = (int)delay.TotalSeconds; i > 0; i--)
                        {
                            progress?.Report($"Retrying in {i}s ({attempt}/{maxRetries})...");
                            
                            await Task.Delay(1000);
                        }
                    }
                    if (attempt == maxRetries)
                    {
                        try
                        {
                            //progress?.Report($"Synchronization attempt {attempt}failed.");
                            progress?.Report($"Synchronization failed.");
                            await Task.Delay(delayMs);

                            _logger.LogError(ex, "Sync ultimately failed after {MaxRetries} attempts.", maxRetries);
                            progress?.Report($"Synchronization failed after {maxRetries} attempts.");


                            throw new InvalidOperationException($"Synchronization failed after {maxRetries} attempts.");
                        }
                        catch (InvalidOperationException ioe)
                {
                    // Handle the exception (e.g., log it, show a message, etc.)
                    _logger.LogError(ioe, "Sync failed after max retries.");
                            //MessageBox.Show(ioe.Message, "Sync Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            progress?.Report($"Synchronization failed after {maxRetries} attempts.");
                            await Task.Delay(delayMs);
                            return; // Exit the method if max retries reached
                }


            }
                    // Wait before retrying
                    await Task.Delay(delayMs);
                }
                
            }
            if (syncSucceeded)
            {
                progress?.Report("Synchronization complete.");
            }
        }


    

         // Pushing Local Changes
        private void PushLocalChanges(IProgress<string>? progress = null)
        {
            // Order matters: Members first due to Attendance dependency
            //MessageBox.Show("Pushing Local Changes", "Syncing", MessageBoxButton.OK, MessageBoxImage.Information);
            progress?.Report("Uploading local members...");
            PushMembers();
            progress?.Report("Uploading local attendances...");
            PushAttendances();
            progress?.Report("Uploading local summaries...");
            PushSummaries();
            //MessageBox.Show("Local Changes Pushed", "Syncing", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        

        private void PushMembers()
        {
            var localMembers = _localContext.Members
                .AsNoTracking()
                .Where(m => !m.SyncStatus || m.LastModified > _lastSyncTime)
                .ToList();

            _logger.LogInformation("Pushing local members: count={Count}, since={LastSyncTime:u}",
                             localMembers.Count, _lastSyncTime);

            //if (localMembers.Any())
            //{
            //    _logger.LogInformation("First member LastModified: {FirstLastModified:u}",
            //                     localMembers.First().LastModified);
            //}

            foreach (var member in localMembers)
            {
                try
                {
                    _logger.LogInformation("Processing member {MemberId}", member.Id);

                    var onlineMember = _onlineContext.Members
                       .AsNoTracking()
                       .SingleOrDefault(m => m.Id == member.Id);
                    bool needsSync = onlineMember == null || onlineMember.LastModified < member.LastModified /*|| member.SyncStatus == false*/;

                    _logger.LogInformation("Found online? {Exists}, onlineLastModified={OnlineLastModified:u}, needsSync={NeedsSync}",
                                     onlineMember != null, onlineMember?.LastModified, needsSync);
                    if (!needsSync)
                    {
                        _logger.LogInformation("Skipping Member {MemberId} (no newer changes).", member.Id);
                        continue;
                    }

                    // Create a new Member object for the online context with all properties
                    var onlineMemberToSync = new Member
                    {
                        Id = member.Id,
                        FirstName = member.FirstName,
                        LastName = member.LastName,
                        OtherNames = member.OtherNames,
                        PhoneNumber = member.PhoneNumber,
                        Email = member.Email,
                        Sex = member.Sex,
                        PicturePath = member.PicturePath, // Initially set to local path
                        MembershipStartDate = member.MembershipStartDate,
                        IsActive = member.IsActive,
                        DateOfBirth = member.DateOfBirth,
                        Nationality = member.Nationality,
                        educationalLevel = member.educationalLevel,
                        Address = member.Address,
                        Location = member.Location,
                        HasFamilyMemberInChurch = member.HasFamilyMemberInChurch,
                        maritalStatus = member.maritalStatus,
                        occupationType = member.occupationType,
                        IsBaptized = member.IsBaptized,
                        PlaceOfBaptism = member.PlaceOfBaptism,
                        BaptismDate = member.BaptismDate,
                        Hometown = member.Hometown,
                        NextOfKinName = member.NextOfKinName,
                        NextOfKinContact = member.NextOfKinContact,
                        MotherName = member.MotherName,
                        MotherContact = member.MotherContact,
                        FatherName = member.FatherName,
                        FatherContact = member.FatherContact,
                        FamilyMemberName = member.FamilyMemberName,
                        FamilyMemberContact = member.FamilyMemberContact,
                        MemberRole = member.MemberRole,
                        Skills = member.Skills,
                        LastModified = member.LastModified,
                        SyncStatus = member.SyncStatus,
                        SpouseName = member.SpouseName,
                        SpouseContact = member.SpouseContact,
                        OccupationDescription = member.OccupationDescription
                    };


                    if (needsSync)
                    {
                        
                        // 3. Detach any tracked copy of this Member in the online context
                        var trackedInOnline = _onlineContext.ChangeTracker
                            .Entries<Member>()
                            .FirstOrDefault(e => e.Entity.Id == member.Id);
                        if (trackedInOnline != null)
                        {
                            trackedInOnline.State = EntityState.Detached;
                        }

                        _logger.LogInformation("Local member {MemberId} PicturePath: {PicturePath}", member.Id, member.PicturePath);
                        if (!string.IsNullOrEmpty(onlineMemberToSync.PicturePath)
                            && !onlineMemberToSync.PicturePath.StartsWith("https://drive.google.com"))
                        {
                            _logger.LogInformation("Uploading image for member {MemberId}", member.Id);
                            onlineMemberToSync.PicturePath = _googleDriveService.UploadImage(member.PicturePath);
                            _logger.LogInformation("Image uploaded: {PicturePath}", onlineMemberToSync.PicturePath);
                        }

                        if (onlineMember == null)
                        {
                            _logger.LogInformation("Adding new member {MemberId} to online DB", member.Id);
                            _onlineContext.Members.Add(onlineMemberToSync);
                        }
                        //else
                        //{
                        //    _logger.LogInformation("Updating online member {MemberId}", member.Id);
                        //    //_onlineContext.Entry(onlineMember).CurrentValues.SetValues(member);
                        //    _onlineContext.Members.Update(onlineMemberToSync);
                        //    _onlineContext.Entry(onlineMemberToSync).State = EntityState.Modified;
                        //}
                        else if (onlineMember.LastModified < member.LastModified)
                        {
                            _onlineContext.Members.Attach(onlineMember);
                            _onlineContext.Entry(onlineMember).CurrentValues.SetValues(onlineMemberToSync);
                            _onlineContext.Entry(onlineMember).State = EntityState.Modified;
                        }
                        else
                        {
                            continue; // No newer changes
                        }

                        _onlineContext.SaveChanges();

                        member.SyncStatus = true;
                        // Instead of attaching, do this:
                        _localContext.Members
                            .Where(m => m.Id == member.Id)
                            .ExecuteUpdate(setters => setters.SetProperty(m => m.SyncStatus, true));
                        _localContext.SaveChanges();

                        _logger.LogInformation("Local member {MemberId} Sync Status: {SyncStatus}", member.Id, member.SyncStatus);
                        _logger.LogInformation("Successfully synced member {MemberId}", member.Id);
                        
                        
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

            
            _logger.LogInformation("Local members pushed.");
        }


        private void PushAttendances()
        {
            var localAttendances = _localContext.Attendances
                //.Include(a => a.Member)
                .Where(a => !a.AttendanceSyncStatus || a.AttendanceLastModified > _lastSyncTime)
                .ToList();

            _logger.LogInformation("Pushing {Count} local Attendances (since {LastSyncTime:u})", localAttendances.Count, _lastSyncTime);

            foreach (var attendance in localAttendances)
            {
                try
                {
                    //var onlineAttendance = _onlineContext.Attendances
                    //   .AsNoTracking()
                    //   .SingleOrDefault(a => a.Id == attendance.Id);
                    var onlineAttendance = _onlineContext.Attendances
               .AsNoTracking()
               .SingleOrDefault(a =>
                   a.Id == attendance.Id ||
                   (a.MemberId == attendance.MemberId &&
                    a.ServiceDate == attendance.ServiceDate &&
                    a.ServiceType == attendance.ServiceType));

                    if (onlineAttendance == null || onlineAttendance.AttendanceLastModified < attendance.AttendanceLastModified)
                    {
                        var trackedInOnline = _onlineContext.ChangeTracker
                        .Entries<Attendance>()
                        .FirstOrDefault(e => e.Entity.Id == attendance.Id);
                        if (trackedInOnline != null)
                        {
                            trackedInOnline.State = EntityState.Detached;
                        }

                        if (onlineAttendance == null)
                        {
                            _logger.LogInformation("Adding new attendance {AttendanceId} to online DB", attendance.Id);
                            //var newAttendance = new Attendance
                            //{
                            //    MemberId = attendance.MemberId,
                            //    Member = attendance.Member,
                            //    ServiceDate = attendance.ServiceDate,
                            //    ServiceType = attendance.ServiceType,
                            //    Status = attendance.Status,
                            //    Notes = attendance.Notes,
                            //    RecordedAt = attendance.RecordedAt,
                            //    AttendanceLastModified = attendance.AttendanceLastModified,
                            //    AttendanceSyncStatus = attendance.AttendanceSyncStatus
                            //    // Add other properties as needed
                            //};
                            //_onlineContext.Attendances.Add(newAttendance);
                            // Detach or mark the Member as Unchanged
                            _onlineContext.Entry(attendance.Member).State = EntityState.Unchanged;
                            _onlineContext.Attendances.Add(attendance);
                        }
                        else if (onlineAttendance.AttendanceLastModified < attendance.AttendanceLastModified)
                        {//else

                            //_logger.LogInformation("Updating existing online Attendance {AttendanceId}.", attendance.Id);
                            //_onlineContext.Entry(onlineAttendance).CurrentValues.SetValues(new
                            //{
                            //    attendance.AttendanceLastModified,
                            //    attendance.AttendanceSyncStatus,
                            //    attendance.MemberId,
                            //    attendance.Notes,
                            //    attendance.RecordedAt,
                            //    attendance.ServiceDate,
                            //    attendance.ServiceType,
                            //    attendance.Status
                            //});
                            //_onlineContext.Attendances.Update(attendance);
                            //_onlineContext.Entry(attendance).State = EntityState.Modified;
                            _logger.LogInformation("Updating existing online Attendance {AttendanceId}.", onlineAttendance.Id);
                            // Attach the onlineAttendance and update its values
                            _onlineContext.Attendances.Attach(attendance);
                            _onlineContext.Entry(onlineAttendance).CurrentValues.SetValues(attendance);
                            _onlineContext.Entry(onlineAttendance).State = EntityState.Modified;
                        }
                        else
                        {
                            _logger.LogInformation("Skipping attendance {AttendanceId} (no newer changes).", attendance.Id);
                            continue;
                        }

                        _onlineContext.SaveChanges();
                        _logger.LogInformation("Attendance {AttendanceId} successfully pushed online.", attendance.Id);
                        attendance.AttendanceSyncStatus = true;
                        _localContext.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to sync attendance {attendance.Id}: {ex.Message}");
                    continue;
                }
            }
            
            //MessageBox.Show("Local Attendances Pushed", "Syncing", MessageBoxButton.OK, MessageBoxImage.Information);
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
                    //var onlineSummary = _onlineContext.ChurchAttendanceSummaries
                    //    .Find(summary.SummaryDate, summary.ServiceType);
                    var onlineSummary = _onlineContext.ChurchAttendanceSummaries
                        .AsNoTracking()
                        .SingleOrDefault(s =>
                            s.SummaryDate == summary.SummaryDate &&
                            s.ServiceType == summary.ServiceType);
                    if (onlineSummary == null || onlineSummary.SummaryLastModified < summary.SummaryLastModified)
                    {
                        var trackedInOnline = _onlineContext.ChangeTracker
                       .Entries<ChurchAttendanceSummary>()
                       .FirstOrDefault(e =>
                           e.Entity.SummaryDate == summary.SummaryDate &&
                           e.Entity.ServiceType == summary.ServiceType);
                        if (trackedInOnline != null)
                        {
                            trackedInOnline.State = EntityState.Detached;
                        }

                        if (onlineSummary == null)
                        {
                            _logger.LogInformation(
                           "Adding new Summary {Date}-{Type} to online DB.",
                           summary.SummaryDate, summary.ServiceType);
                            _onlineContext.ChurchAttendanceSummaries.Add(summary);
                        }
                        else
                        {
                            _logger.LogInformation(
                            "Updating existing online Summary {Date}-{Type}.",
                            summary.SummaryDate, summary.ServiceType);
                            //_onlineContext.Entry(onlineSummary).CurrentValues.SetValues(summary);
                            _onlineContext.ChurchAttendanceSummaries.Update(summary);
                            _onlineContext.Entry(summary).State = EntityState.Modified;
                        }

                        _onlineContext.SaveChanges();
                        summary.SummarySyncStatus = true;
                        _localContext.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogInformation($"Failed to sync summary {summary.SummaryDate}-{summary.ServiceType}: {ex.Message}");
                    continue;
                }
            }
            
            //MessageBox.Show("Local Summaries Pushed", "Syncing", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        //Pulling Online Changes
        private void PullOnlineChanges(IProgress<string>? progress = null)
        {
            // Order matters: Members first due to Attendance dependency
            //MessageBox.Show("Pulling Online Changes", "Syncing", MessageBoxButton.OK, MessageBoxImage.Information);
            progress?.Report("Downloading online members...");
            PullMembers();
            progress?.Report("Downloading online attendances...");
            PullAttendances();
            progress?.Report("Downloading online summaries...");
            PullSummaries();
            //MessageBox.Show("Online Changes Pulled", "Syncing", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void PullMembers()
        {
            //var onlineMembers = _onlineContext.Members
            //    .Where(m => m.LastModified > _lastSyncTime)
            //    .ToList();
            var onlineMembers = _onlineContext.Members
               .Where(m => m.LastModified > _lastSyncTime)
               .AsNoTracking()
               .ToList();
            // Debug: Show number of members to pull and _lastSyncTime
            //MessageBox.Show($"Members to pull: {onlineMembers.Count}, lastSyncTime: {_lastSyncTime}", "Debug");
            //if (onlineMembers.Any())
            //{
            //    // Debug: Show LastModified of the first member
            //    MessageBox.Show($"First online member LastModified: {onlineMembers.First().LastModified}", "Debug");
            //}

            foreach (var onlineMember in onlineMembers)
            {
                try
                {
                    //var localMember = _localContext.Members.Find(onlineMember.Id);
                    var localMember = _localContext.Members
                        .AsNoTracking()
                        .SingleOrDefault(m => m.Id == onlineMember.Id);

                    if (localMember == null || localMember.LastModified < onlineMember.LastModified)
                    {
                        var trackedInLocal = _localContext.ChangeTracker
                        .Entries<Member>()
                        .FirstOrDefault(e => e.Entity.Id == onlineMember.Id);
                        if (trackedInLocal != null)
                        {
                            trackedInLocal.State = EntityState.Detached;
                        }

                        

                        _logger.LogInformation("Before download, PicturePath: {PicturePath}", onlineMember.PicturePath);
                        // Handle image download if necessary
                        if (!string.IsNullOrEmpty(onlineMember.PicturePath) && onlineMember.PicturePath.StartsWith("https://drive.google.com"))
                        {
                            onlineMember.PicturePath = _googleDriveService.DownloadImage(onlineMember.PicturePath);
                            _logger.LogInformation("After download, PicturePath: {PicturePath}", onlineMember.PicturePath);
                        }

                        if (localMember == null)
                        {
                            var existingMember = _localContext.Members.FirstOrDefault(m => m.PhoneNumber == onlineMember.PhoneNumber);
                            if (existingMember == null)
                            {
                                _localContext.Members.Add(onlineMember);
                                _logger.LogInformation("After download, PicturePath: {PicturePath}", onlineMember.PicturePath);
                            }
                            else
                            {
                                MessageBox.Show($"Skipping member {onlineMember.Id} due to duplicate phone number {onlineMember.PhoneNumber}.");
                            }
                            //_localContext.Members.Add(onlineMember);
                            //_logger.LogInformation("After download, PicturePath: {PicturePath}", onlineMember.PicturePath);
                        }
                        else
                        {
                            //_localContext.Entry(localMember).CurrentValues.SetValues(onlineMember);
                            _localContext.Members.Attach(onlineMember);
                            _localContext.Entry(onlineMember).State = EntityState.Modified;
                            _logger.LogInformation("After download, PicturePath: {PicturePath}", onlineMember.PicturePath);
                        }

                        _logger.LogInformation("After download, PicturePath: {PicturePath}", onlineMember.PicturePath);
                        _localContext.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogInformation($"Failed to pull member {onlineMember.Id}: {ex.Message}");
                    continue;
                }
            }
            //MessageBox.Show("Online Members Pulled", "Syncing", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void PullAttendances()
        {
            //var onlineAttendances = _onlineContext.Attendances
            //    .Where(a => a.AttendanceLastModified > _lastSyncTime)
            //    .ToList();
            var onlineAttendances = _onlineContext.Attendances
                .Where(a => a.AttendanceLastModified > _lastSyncTime)
                .AsNoTracking()
                .ToList();

            foreach (var onlineAttendance in onlineAttendances)
            {
                try
                {
                    //var localAttendance = _localContext.Attendances.Find(onlineAttendance.Id);
                    var localAttendance = _localContext.Attendances
                        .AsNoTracking()
                        .SingleOrDefault(a => a.Id == onlineAttendance.Id);

                    if (localAttendance == null || localAttendance.AttendanceLastModified < onlineAttendance.AttendanceLastModified)
                    {
                        var trackedInLocal = _localContext.ChangeTracker
                       .Entries<Attendance>()
                       .FirstOrDefault(e => e.Entity.Id == onlineAttendance.Id);
                        if (trackedInLocal != null)
                        {
                            trackedInLocal.State = EntityState.Detached;
                        }

                        if (localAttendance == null)
                        {
                            _localContext.Attendances.Add(onlineAttendance);
                        }
                        else
                        {
                            //_localContext.Entry(localAttendance).CurrentValues.SetValues(onlineAttendance);
                            _logger.LogInformation("Updating existing local Attendance {AttendanceId}.", onlineAttendance.Id);
                            _localContext.Attendances.Attach(onlineAttendance);
                            _localContext.Entry(onlineAttendance).State = EntityState.Modified;
                        }

                        _localContext.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogInformation($"Failed to pull attendance {onlineAttendance.Id}: {ex.Message}");
                    continue;
                }
            }
            //MessageBox.Show("Online Attendances Pulled", "Syncing", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void PullSummaries()
        {
            //var onlineSummaries = _onlineContext.ChurchAttendanceSummaries
            //    .Where(s => s.SummaryLastModified > _lastSyncTime)
            //    .ToList();
            var onlineSummaries = _onlineContext.ChurchAttendanceSummaries
                .Where(s => s.SummaryLastModified > _lastSyncTime)
                .AsNoTracking()
                .ToList();

            foreach (var onlineSummary in onlineSummaries)
            {
                try
                {
                    //var localSummary = _localContext.ChurchAttendanceSummaries
                    //    .Find(onlineSummary.SummaryDate, onlineSummary.ServiceType);
                    var localSummary = _localContext.ChurchAttendanceSummaries
                        .AsNoTracking()
                        .SingleOrDefault(s =>
                            s.SummaryDate == onlineSummary.SummaryDate &&
                            s.ServiceType == onlineSummary.ServiceType);

                    if (localSummary == null || localSummary.SummaryLastModified < onlineSummary.SummaryLastModified)
                    {
                        var trackedInLocal = _localContext.ChangeTracker
                       .Entries<ChurchAttendanceSummary>()
                       .FirstOrDefault(e =>
                           e.Entity.SummaryDate == onlineSummary.SummaryDate &&
                           e.Entity.ServiceType == onlineSummary.ServiceType);
                        if (trackedInLocal != null)
                        {
                            trackedInLocal.State = EntityState.Detached;
                        }

                        if (localSummary == null)
                        {
                            _localContext.ChurchAttendanceSummaries.Add(onlineSummary);
                        }
                        else
                        {
                            //_localContext.Entry(localSummary).CurrentValues.SetValues(onlineSummary);
                            _localContext.ChurchAttendanceSummaries.Attach(onlineSummary);
                            _localContext.Entry(onlineSummary).State = EntityState.Modified;
                        }

                        _localContext.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogInformation($"Failed to pull summary {onlineSummary.SummaryDate}-{onlineSummary.ServiceType}: {ex.Message}");
                    continue;
                }
            }
            //MessageBox.Show("Online Summaries Pulled", "Syncing", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // Helper Methods
        private DateTime LoadLastSyncTime()
        {
            // Load the last sync time from the local context
            //MessageBox.Show("Loading Last Sync Time", "Syncing", MessageBoxButton.OK, MessageBoxImage.Information);
            var syncMetadata = _localContext.SyncMetadata.FirstOrDefault(sm => sm.Key == "LastSyncTime");
            //if (syncMetadata != null && DateTime.TryParse(syncMetadata.Value, out DateTime lastSyncTime))
            //{
            //    return lastSyncTime;

            //}
            //return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            if (syncMetadata != null)
            {
                // Debug: Show raw SyncMetadata value
                //MessageBox.Show($"SyncMetadata Value: {syncMetadata.Value}", "Debug");
                if (DateTime.TryParse(syncMetadata.Value, out DateTime lastSyncTime))
                {
                    // Debug: Show parsed lastSyncTime
                    //MessageBox.Show($"Parsed lastSyncTime: {lastSyncTime}", "Debug");
                    return lastSyncTime;
                }
                else
                {
                    // Debug: Indicate parsing failure
                    //MessageBox.Show("Failed to parse SyncMetadata Value", "Debug");
                }
            }
            else
            {
                // Debug: Indicate no metadata found
                //MessageBox.Show("No SyncMetadata found for LastSyncTime", "Debug");
            }
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        }

        private void SaveLastSyncTime(DateTime time)
        {
            // Save the last sync time to the local context
            //MessageBox.Show("Saving Last Sync Time", "Syncing", MessageBoxButton.OK, MessageBoxImage.Information);
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
            //MessageBox.Show("Last Sync Time Saved", "Syncing", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}