using Google.Apis.Drive.v3;
using Google.Apis.Upload;
using InvoiceGenerator.Api.Models;
using InvoiceGenerator.Api.Services.Interfaces;
using Microsoft.Extensions.Options;


namespace InvoiceGenerator.Api.Services
{
    public class GoogleDriveUploader : IDriveUploader
    {
        private readonly DriveService _driveService;
        private readonly GoogleDriveSettings _driveSettings;
        private readonly ILogger<GoogleDriveUploader> _logger;

        public GoogleDriveUploader(
            DriveService driveService,
            IOptions<GoogleDriveSettings> driveSettings,
            ILogger<GoogleDriveUploader> logger)
        {
            _driveService = driveService;
            _driveSettings = driveSettings.Value;
            _logger = logger;
        }

        // public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string mimeType, string folderId)
        // {
        //     var fileMetadata = new Google.Apis.Drive.v3.Data.File
        //     {
        //         Name = fileName,
        //         Parents = new List<string> { folderId }
        //     };

        //     var request = _driveService.Files.Create(fileMetadata, fileStream, mimeType);
        //     request.Fields = "id";

        //     var result = await request.UploadAsync();

        //     if (result.Status == UploadStatus.Completed)
        //     {
        //         _logger.LogInformation("File uploaded: {FileName}", fileName);
        //         return request.ResponseBody.Id;
        //     }
        //     else
        //     {
        //         _logger.LogError("File upload failed: {FileName}", fileName);
        //         throw new Exception($"File upload failed: {result.Exception?.Message}");
        //     }
        // }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string mimeType, string folderId)
        {
            var fileMetadata = new Google.Apis.Drive.v3.Data.File
            {
                Name = fileName,
                Parents = new List<string> { folderId }
            };

            var request = _driveService.Files.Create(fileMetadata, fileStream, mimeType);
            request.Fields = "id, webViewLink";

            var result = await request.UploadAsync();

            if (result.Status != UploadStatus.Completed)
            {
                _logger.LogError("File upload failed: {FileName}", fileName);
                throw new Exception($"File upload failed: {result.Exception?.Message}");
            }

            var uploadedFile = request.ResponseBody;

            // Step 1: Make it public (anyone with the link can view)
            var permission = new Google.Apis.Drive.v3.Data.Permission
            {
                Type = "anyone",
                Role = "reader"
            };

            await _driveService.Permissions.Create(permission, uploadedFile.Id).ExecuteAsync();

            _logger.LogInformation("File uploaded and permission granted: {FileName}", fileName);

            // Step 2: Return the sharable URL
            return uploadedFile.WebViewLink;
        }
        public async Task<IList<Google.Apis.Drive.v3.Data.File>> ListFilesInFolderAsync(string folderId)
        {
            var request = _driveService.Files.List();
            request.Q = $"'{folderId}' in parents and trashed = false";
            request.Fields = "files(id, name)";

            var result = await request.ExecuteAsync();
            return result.Files;
        }

        public async Task MoveFileAsync(string fileId, string oldFolderId, string newFolderId)
        {
            // Get the file's existing parents
            var getRequest = _driveService.Files.Get(fileId);
            getRequest.Fields = "parents";
            var file = await getRequest.ExecuteAsync();

            var previousParents = string.Join(",", file.Parents);

            // Move the file
            var updateRequest = _driveService.Files.Update(new Google.Apis.Drive.v3.Data.File(), fileId);
            updateRequest.AddParents = newFolderId;
            updateRequest.RemoveParents = previousParents;
            updateRequest.Fields = "id, parents";

            await updateRequest.ExecuteAsync();
            _logger.LogInformation("File {FileId} moved from {OldFolder} to {NewFolder}", fileId, oldFolderId, newFolderId);
        }
    }
}