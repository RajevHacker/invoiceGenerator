namespace InvoiceGenerator.Api.Services.Interfaces;

public interface IDriveUploader
{
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string mimeType, string folderId);
    Task<IList<Google.Apis.Drive.v3.Data.File>> ListFilesInFolderAsync(string folderId);
    Task MoveFileAsync(string fileId, string oldFolderId, string newFolderId);
}