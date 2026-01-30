using Google;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Windows;

namespace AccraRoadAttendance.Services
{
    public class GoogleDriveService
    {
        private readonly DriveService _driveService;
        private readonly string _profilePicturesFolderId;
        private readonly ILogger<GoogleDriveService> _logger;

        public GoogleDriveService(ILogger<GoogleDriveService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _logger.LogInformation("Initializing GoogleDriveService...");
            try
            {


                // 1)  Locate encrypted blob
                const string resourceName =
                    "AccraRoadAttendance.Resources.service_key.enc";   // adjust if needed
                using var encStream = Assembly.GetExecutingAssembly()
                                     .GetManifestResourceStream(resourceName)
                                     ?? throw new FileNotFoundException(resourceName);

                // 2)  Read encrypted bytes
                using var msEnc = new MemoryStream();
                encStream.CopyTo(msEnc);
                byte[] encBytes = msEnc.ToArray();

                // 3)  Decrypt
                string passphrase = "A$$hole123";


                byte[] plainBytes = DecryptAes256_OpenSsl(encBytes, passphrase);
                using var jsonStream = new MemoryStream(plainBytes);

                // 4)  Build credential
                var credential = GoogleCredential.FromStream(jsonStream)
                                                 .CreateScoped(DriveService.Scope.Drive);



                _driveService = new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "AccraRoadAttendance"
                });
                _logger.LogInformation("DriveService initialized.");

                // Set the ProfilePictures folder ID
                _profilePicturesFolderId = "1SdchL1SWS1T-e5KWCMAxw89q4v1JkWd9";
                Console.WriteLine($"ProfilePictures folder ID set to: {_profilePicturesFolderId}");
                //MessageBox.Show($"ProfilePictures folder ID set to: {_profilePicturesFolderId}");
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Error initializing GoogleDriveService: {ex.Message}");
                throw;
            }
        }

        public string UploadImage(string localPath)
        {

            _logger.LogInformation($"Starting image upload for file: {localPath}");
            //MessageBox.Show($"Starting image upload for file: {localPath}");
            try
            {
                //CheckFolderExists(_profilePicturesFolderId);
                //ListFilesInFolder(_profilePicturesFolderId);

                if (!File.Exists(localPath))
                {
                    _logger.LogInformation($"File not found: {localPath}");
                    throw new FileNotFoundException($"Image file not found: {localPath}");
                }
                _logger.LogInformation("File exists. Checking file extension...");
                //MessageBox.Show("File exists. Checking file extension...");


                // 1) Read into memory (this closes the file immediately)
                byte[] fileBytes = File.ReadAllBytes(localPath);
                string fileName = Path.GetFileName(localPath);
                string extension = Path.GetExtension(localPath).ToLower();
                string mimeType = extension switch
                {
                    ".jpg" => "image/jpeg",
                    ".jpeg" => "image/jpeg",
                    ".png" => "image/png",
                    _ => throw new NotSupportedException($"Unsupported image format: {extension}")
                };
                _logger.LogInformation($"File extension: {extension}, MIME type: {mimeType}");
                //MessageBox.Show($"File extension: {extension}, MIME type: {mimeType}");

                var fileMetadata = new Google.Apis.Drive.v3.Data.File
                {
                    Name = fileName,
                    Parents = new[] { _profilePicturesFolderId }
                };
                Console.WriteLine($"File metadata created. File name: {fileMetadata.Name}, Parent folder ID: {_profilePicturesFolderId}");
                //MessageBox.Show($"File metadata created. File name: {fileMetadata.Name}, Parent folder ID: {_profilePicturesFolderId}");

                //using (var stream = new FileStream(localPath, FileMode.Open))
                //{
                //    Console.WriteLine("Uploading file to Google Drive...");
                //    var request = _driveService.Files.Create(fileMetadata, stream, mimeType);
                //    request.Fields = "id";
                //    request.Upload();
                //    var file = request.ResponseBody;
                //    Console.WriteLine($"File uploaded successfully. File ID: {file.Id}");

                //    // Set file to be publicly accessible
                //    Console.WriteLine("Setting file permissions to public (anyone, reader)...");
                //    var permission = new Google.Apis.Drive.v3.Data.Permission { Type = "anyone", Role = "reader" };
                //    _driveService.Permissions.Create(permission, file.Id).Execute();
                //    Console.WriteLine("Permissions set successfully.");

                //    var publicUrl = $"https://drive.google.com/uc?id={file.Id}";
                //    Console.WriteLine($"Public URL generated: {publicUrl}");
                //    return publicUrl;
                //}
                using var memStream = new MemoryStream(fileBytes);
                var request = _driveService.Files.Create(fileMetadata, memStream, mimeType);
                request.Fields = "id";
                request.Upload();
                var file = request.ResponseBody;
                if (file == null)
                {
                    _logger.LogInformation("Upload failed: No file metadata returned.");
                    throw new Exception("Failed to upload file: ResponseBody is null");
                }
                _logger.LogInformation($"File uploaded successfully. File ID: {file.Id}");



                //Set file to be publicly accessible
                Console.WriteLine("Setting file permissions to public (anyone, reader)...");
                var permission = new Google.Apis.Drive.v3.Data.Permission { Type = "anyone", Role = "reader" };
                _driveService.Permissions.Create(permission, file.Id).Execute();
                Console.WriteLine("Permissions set successfully.");

                var publicUrl = $"https://drive.google.com/uc?id={file.Id}";
                _logger.LogInformation($"Public URL generated: {publicUrl}");
                return publicUrl;
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Error uploading image to Google Drive: {ex.Message}");
                throw new Exception($"Failed to upload image to Google Drive: {ex.Message}", ex);
            }
        }

        public string DownloadImage(string driveUrl)
        {
            _logger.LogInformation($"Starting image download from URL: {driveUrl}");
            try
            {
                var fileId = driveUrl.Split('=')[1];
                _logger.LogInformation($"Extracted file ID: {fileId}");

                //var request = _driveService.Files.Get(fileId);
                //var stream = new MemoryStream();
                //Console.WriteLine("Downloading file from Google Drive...");
                //request.Download(stream);
                //Console.WriteLine("File downloaded successfully.");

                //var localPath = Path.Combine("ProfilePictures", $"{fileId}.jpg");
                //Console.WriteLine($"Saving file to local path: {localPath}");

                //Directory.CreateDirectory("ProfilePictures");
                //File.WriteAllBytes(localPath, stream.ToArray());
                //Console.WriteLine("File saved locally.");
                // Get file metadata to determine MIME type
                var metadataRequest = _driveService.Files.Get(fileId);
                metadataRequest.Fields = "id, mimeType";
                var file = metadataRequest.Execute();
                string mimeType = file.MimeType;

                // Map MIME type to extension
                string extension = mimeType switch
                {
                    "image/jpeg" => ".jpg",
                    "image/png" => ".png",
                    _ => ".jpg" // Default to .jpg if unknown
                };


                string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string localPath = Path.Combine(appDataPath, "AccraRoadAttendance", "ProfilePictures", $"{fileId}{extension}");

                string? directoryPath = Path.GetDirectoryName(localPath);
                if (directoryPath is not null)
                {
                    Directory.CreateDirectory(directoryPath);
                }

                // Check if file already exists locally
                if (File.Exists(localPath))
                {
                    _logger.LogInformation($"File already exists at {localPath}; skipping download.");
                    return localPath;
                }

                // Download the file
                var request = _driveService.Files.Get(fileId);
                using var stream = new MemoryStream();
                request.Download(stream);
                _logger.LogInformation("File downloaded successfully.");

                File.WriteAllBytes(localPath, stream.ToArray());
                _logger.LogInformation($"File saved to: {localPath}");
                return localPath;
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Error downloading image from Google Drive: {ex.Message}");
                throw new Exception($"Failed to download image from Google Drive: {ex.Message}", ex);
            }
        }


        //DEBUGGING GOOGLE DRIVE ACCESSIBILITY
        //public void ListFilesInFolder(string folderId)
        //{
        //    var listRequest = _driveService.Files.List();
        //    listRequest.Q = $"'{folderId}' in parents";
        //    listRequest.Fields = "files(id, name)";
        //    try
        //    {
        //        var files = listRequest.Execute().Files;
        //        Console.WriteLine("Files in folder:");
        //        foreach (var file in files)
        //        {
        //            Console.WriteLine($"{file.Name} ({file.Id})");
        //            MessageBox.Show($"{file.Name} ({file.Id})");
        //        }
        //    }
        //    catch (GoogleApiException ex)
        //    {
        //        Console.WriteLine("Error listing files: " + ex.Message);
        //        MessageBox.Show("Error listing files: " + ex.Message);
        //    }
        //}

        //public void CheckFolderExists(string folderId)
        //{
        //    try
        //    {
        //        var getRequest = _driveService.Files.Get(folderId);
        //        getRequest.Fields = "id, name";
        //        var folder = getRequest.Execute();
        //        Console.WriteLine($"Folder exists: {folder.Name} ({folder.Id})");
        //        MessageBox.Show($"Folder exists: {folder.Name} ({folder.Id})");
        //    }
        //    catch (GoogleApiException ex)
        //    {
        //        if (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
        //        {
        //            Console.WriteLine("Folder not found.");
        //           MessageBox.Show("Folder not found.");
        //        }
        //        else
        //        {
        //            Console.WriteLine("Error checking folder: " + ex.Message);
        //            MessageBox.Show("Error checking folder: " + ex.Message);
        //        }
        //    }
        //}


        private static byte[] DecryptAes256_OpenSsl(byte[] encData, string pass)
        {
            // encData format:  "Salted__" (8) | salt (8) | ciphertext
            if (encData.Length < 16 || Encoding.ASCII.GetString(encData, 0, 8) != "Salted__")
                throw new InvalidDataException("Not an OpenSSL-salted file.");

            byte[] salt = new byte[8];
            Array.Copy(encData, 8, salt, 0, 8);
            byte[] cipherText = new byte[encData.Length - 16];
            Array.Copy(encData, 16, cipherText, 0, cipherText.Length);



            using var kdf = new Rfc2898DeriveBytes(pass, salt, 10000, HashAlgorithmName.SHA256);
            byte[] keyIv = kdf.GetBytes(48); // 32 bytes key + 16 bytes IV
            byte[] key = keyIv.Take(32).ToArray();
            byte[] iv = keyIv.Skip(32).Take(16).ToArray();

            using var aes = Aes.Create();
            aes.KeySize = 256;
            aes.BlockSize = 128;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = key;
            aes.IV = iv;


            using var decryptor = aes.CreateDecryptor();
            try
            {
                return decryptor.TransformFinalBlock(cipherText, 0, cipherText.Length);
            }
            catch (CryptographicException ex)
            {
                throw new InvalidDataException(ex.Message);
            }
        }
    }
}



//using Google;
//using Google.Apis.Auth.OAuth2;
//using Google.Apis.Drive.v3;
//using Google.Apis.Services;
//using Google.Apis.Util.Store;
//using Microsoft.Extensions.Logging;
//using System;
//using System.IO;
//using System.Reflection;
//using System.Security.Cryptography;
//using System.Text;
//using System.Windows;

//namespace AccraRoadAttendance.Services
//{
//    public class GoogleDriveService
//    {
//        private  DriveService _driveService;
//        private  string _profilePicturesFolderId;
//        private readonly ILogger<GoogleDriveService> _logger;

//        public static async Task<GoogleDriveService> CreateAsync(ILogger<GoogleDriveService> logger)
//        {
//            var instance = new GoogleDriveService(logger);
//            await instance.InitializeAsync();
//            return instance;
//        }

//        private GoogleDriveService(ILogger<GoogleDriveService> logger)
//        {
//            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
//        }

//        private async Task InitializeAsync()
//        {
//            _logger.LogInformation("Initializing GoogleDriveService (OAuth user flow)...");

//            try
//            {
//                // 1. Locate and decrypt client_secret.enc
//                const string resourceName = "AccraRoadAttendance.Resources.client_secret.enc";
//                using var encStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName)
//                    ?? throw new FileNotFoundException($"Embedded resource not found: {resourceName}");

//                using var msEnc = new MemoryStream();
//                await encStream.CopyToAsync(msEnc);
//                byte[] encBytes = msEnc.ToArray();

//                string passphrase = "A$$hole123"; // ← Consider making this configurable / more secure in real production
//                byte[] plainBytes = DecryptAes256_OpenSsl(encBytes, passphrase);

//                using var jsonStream = new MemoryStream(plainBytes);

//                // 2. Define secure data store for tokens (DPAPI protected file)
//                var tokenFolder = Path.Combine(
//                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
//                    "AccraRoadAttendance", "OAuthTokens");
//                Directory.CreateDirectory(tokenFolder);

//                var dataStore = new SecureFileDataStore(tokenFolder);

//                // 3. Authorize (shows browser only first time)
//                string[] scopes = { DriveService.Scope.Drive }; // or DriveFile for narrower access

//                var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
//                    GoogleClientSecrets.FromStream(jsonStream).Secrets,
//                    scopes,
//                    "user",                             // user identifier (can be email or fixed string)
//                    CancellationToken.None,
//                    dataStore);

//                // 4. Build Drive service
//                _driveService = new DriveService(new BaseClientService.Initializer
//                {
//                    HttpClientInitializer = credential,
//                    ApplicationName = "AccraRoadAttendance"
//                });

//                _profilePicturesFolderId = "1SdchL1SWS1T-e5KWCMAxw89q4v1JkWd9";

//                // Optional: quick access test
//                await TestFolderAccessAsync(_profilePicturesFolderId);

//                _logger.LogInformation("GoogleDriveService initialized successfully (OAuth).");
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Failed to initialize Google Drive service");
//                MessageBox.Show($"Initialization failed:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
//                throw;
//            }
//        }



//        public string UploadImage(string localPath)
//        {
//            _logger.LogInformation($"Starting image upload for file: {localPath}");
//            MessageBox.Show($"DEBUG: UploadImage called with path: {localPath}");

//            try
//            {
//                if (!File.Exists(localPath))
//                {
//                    MessageBox.Show($"ERROR: File not found at path: {localPath}");
//                    _logger.LogInformation($"File not found: {localPath}");
//                    throw new FileNotFoundException($"Image file not found: {localPath}");
//                }

//                MessageBox.Show("DEBUG: File exists, reading bytes...");

//                // 1) Read into memory
//                byte[] fileBytes = File.ReadAllBytes(localPath);
//                MessageBox.Show($"DEBUG: Read {fileBytes.Length} bytes from file");

//                string fileName = Path.GetFileName(localPath);
//                string extension = Path.GetExtension(localPath).ToLower();

//                MessageBox.Show($"DEBUG: File name: {fileName}\nExtension: {extension}");

//                string mimeType = extension switch
//                {
//                    ".jpg" => "image/jpeg",
//                    ".jpeg" => "image/jpeg",
//                    ".png" => "image/png",
//                    _ => throw new NotSupportedException($"Unsupported image format: {extension}")
//                };

//                MessageBox.Show($"DEBUG: MIME type determined: {mimeType}");

//                var fileMetadata = new Google.Apis.Drive.v3.Data.File
//                {
//                    Name = fileName,
//                    Parents = new[] { _profilePicturesFolderId }
//                };

//                MessageBox.Show($"DEBUG: File metadata created\nName: {fileMetadata.Name}\nParent folder: {_profilePicturesFolderId}\n\nStarting upload...");

//                using var memStream = new MemoryStream(fileBytes);
//                var request = _driveService.Files.Create(fileMetadata, memStream, mimeType);
//                request.Fields = "id, name, webViewLink, webContentLink";

//                MessageBox.Show("DEBUG: Upload request created. Calling Upload()...");

//                // Check the upload progress
//                var uploadProgress = request.Upload();

//                MessageBox.Show($"DEBUG: Upload() returned.\nStatus: {uploadProgress.Status}\n" +
//                    $"Bytes Sent: {uploadProgress.BytesSent}\n" +
//                    $"Exception: {uploadProgress.Exception?.Message ?? "None"}");

//                if (uploadProgress.Status == Google.Apis.Upload.UploadStatus.Failed)
//                {
//                    MessageBox.Show($"ERROR: Upload failed!\nException: {uploadProgress.Exception?.Message}\n" +
//                        $"Inner: {uploadProgress.Exception?.InnerException?.Message}");
//                    throw new Exception($"Upload failed: {uploadProgress.Exception?.Message}", uploadProgress.Exception);
//                }

//                if (uploadProgress.Exception != null)
//                {
//                    MessageBox.Show($"ERROR: Upload had exception even though status is {uploadProgress.Status}\n" +
//                        $"Exception: {uploadProgress.Exception.Message}\n" +
//                        $"Inner: {uploadProgress.Exception.InnerException?.Message}");
//                    throw uploadProgress.Exception;
//                }

//                var file = request.ResponseBody;

//                MessageBox.Show($"DEBUG: Checking ResponseBody...\nResponseBody is null: {file == null}");

//                if (file == null)
//                {
//                    MessageBox.Show("ERROR: Upload completed but ResponseBody is null!\n" +
//                        "This usually means:\n" +
//                        "1. Service account lacks permission to write to folder\n" +
//                        "2. Folder doesn't exist\n" +
//                        "3. API quota exceeded\n\n" +
//                        "Check that your service account email has 'Editor' or 'Writer' access to the folder!");
//                    _logger.LogInformation("Upload failed: No file metadata returned.");
//                    throw new Exception("Failed to upload file: ResponseBody is null. Service account may lack write permission to folder.");
//                }

//                MessageBox.Show($"DEBUG: ✓ Upload SUCCESS!\nFile ID: {file.Id}\nFile Name: {file.Name}");
//                _logger.LogInformation($"File uploaded successfully. File ID: {file.Id}");

//                // Set file to be publicly accessible
//                MessageBox.Show("DEBUG: Setting file permissions to public...");

//                try
//                {
//                    var permission = new Google.Apis.Drive.v3.Data.Permission
//                    {
//                        Type = "anyone",
//                        Role = "reader"
//                    };

//                    _driveService.Permissions.Create(permission, file.Id).Execute();
//                    MessageBox.Show("DEBUG: ✓ Permissions set successfully");
//                }
//                catch (GoogleApiException permEx)
//                {
//                    MessageBox.Show($"WARNING: Could not set public permissions!\n" +
//                        $"Status: {permEx.HttpStatusCode}\n" +
//                        $"Message: {permEx.Message}\n\n" +
//                        $"File uploaded but may not be publicly accessible.");
//                }

//                var publicUrl = $"https://drive.google.com/uc?id={file.Id}";
//                MessageBox.Show($"DEBUG: ✓ Upload complete!\nPublic URL: {publicUrl}");

//                _logger.LogInformation($"Public URL generated: {publicUrl}");
//                return publicUrl;
//            }
//            catch (GoogleApiException ex)
//            {
//                MessageBox.Show($"ERROR: Google API Exception during upload!\n" +
//                    $"Status Code: {ex.HttpStatusCode}\n" +
//                    $"Message: {ex.Message}\n" +
//                    $"Error Code: {ex.Error?.Code}\n" +
//                    $"Error Message: {ex.Error?.Message}\n\n" +
//                    $"Errors:\n{string.Join("\n", ex.Error?.Errors?.Select(e => $"- {e.Reason}: {e.Message}") ?? new[] { "None" })}");
//                _logger.LogInformation($"Google API error uploading image: {ex.Message}");
//                throw new Exception($"Failed to upload image to Google Drive: {ex.Message}", ex);
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show($"ERROR during upload:\n{ex.GetType().Name}\n{ex.Message}\n\nStack:\n{ex.StackTrace}");
//                _logger.LogInformation($"Error uploading image to Google Drive: {ex.Message}");
//                throw new Exception($"Failed to upload image to Google Drive: {ex.Message}", ex);
//            }
//        }





//        public string DownloadImage(string driveUrl)
//        {
//            MessageBox.Show($"Starting image download from URL: {driveUrl}", "DEBUG");
//            _logger.LogInformation($"Starting image download from URL: {driveUrl}");

//            try
//            {
//                var fileId = driveUrl.Split('=')[1];
//                MessageBox.Show($"Extracted file ID: {fileId}", "DEBUG");
//                _logger.LogInformation($"Extracted file ID: {fileId}");

//                // Get file metadata
//                MessageBox.Show("Getting file metadata...", "DEBUG");
//                var metadataRequest = _driveService.Files.Get(fileId);
//                metadataRequest.Fields = "id, mimeType";
//                var file = metadataRequest.Execute();
//                string mimeType = file.MimeType;
//                MessageBox.Show($"File MIME type: {mimeType}", "DEBUG");

//                // Map MIME type to extension
//                string extension = mimeType switch
//                {
//                    "image/jpeg" => ".jpg",
//                    "image/png" => ".png",
//                    _ => ".jpg"
//                };

//                MessageBox.Show($"File extension determined: {extension}", "DEBUG");

//                string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
//                string localPath = Path.Combine(appDataPath, "AccraRoadAttendance", "ProfilePictures", $"{fileId}{extension}");

//                MessageBox.Show($"Local save path: {localPath}", "DEBUG");

//                string? directoryPath = Path.GetDirectoryName(localPath);
//                if (directoryPath is not null)
//                {
//                    Directory.CreateDirectory(directoryPath);
//                    MessageBox.Show($"Directory created: {directoryPath}", "DEBUG");
//                }

//                // Check if file already exists locally
//                if (File.Exists(localPath))
//                {
//                    MessageBox.Show($"File already exists at {localPath}; skipping download.", "DEBUG");
//                    _logger.LogInformation($"File already exists at {localPath}; skipping download.");
//                    return localPath;
//                }

//                // Download the file
//                MessageBox.Show("Starting file download...", "DEBUG");
//                var request = _driveService.Files.Get(fileId);
//                using var stream = new MemoryStream();
//                request.Download(stream);
//                MessageBox.Show($"File downloaded. Size: {stream.Length} bytes", "DEBUG");
//                _logger.LogInformation("File downloaded successfully.");

//                File.WriteAllBytes(localPath, stream.ToArray());
//                MessageBox.Show($"File saved to: {localPath}", "DEBUG");
//                _logger.LogInformation($"File saved to: {localPath}");

//                MessageBox.Show("Image download COMPLETE!", "DEBUG");
//                return localPath;
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show($"ERROR downloading image: {ex.Message}\n\nStack Trace: {ex.StackTrace}", "DEBUG");
//                _logger.LogInformation($"Error downloading image from Google Drive: {ex.Message}");
//                throw new Exception($"Failed to download image from Google Drive: {ex.Message}", ex);
//            }
//        }

//        // Test folder access method
//        private async Task TestFolderAccessAsync(string folderId)
//        {
//            try
//            {
//                var getRequest = _driveService.Files.Get(folderId);
//                getRequest.Fields = "id, name, mimeType";
//                var folder = await getRequest.ExecuteAsync();

//                _logger.LogInformation($"Folder access OK: {folder.Name} ({folder.Id})");
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, $"Cannot access folder {folderId}");
//                throw;
//            }
//        }

//        private static byte[] DecryptAes256_OpenSsl(byte[] encData, string pass)
//        {
//            MessageBox.Show("Starting AES decryption...", "DEBUG");

//            // encData format: "Salted__" (8) | salt (8) | ciphertext
//            if (encData.Length < 16 || Encoding.ASCII.GetString(encData, 0, 8) != "Salted__")
//            {
//                MessageBox.Show("ERROR: Not an OpenSSL-salted file or data too short", "DEBUG");
//                throw new InvalidDataException("Not an OpenSSL-salted file.");
//            }

//            byte[] salt = new byte[8];
//            Array.Copy(encData, 8, salt, 0, 8);
//            byte[] cipherText = new byte[encData.Length - 16];
//            Array.Copy(encData, 16, cipherText, 0, cipherText.Length);

//            MessageBox.Show($"Salt extracted, ciphertext length: {cipherText.Length}", "DEBUG");

//            using var kdf = new Rfc2898DeriveBytes(pass, salt, 10000, HashAlgorithmName.SHA256);
//            byte[] keyIv = kdf.GetBytes(48); // 32 bytes key + 16 bytes IV
//            byte[] key = keyIv.Take(32).ToArray();
//            byte[] iv = keyIv.Skip(32).Take(16).ToArray();

//            MessageBox.Show("Key and IV derived successfully", "DEBUG");

//            using var aes = Aes.Create();
//            aes.KeySize = 256;
//            aes.BlockSize = 128;
//            aes.Mode = CipherMode.CBC;
//            aes.Padding = PaddingMode.PKCS7;
//            aes.Key = key;
//            aes.IV = iv;

//            using var decryptor = aes.CreateDecryptor();
//            try
//            {
//                MessageBox.Show("Performing decryption transform...", "DEBUG");
//                byte[] result = decryptor.TransformFinalBlock(cipherText, 0, cipherText.Length);
//                MessageBox.Show($"Decryption successful. Result length: {result.Length}", "DEBUG");
//                return result;
//            }
//            catch (CryptographicException ex)
//            {
//                MessageBox.Show($"ERROR in decryption: {ex.Message}", "DEBUG");
//                throw new InvalidDataException(ex.Message);
//            }
//        }

//        // Simple DPAPI-protected file store (no extra NuGet)
//        internal class SecureFileDataStore : IDataStore
//        {
//            private readonly string _folder;
//            private static readonly byte[] Entropy;

//            static SecureFileDataStore()
//            {
//                Entropy = Encoding.UTF8.GetBytes("AccraRoadAttendanceEntropy2026"); // Change this string to something unique/long
//            }

//            public SecureFileDataStore(string folder)
//            {
//                _folder = folder ?? throw new ArgumentNullException(nameof(folder));
//            }

//            public Task StoreAsync<T>(string key, T value)
//            {
//                var path = Path.Combine(_folder, $"{key}.token");
//                var json = System.Text.Json.JsonSerializer.Serialize(value);
//                var bytes = Encoding.UTF8.GetBytes(json);
//                var protectedData = ProtectedData.Protect(bytes, Entropy, DataProtectionScope.CurrentUser);
//                File.WriteAllBytes(path, protectedData);
//                return Task.CompletedTask;
//            }

//            public Task<T> GetAsync<T>(string key)
//            {
//                var path = Path.Combine(_folder, $"{key}.token");
//                if (!File.Exists(path)) return Task.FromResult(default(T)!);

//                var protectedData = File.ReadAllBytes(path);
//                var bytes = ProtectedData.Unprotect(protectedData, Entropy, DataProtectionScope.CurrentUser);
//                var json = Encoding.UTF8.GetString(bytes);
//                var value = System.Text.Json.JsonSerializer.Deserialize<T>(json);
//                return Task.FromResult(value!);
//            }

//            public Task DeleteAsync<T>(string key)
//            {
//                var path = Path.Combine(_folder, $"{key}.token");
//                if (File.Exists(path)) File.Delete(path);
//                return Task.CompletedTask;
//            }

//            public Task ClearAsync()
//            {
//                foreach (var file in Directory.GetFiles(_folder, "*.token"))
//                    File.Delete(file);
//                return Task.CompletedTask;
//            }
//        }
//    }
//}