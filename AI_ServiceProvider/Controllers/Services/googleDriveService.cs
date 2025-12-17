using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;

namespace AI_ServiceProvider.Controllers.Services
{
    public class GoogleDriveService:IGoogleDriveService
    {
        private readonly DriveService _driveService;

        public GoogleDriveService(IConfiguration configuration)
        {
            // Get the path to the credentials file from appsettings.json
            var credentialsPath = configuration["GoogleDriveSettings:CredentialsJsonPath"];

            // Authenticate using the service account credentials
            var credential = GoogleCredential.FromFile(credentialsPath)
                .CreateScoped(DriveService.Scope.DriveFile);

            // Create the DriveService instance
            _driveService = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "AI Image Parser API"
            });
        }

        // In GoogleDriveService.cs

        public async Task<string?> UploadFileAsync(IFormFile file, string folderId)
        {
            if (file == null || file.Length == 0)
            {
                return null;
            }

            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = file.FileName,
                Parents = new List<string> { folderId }
            };

            FilesResource.CreateMediaUpload request;
            using (var stream = file.OpenReadStream())
            {
                request = _driveService.Files.Create(fileMetadata, stream, file.ContentType);
                request.Fields = "id";

                // Perform the upload and get the progress result
                var progress = await request.UploadAsync();

                // --- THIS IS THE CRUCIAL CHECK ---
                if (progress.Status == Google.Apis.Upload.UploadStatus.Failed)
                {
                    // If the upload failed, throw an exception with the details.
                    // This is much better than a NullReferenceException.
                    throw new Exception($"Google Drive upload failed: {progress.Exception?.Message}");
                }
                // --- END OF CRUCIAL CHECK ---
            }

            // Now it's safe to get the response body
            var uploadedFile = request.ResponseBody;

            // It's still good practice to check if uploadedFile is not null, just in case
            if (uploadedFile == null)
            {
                throw new Exception("Google Drive upload failed: Response body was null after upload.");
            }

            // Make the file publicly accessible
            var permission = new Google.Apis.Drive.v3.Data.Permission()
            {
                Type = "anyone",
                Role = "reader"
            };
            await _driveService.Permissions.Create(permission, uploadedFile.Id).ExecuteAsync();

            return $"https://drive.google.com/uc?id={uploadedFile.Id}";
        }

    }
}
