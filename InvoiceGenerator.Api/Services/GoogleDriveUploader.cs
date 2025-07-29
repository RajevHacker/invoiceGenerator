using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;
using InvoiceGenerator.Api.Models;
using InvoiceGenerator.Api.Services.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;
using File = Google.Apis.Drive.v3.Data.File;

namespace InvoiceGenerator.Api.Services
{
    public class GoogleDriveUploader: IDriveUploader
    {
        private readonly string[] Scopes = { DriveService.Scope.Drive };
        private readonly string ApplicationName = "DriveUploaderAPI";
        private readonly GoogleAuthorizationService _authService;
        private readonly ILogger<GoogleDriveUploader> _logger;

        private DriveService? _driveService;
        public GoogleDriveUploader(ILogger<GoogleDriveUploader> logger, GoogleAuthorizationService googleAuthorizationService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _authService = googleAuthorizationService;
        }

        // public async Task<string> UploadFileAsync(IFormFile formFile)
        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string mimeType, string folderId)
        {
            if (_driveService == null)
                await InitDriveServiceAsync();

            var fileMetadata = new Google.Apis.Drive.v3.Data.File
            {
                Name = fileName,
                Parents = new List<string> { folderId }
            };
            if (_driveService == null)
                throw new InvalidOperationException("DriveService not initialized.");
            var request = _driveService.Files.Create(fileMetadata, fileStream, mimeType);
            request.Fields = "id";
            await request.UploadAsync();
            var uploadedFileId = request.ResponseBody.Id;
            var permission = new Permission
            {
                Type = "anyone",
                Role = "reader"
            };
            await _driveService.Permissions.Create(permission, uploadedFileId).ExecuteAsync();

            return $"https://drive.google.com/file/d/{uploadedFileId}/view?usp=sharing";
        }
        public async Task<IList<File>> ListFilesInFolderAsync(string folderId)
        {
            if (_driveService == null)
                await InitDriveServiceAsync();

            var files = new List<File>();
            string pageToken = null;

            do
            {
                var request = _driveService.Files.List();
                request.Q = $"'{folderId}' in parents and trashed = false";
                request.Fields = "nextPageToken, files(id, name, mimeType, webViewLink, createdTime)";
                request.OrderBy = "createdTime desc"; // optional sorting
                request.PageSize = 100;
                request.PageToken = pageToken;

                var result = await request.ExecuteAsync();

                if (result.Files != null && result.Files.Any())
                {
                    files.AddRange(result.Files);
                }

                pageToken = result.NextPageToken;

            } while (pageToken != null);

            return files;
        }
        public async Task MoveFileAsync(string fileId, string oldFolderId, string newFolderId)
        {
            if (_driveService == null)
                await InitDriveServiceAsync();
            // Step 1: Get current parents
            var getRequest = _driveService.Files.Get(fileId);
            getRequest.Fields = "parents";
            var file = await getRequest.ExecuteAsync();
            var previousParents = file.Parents != null && file.Parents.Any()
                ? string.Join(",", file.Parents)
                : null;
            // Step 2: Move the file by changing parents
            var updateRequest = _driveService.Files.Update(new File(), fileId);
            updateRequest.AddParents = newFolderId;
            updateRequest.RemoveParents = previousParents;
            updateRequest.Fields = "id, parents";
            var updatedFile = await updateRequest.ExecuteAsync();
        }
        public async Task InitDriveServiceAsync()
        {
            var scopes = new[]
            {
                Google.Apis.Sheets.v4.SheetsService.Scope.Spreadsheets,
                Google.Apis.Drive.v3.DriveService.Scope.Drive
            };
            if (_driveService != null) return;
            try
            {
                var credential = await _authService.GetUserCredentialAsync(scopes);
                _driveService = new DriveService(_authService.GetServiceInitializer(credential, ApplicationName));
                _logger.LogInformation("✅ Google Drive API authorized.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to initialize Google Drive API.");
                throw;
            }
        }
    }
}