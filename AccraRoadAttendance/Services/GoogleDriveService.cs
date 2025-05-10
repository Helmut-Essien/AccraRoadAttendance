using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using System;
using System.IO;
using System.Reflection;

namespace AccraRoadAttendance.Services
{
    public class GoogleDriveService
    {
        private readonly DriveService _driveService;
        private readonly string _profilePicturesFolderId;

        public GoogleDriveService()
        {
            Console.WriteLine("Initializing GoogleDriveService...");
            try
            {
                var asm = Assembly.GetExecutingAssembly();
                foreach (var name in asm.GetManifestResourceNames())
                    Console.WriteLine(name);
                var resourceName = "AccraRoadAttendance.Resources.service-account-key.json";
                // adjust namespace + folder
                using var stream = asm.GetManifestResourceStream(resourceName)
                                ?? throw new FileNotFoundException(resourceName);
                var credential = GoogleCredential.FromStream(stream)
                                   .CreateScoped(DriveService.Scope.Drive);

                // Load Service Account credentials
                //var credential = GoogleCredential.FromFile("Resources/service-account-key.json")
                //    .CreateScoped(DriveService.Scope.Drive);
                //Console.WriteLine("Service account credentials loaded successfully.");

                _driveService = new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "AccraRoadAttendance"
                });
                Console.WriteLine("DriveService initialized.");

                // Set the ProfilePictures folder ID
                _profilePicturesFolderId = "1gN_Hhie-bN7FGIm_MN3DQ1QZF95eHRjR";
                Console.WriteLine($"ProfilePictures folder ID set to: {_profilePicturesFolderId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing GoogleDriveService: {ex.Message}");
                throw;
            }
        }

        public string UploadImage(string localPath)
        {
            Console.WriteLine($"Starting image upload for file: {localPath}");
            try
            {
                if (!File.Exists(localPath))
                {
                    Console.WriteLine($"File not found: {localPath}");
                    throw new FileNotFoundException($"Image file not found: {localPath}");
                }
                Console.WriteLine("File exists. Checking file extension...");

                var extension = Path.GetExtension(localPath).ToLower();
                var mimeType = extension switch
                {
                    ".jpg" => "image/jpeg",
                    ".jpeg" => "image/jpeg",
                    ".png" => "image/png",
                    _ => throw new NotSupportedException($"Unsupported image format: {extension}")
                };
                Console.WriteLine($"File extension: {extension}, MIME type: {mimeType}");

                var fileMetadata = new Google.Apis.Drive.v3.Data.File
                {
                    Name = Path.GetFileName(localPath),
                    Parents = new[] { _profilePicturesFolderId }
                };
                Console.WriteLine($"File metadata created. File name: {fileMetadata.Name}, Parent folder ID: {_profilePicturesFolderId}");

                using (var stream = new FileStream(localPath, FileMode.Open))
                {
                    Console.WriteLine("Uploading file to Google Drive...");
                    var request = _driveService.Files.Create(fileMetadata, stream, mimeType);
                    request.Fields = "id";
                    request.Upload();
                    var file = request.ResponseBody;
                    Console.WriteLine($"File uploaded successfully. File ID: {file.Id}");

                    // Set file to be publicly accessible
                    Console.WriteLine("Setting file permissions to public (anyone, reader)...");
                    var permission = new Google.Apis.Drive.v3.Data.Permission { Type = "anyone", Role = "reader" };
                    _driveService.Permissions.Create(permission, file.Id).Execute();
                    Console.WriteLine("Permissions set successfully.");

                    var publicUrl = $"https://drive.google.com/uc?id={file.Id}";
                    Console.WriteLine($"Public URL generated: {publicUrl}");
                    return publicUrl;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading image to Google Drive: {ex.Message}");
                throw new Exception($"Failed to upload image to Google Drive: {ex.Message}", ex);
            }
        }

        public string DownloadImage(string driveUrl)
        {
            Console.WriteLine($"Starting image download from URL: {driveUrl}");
            try
            {
                var fileId = driveUrl.Split('=')[1];
                Console.WriteLine($"Extracted file ID: {fileId}");

                var request = _driveService.Files.Get(fileId);
                var stream = new MemoryStream();
                Console.WriteLine("Downloading file from Google Drive...");
                request.Download(stream);
                Console.WriteLine("File downloaded successfully.");

                var localPath = Path.Combine("ProfilePictures", $"{fileId}.jpg");
                Console.WriteLine($"Saving file to local path: {localPath}");

                Directory.CreateDirectory("ProfilePictures");
                File.WriteAllBytes(localPath, stream.ToArray());
                Console.WriteLine("File saved locally.");

                return localPath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error downloading image from Google Drive: {ex.Message}");
                throw new Exception($"Failed to download image from Google Drive: {ex.Message}", ex);
            }
        }
    }
}