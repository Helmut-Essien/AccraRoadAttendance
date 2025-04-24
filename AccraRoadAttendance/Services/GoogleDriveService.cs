using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccraRoadAttendance.Services
{
    public class GoogleDriveService
    {
        private readonly DriveService _driveService;
        private readonly string _profilePicturesFolderId;


        public GoogleDriveService()
        {
            //var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
            //    new ClientSecrets { ClientId = "YOUR_CLIENT_ID", ClientSecret = "YOUR_CLIENT_SECRET" },
            //    new[] { DriveService.Scope.Drive },
            //    "user",
            //    CancellationToken.None).Result;

            //_driveService = new DriveService(new BaseClientService.Initializer()
            //{
            //    HttpClientInitializer = credential,
            //    ApplicationName = "AccraRoadAttendance"
            //});
            // Load Service Account credentials
            var credential = GoogleCredential.FromFile("Resources/service-account-key.json")
                .CreateScoped(DriveService.Scope.Drive);

            _driveService = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "AccraRoadAttendance"
            });

            // Set the ProfilePictures folder ID (from the shared folder in calypsotumbler1@gmail.com)
            _profilePicturesFolderId = "1gN_Hhie-bN7FGIm_MN3DQ1QZF95eHRjR"; // Replace with actual folder ID

        }

        public string UploadImage(string localPath)
        {
            try
            {
                if (!File.Exists(localPath))
                    throw new FileNotFoundException($"Image file not found: {localPath}");

                var extension = Path.GetExtension(localPath).ToLower();
                var mimeType = extension switch
                {
                    ".jpg" => "image/jpeg",
                    ".jpeg" => "image/jpeg",
                    ".png" => "image/png",
                    _ => throw new NotSupportedException($"Unsupported image format: {extension}")
                };

                var fileMetadata = new Google.Apis.Drive.v3.Data.File
                {
                    Name = Path.GetFileName(localPath),
                    Parents = new[] { _profilePicturesFolderId }
                };

                using (var stream = new FileStream(localPath, FileMode.Open))
                {
                    var request = _driveService.Files.Create(fileMetadata, stream, mimeType);
                    request.Fields = "id";
                    request.Upload();
                    var file = request.ResponseBody;

                    // Set file to be publicly accessible
                    var permission = new Google.Apis.Drive.v3.Data.Permission { Type = "anyone", Role = "reader" };
                    _driveService.Permissions.Create(permission, file.Id).Execute();

                    return $"https://drive.google.com/uc?id={file.Id}";
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to upload image to Google Drive: {ex.Message}", ex);
            }
        }

        public string DownloadImage(string driveUrl)
        {
            try
            {
                var fileId = driveUrl.Split('=')[1];
                var request = _driveService.Files.Get(fileId);
                var stream = new MemoryStream();
                request.Download(stream);
                var localPath = Path.Combine("ProfilePictures", $"{fileId}.jpg");
                Directory.CreateDirectory("ProfilePictures");
                File.WriteAllBytes(localPath, stream.ToArray());
                return localPath;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to download image from Google Drive: {ex.Message}", ex);
            }
        }
    }
}

