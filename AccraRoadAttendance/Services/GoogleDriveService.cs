using Google;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
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

        public GoogleDriveService()
        {
            Console.WriteLine("Initializing GoogleDriveService...");
            try
            {
                //    var asm = Assembly.GetExecutingAssembly();

                //    var resourceName = "AccraRoadAttendance.Resources.service-account-key.json";
                //    // adjust namespace + folder
                //    using var stream = asm.GetManifestResourceStream(resourceName)
                //                    ?? throw new FileNotFoundException(resourceName);
                //    var credential = GoogleCredential.FromStream(stream)
                //                       .CreateScoped(DriveService.Scope.Drive);

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

            //    //Load Service Account credentials
            //    var credential = GoogleCredential.FromFile("Resources/service-account-key.json")
            //        .CreateScoped(DriveService.Scope.Drive);
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
                MessageBox.Show($"ProfilePictures folder ID set to: {_profilePicturesFolderId}");
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
            MessageBox.Show($"Starting image upload for file: {localPath}");
            try
            {
                CheckFolderExists(_profilePicturesFolderId);
                ListFilesInFolder(_profilePicturesFolderId);

                if (!File.Exists(localPath))
                {
                    Console.WriteLine($"File not found: {localPath}");
                    throw new FileNotFoundException($"Image file not found: {localPath}");
                }
                Console.WriteLine("File exists. Checking file extension...");
                MessageBox.Show("File exists. Checking file extension...");


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
                Console.WriteLine($"File extension: {extension}, MIME type: {mimeType}");

                var fileMetadata = new Google.Apis.Drive.v3.Data.File
                {
                    Name = fileName,
                    Parents = new[] { _profilePicturesFolderId }
                };
                Console.WriteLine($"File metadata created. File name: {fileMetadata.Name}, Parent folder ID: {_profilePicturesFolderId}");
                MessageBox.Show($"File metadata created. File name: {fileMetadata.Name}, Parent folder ID: {_profilePicturesFolderId}");

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
                    Console.WriteLine("Upload failed: No file metadata returned.");
                    throw new Exception("Failed to upload file: ResponseBody is null");
                }
                Console.WriteLine($"File uploaded successfully. File ID: {file.Id}");
                


                //Set file to be publicly accessible
                Console.WriteLine("Setting file permissions to public (anyone, reader)...");
                var permission = new Google.Apis.Drive.v3.Data.Permission { Type = "anyone", Role = "reader" };
                _driveService.Permissions.Create(permission, file.Id).Execute();
                Console.WriteLine("Permissions set successfully.");

                var publicUrl = $"https://drive.google.com/uc?id={file.Id}";
                Console.WriteLine($"Public URL generated: {publicUrl}");
                return publicUrl;
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


        //DEBUGGING GOOGLE DRIVE ACCESSIBILITY
        public void ListFilesInFolder(string folderId)
        {
            var listRequest = _driveService.Files.List();
            listRequest.Q = $"'{folderId}' in parents";
            listRequest.Fields = "files(id, name)";
            try
            {
                var files = listRequest.Execute().Files;
                Console.WriteLine("Files in folder:");
                foreach (var file in files)
                {
                    Console.WriteLine($"{file.Name} ({file.Id})");
                    MessageBox.Show($"{file.Name} ({file.Id})");
                }
            }
            catch (GoogleApiException ex)
            {
                Console.WriteLine("Error listing files: " + ex.Message);
                MessageBox.Show("Error listing files: " + ex.Message);
            }
        }

        public void CheckFolderExists(string folderId)
        {
            try
            {
                var getRequest = _driveService.Files.Get(folderId);
                getRequest.Fields = "id, name";
                var folder = getRequest.Execute();
                Console.WriteLine($"Folder exists: {folder.Name} ({folder.Id})");
                MessageBox.Show($"Folder exists: {folder.Name} ({folder.Id})");
            }
            catch (GoogleApiException ex)
            {
                if (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    Console.WriteLine("Folder not found.");
                   MessageBox.Show("Folder not found.");
                }
                else
                {
                    Console.WriteLine("Error checking folder: " + ex.Message);
                    MessageBox.Show("Error checking folder: " + ex.Message);
                }
            }
        }


        private static byte[] DecryptAes256_OpenSsl(byte[] encData, string pass)
        {
            // encData format:  "Salted__" (8) | salt (8) | ciphertext
            if (encData.Length < 16 || Encoding.ASCII.GetString(encData, 0, 8) != "Salted__")
                throw new InvalidDataException("Not an OpenSSL-salted file.");

            byte[] salt = new byte[8];
            Array.Copy(encData, 8, salt, 0, 8);
            byte[] cipherText = new byte[encData.Length - 16];
            Array.Copy(encData, 16, cipherText, 0, cipherText.Length);

            // Derive key + IV with PBKDF2 (matches `-pbkdf2` flag)
            //using var kdf = new Rfc2898DeriveBytes(pass, salt, 10000, HashAlgorithmName.SHA256);
            //byte[] key = kdf.GetBytes(32);   // 256-bit key
            //byte[] iv = kdf.GetBytes(16);   // 128-bit IV

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

            //using var decryptor = aes.CreateDecryptor();
            //using var msPlain = new MemoryStream();
            //using (var cs = new CryptoStream(msPlain, decryptor, CryptoStreamMode.Write))
            //{
            //    cs.Write(cipherText, 0, cipherText.Length);
            //    //cs.FlushFinalBlock();
            //}
            //return msPlain.ToArray();
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