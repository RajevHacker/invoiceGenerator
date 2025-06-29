namespace InvoiceGenerator.Api.Services.Interfaces;

public interface IGoogleSheetsExporter
{
    string GenerateExportUrl(string spreadsheetId, int gid);
}