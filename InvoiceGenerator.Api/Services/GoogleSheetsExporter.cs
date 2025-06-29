using InvoiceGenerator.Api.Services.Interfaces;

namespace InvoiceGenerator.Api.Services;

public class GoogleSheetsExporter : IGoogleSheetsExporter
{
    public string GenerateExportUrl(string spreadsheetId, int gid)
    {
        return $"https://docs.google.com/spreadsheets/d/{spreadsheetId}/export?format=pdf&gid={gid}";
    }
}