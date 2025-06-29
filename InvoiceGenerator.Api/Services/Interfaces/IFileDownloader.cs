namespace InvoiceGenerator.Api.Services.Interfaces;

public interface IFileDownloader
{
    // Task<Stream> DownloadFileAsync(string url);
    public Task<Stream> DownloadSingleSheetPdfAsync(string exportUrl);
}