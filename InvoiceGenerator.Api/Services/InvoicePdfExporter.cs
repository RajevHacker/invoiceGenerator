using InvoiceGenerator.Api.Services.Interfaces;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;

namespace InvoiceGenerator.Api.Services;

public class InvoicePdfExporter : IInvoicePdfExporter
{
    private readonly IGoogleSheetsExporter _sheetsExporter;
    private readonly IFileDownloader _fileDownloader;
    private readonly IDriveUploader _driveUploader;
    private readonly ILogger<IInvoicePdfExporter> _logger;

    public InvoicePdfExporter(
        IGoogleSheetsExporter sheetsExporter,
        IFileDownloader fileDownloader,
        IDriveUploader driveUploader, ILogger<IInvoicePdfExporter> logger)
    {
        _sheetsExporter = sheetsExporter;
        _fileDownloader = fileDownloader;
        _driveUploader = driveUploader;
        _logger = logger;
    }

    public async Task<string> ExportAndUploadInvoiceAsync(string spreadsheetId, int gid, string fileName, string folderId)
    {
        var exportUrl = _sheetsExporter.GenerateExportUrl(spreadsheetId, gid);
        var pdfStream = await _fileDownloader.DownloadSingleSheetPdfAsync(exportUrl);
        var a = await _driveUploader.UploadFileAsync(pdfStream, fileName, "application/pdf", folderId);
        System.Console.WriteLine("============");
        System.Console.WriteLine(a);
        return a;
    }
}