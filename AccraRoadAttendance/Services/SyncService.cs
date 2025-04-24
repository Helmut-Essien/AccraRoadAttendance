//using AccraRoadAttendance.Data;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace AccraRoadAttendance.Services
//{
//    public class SyncService
//    {
//        private readonly AttendanceDbContext _localContext;
//        private readonly AttendanceDbContext _onlineContext;
//        private readonly GoogleDriveService _googleDriveService;
//        private DateTime _lastSyncTime;

//        public SyncService(AttendanceDbContext localContext, AttendanceDbContext onlineContext, GoogleDriveService googleDriveService)
//        {
//            _localContext = localContext;
//            _onlineContext = onlineContext;
//            _googleDriveService = googleDriveService;
//            _lastSyncTime = LoadLastSyncTime(); // Load from a persistent store (e.g., file or DB)
//        }

//        public void SyncData()
//        {
//            PushLocalChanges();
//            PullOnlineChanges();
//            SaveLastSyncTime(DateTime.UtcNow);
//        }

//        private void PushLocalChanges()
//        {
//            // Sync Members
//            var localMembers = _localContext.Members.Where(m => !m.SyncStatus || m.LastModified > _lastSyncTime).ToList();
//            foreach (var member in localMembers)
//            {
//                var onlineMember = _onlineContext.Members.Find(member.Id);
//                if (onlineMember == null || onlineMember.LastModified < member.LastModified)
//                {
//                    if (onlineMember == null)
//                    {
//                        _onlineContext.Members.Add(member);
//                    }
//                    else
//                    {
//                        _onlineContext.Entry(onlineMember).CurrentValues.SetValues(member);
//                    }

//                    if (!string.IsNullOrEmpty(member.PicturePath) && !member.PicturePath.StartsWith("https://drive.google.com"))
//                    {
//                        var driveUrl = _googleDriveService.UploadImage(member.PicturePath);
//                        member.PicturePath = driveUrl;
//                    }

//                    _onlineContext.SaveChanges();
//                    member.SyncStatus = true;
//                }
//            }
//            _localContext.SaveChanges();

//            // Similar logic for Attendance and ChurchAttendanceSummary...
//        }

//        private void PullOnlineChanges()
//        {
//            // Sync Members
//            var onlineMembers = _onlineContext.Members.Where(m => m.LastModified > _lastSyncTime).ToList();
//            foreach (var onlineMember in onlineMembers)
//            {
//                var localMember = _localContext.Members.Find(onlineMember.Id);
//                if (localMember == null || localMember.LastModified < onlineMember.LastModified)
//                {
//                    if (localMember == null)
//                    {
//                        _localContext.Members.Add(onlineMember);
//                    }
//                    else
//                    {
//                        _localContext.Entry(localMember).CurrentValues.SetValues(onlineMember);
//                    }

//                    if (!string.IsNullOrEmpty(onlineMember.PicturePath) && onlineMember.PicturePath.StartsWith("https://drive.google.com"))
//                    {
//                        var localPath = _googleDriveService.DownloadImage(onlineMember.PicturePath);
//                        onlineMember.PicturePath = localPath;
//                    }

//                    _localContext.SaveChanges();
//                }
//            }

//            // Similar logic for Attendance and ChurchAttendanceSummary...
//        }

//        private DateTime LoadLastSyncTime()
//        {
//            // Implement loading from a file or database; default to a past date if not found
//            return DateTime.MinValue;
//        }

//        private void SaveLastSyncTime(DateTime time)
//        {
//            _lastSyncTime = time;
//            // Save to a file or database
//        }
//    }


//}
