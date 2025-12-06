namespace AI_ServiceProvider.Controllers.Services
{
    public class GoogleDriveService:IGoogleDriveService
    {
        public Task<string?> UploadFileAsync(IFormFile file, string folderId)
        {
            return Task.FromResult<string?>(null);
        }
    }
}
