namespace InvoiceGenerator.Api.Services.Interfaces;
using File = Google.Apis.Drive.v3.Data.File;
public interface IDriveUploader
{
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string mimeType, string folderId);
    Task<IList<File>> ListFilesInFolderAsync(string folderId);
    Task MoveFileAsync(string fileId, string oldFolderId, string newFolderId);
    Task InitDriveServiceAsync();
}