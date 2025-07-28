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
        private readonly string[] Scopes = { DriveService.Scope.DriveFile };
        private readonly string ApplicationName = "DriveUploaderAPI";
        private readonly string TargetFolderId = "1S5QZi31_OeKfQRsBMi6Sr0ML_rBEpckR"; // Replace with your folder ID

        private DriveService? _driveService;

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

            var request = _driveService.Files.Create(fileMetadata, fileStream, mimeType);
            request.Fields = "id";
            await request.UploadAsync();

            var uploadedFileId = request.ResponseBody.Id;

            // Optional: make file publicly viewable
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

            if (string.IsNullOrWhiteSpace(fileId) || string.IsNullOrWhiteSpace(newFolderId))
            {
                // _logger.LogWarning("FileId or NewFolderId is missing.");
                // return Ok();
            }

            try
            {
                // Step 1: Get current parents
                var getRequest = _driveService.Files.Get(fileId);
                getRequest.Fields = "parents";
                var file = await getRequest.ExecuteAsync();

                var previousParents = file.Parents != null && file.Parents.Any()
                    ? string.Join(",", file.Parents)
                    : null;

                if (previousParents == null)
                {
                    // _logger.LogWarning("File {FileId} has no parent folders.", fileId);
                    // return false;
                }

                // Step 2: Move the file by changing parents
                var updateRequest = _driveService.Files.Update(new File(), fileId);
                updateRequest.AddParents = newFolderId;
                updateRequest.RemoveParents = previousParents;
                updateRequest.Fields = "id, parents";

                var updatedFile = await updateRequest.ExecuteAsync();

                // _logger.LogInformation("✅ File {FileId} moved from folder {OldFolderId} to {NewFolderId}.", fileId, oldFolderId, newFolderId);
                // return true;
            }
            catch (Exception ex)
            {
                // _logger.LogError(ex, "❌ Failed to move file {FileId} from {OldFolderId} to {NewFolderId}", fileId, oldFolderId, newFolderId);
                // return false;
            }
        }


        public async Task InitDriveServiceAsync()
        {
            using var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read);
            var credPath = "token_store";

            var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.FromStream(stream).Secrets,
                Scopes,
                "user",
                CancellationToken.None,
                new FileDataStore(credPath, true));

            _driveService = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });
        }
    }
}