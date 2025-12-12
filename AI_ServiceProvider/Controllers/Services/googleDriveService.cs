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

        public async Task<string?> UploadFileAsync(IFormFile file, string folderId)
        {
            if (file == null || file.Length == 0)
            {
                return null;
            }

            // Define metadata for the file to be uploaded
            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = file.FileName,
                Parents = new List<string> { folderId } // Specify the folder to upload to
            };

            // Create the upload request
            FilesResource.CreateMediaUpload request;
            using (var stream = file.OpenReadStream())
            {
                request = _driveService.Files.Create(fileMetadata, stream, file.ContentType);
                request.Fields = "id"; // We only need the file ID back
                await request.UploadAsync();
            }

            var uploadedFile = request.ResponseBody;

            // Make the file publicly accessible so anyone with the link can view it
            var permission = new Google.Apis.Drive.v3.Data.Permission()
            {
                Type = "anyone",
                Role = "reader"
            };
            await _driveService.Permissions.Create(permission, uploadedFile.Id).ExecuteAsync();

            // Return the public link with the  drive ID to the file
            return $"https://drive.google.com/uc?id={uploadedFile.Id}";
        }

    }
}
