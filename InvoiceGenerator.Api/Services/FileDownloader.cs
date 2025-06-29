using Google.Apis.Drive.v3;
using Google.Apis.Download;
using InvoiceGenerator.Api.Services.Interfaces;
using InvoiceGenerator.Api.Services;
using System.Net.Http.Headers;
using Google.Apis.Auth.OAuth2;

public class FileDownloader : IFileDownloader
{
    private readonly GoogleCredential _credential;
    private readonly DriveService _driveService;
    private readonly ILogger<FileDownloader> _logger;

    public FileDownloader(GoogleServiceFactory factory, ILogger<FileDownloader> logger)
    {
         _credential = factory.GetCredential(); 
        _driveService =  factory.CreateDriveService();
        _logger = logger;
    }

    public async Task<Stream> DownloadSingleSheetPdfAsync(string exportUrl)
    {
        if (string.IsNullOrEmpty(exportUrl))
        {
            throw new ArgumentException("Spreadsheet ID must not be null or empty.", nameof(exportUrl));
        }

        try
        {
            var token = await _credential.UnderlyingCredential
                .GetAccessTokenForRequestAsync("https://www.googleapis.com/auth/drive.readonly");

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await httpClient.GetAsync(exportUrl);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to export sheet. Status: {Status}, Error: {Error}", response.StatusCode, error);
                throw new Exception($"Export failed: {response.StatusCode} - {error}");
            }

            var stream = new MemoryStream();
            await response.Content.CopyToAsync(stream);
            stream.Position = 0;

            return stream;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error exporting single sheet PDF. SpreadsheetId: {SpreadsheetId}, GID: {Gid}", exportUrl, exportUrl);
            throw;
        }
    }
}