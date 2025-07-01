using InvoiceGenerator.Api.Models;
using InvoiceGenerator.Api.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace InvoiceGenerator.Api.Services;

public class GetInvoiceSummaryService : IGetInvoiceSummary
{
    private readonly IGoogleSheetsService _sheetsService;
    private readonly SheetSettings _settings;
    private readonly IPartnerConfigurationResolver _partnerConfigurationResolver;

    public GetInvoiceSummaryService(
        IGoogleSheetsService sheetsService,
        IOptions<SheetSettings> options,
        IPartnerConfigurationResolver partnerConfigurationResolver)
    {
        _sheetsService = sheetsService;
        _settings = options.Value;
        _partnerConfigurationResolver = partnerConfigurationResolver;
    }

    public async Task<BillHistoryEntry> GetInvoiceSummaryAsync(string partnerName)
    {
        var _sheetConfig = _partnerConfigurationResolver.GetSettings(partnerName).SheetSettings;;
        string sheetName = _sheetConfig.Sheets["Invoice"];
        string spreadsheetId = _sheetConfig.SpreadsheetId;

        var cells = new List<string>
        {
            $"{sheetName}!B9",   // CustomerName
            $"{sheetName}!B13",  // GSTNumber
            $"{sheetName}!E9",   // InvoiceNumber
            $"{sheetName}!E10",  // Date
            $"{sheetName}!D32",  // Qty
            $"{sheetName}!F34",  // TotalBeforeGST
            $"{sheetName}!F36",  // CGST
            $"{sheetName}!F37",  // SGST
            $"{sheetName}!F38",  // IGST
            $"{sheetName}!F40"   // GrandTotal
        };

        var values = new List<string>();
        System.Console.WriteLine(values);
        foreach (var cell in cells)
        {
            var result = await _sheetsService.GetSheetValuesAsync(spreadsheetId, cell);
            values.Add(result.FirstOrDefault()?.FirstOrDefault()?.ToString() ?? "");
        }

        return new BillHistoryEntry
        {
            CustomerName = values.ElementAtOrDefault(0),
            GSTNumber = values.ElementAtOrDefault(1),
            InvoiceNumber = values.ElementAtOrDefault(2),
            Date = DateOnly.TryParse(values.ElementAtOrDefault(3), out var date) ? date : null,
            Qty = int.TryParse(values.ElementAtOrDefault(4), out var qty) ? qty : null,
            TotalBeforeGST = decimal.TryParse(values.ElementAtOrDefault(5), out var total) ? total : null,
            CGST = decimal.TryParse(values.ElementAtOrDefault(6), out var cgst) ? cgst : null,
            SGST = decimal.TryParse(values.ElementAtOrDefault(7), out var sgst) ? sgst : null,
            IGST = decimal.TryParse(values.ElementAtOrDefault(8), out var igst) ? igst : null,
            GrandTotal = decimal.TryParse(values.ElementAtOrDefault(9), out var grandTotal) ? grandTotal : null
        };
    }
}