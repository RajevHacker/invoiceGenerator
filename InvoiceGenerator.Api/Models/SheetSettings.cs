namespace InvoiceGenerator.Api.Models;

public class SheetSettings
{
    public string SpreadsheetId { get; set; }
    public Dictionary<string, string> Sheets { get; set; } = new();
}
