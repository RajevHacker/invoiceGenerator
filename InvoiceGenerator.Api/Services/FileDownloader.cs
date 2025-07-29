using InvoiceGenerator.Api.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;

public class FileDownloader : IFileDownloader
{
    private readonly IGoogleSheetsService _googleSheetsService;
    private readonly ILogger<FileDownloader> _logger;

    public FileDownloader(IGoogleSheetsService googleSheetsService, ILogger<FileDownloader> logger)
    {
        _googleSheetsService = googleSheetsService;
        _logger = logger;
    }

    public async Task<Stream> DownloadSingleSheetPdfAsync(string exportUrl)
    {
        if (string.IsNullOrEmpty(exportUrl))
            throw new ArgumentException("Export URL must not be null or empty.", nameof(exportUrl));

        try
        {
            var token = await _googleSheetsService.GetAccessTokenAsync(); 

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await httpClient.GetAsync(exportUrl);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("❌ Failed to export sheet. Status: {Status}, Error: {Error}", response.StatusCode, error);
                throw new Exception($"Export failed: {response.StatusCode} - {error}");
            }

            var stream = new MemoryStream();
            await response.Content.CopyToAsync(stream);
            stream.Position = 0;

            _logger.LogInformation("✅ Exported sheet PDF successfully.");
            return stream;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Unexpected error exporting single sheet PDF. ExportUrl: {ExportUrl}", exportUrl);
            throw;
        }
    }
}