namespace InvoiceGenerator.Api.Services.Interfaces;

public interface IInvoicePdfExporter
{
    Task<string> ExportAndUploadInvoiceAsync(string spreadsheetId, int gid, string fileName, string folderId);
}