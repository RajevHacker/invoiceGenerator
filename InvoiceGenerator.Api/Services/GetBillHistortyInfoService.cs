using InvoiceGenerator.Api.Models;
using InvoiceGenerator.Api.Services.Interfaces;

namespace InvoiceGenerator.Api.Services;

public class GetBillHistortyInfoService : IGetBillHistortyInfo
{
    private readonly IGoogleSheetsService _sheetsService;
    private readonly IPartnerConfigurationResolver _partnerConfigResolver;
    public GetBillHistortyInfoService(IGoogleSheetsService googleSheetsService, IPartnerConfigurationResolver partnerConfigResolver)
    {
        _sheetsService = googleSheetsService;        
        _partnerConfigResolver = partnerConfigResolver;
    }
    public async Task<BillHistoryEntry?> GetBillHistoryInfo(string invoiceNumber, string partnerName)
    {
        var _sheetSettings = _partnerConfigResolver.GetSettings(partnerName).SheetSettings;
        string sheetName = _sheetSettings.Sheets["BillHistory"];
        string dataRange = $"{sheetName}!A:Z"; // Adjust range if you expect fewer/more columns

        var rows = await _sheetsService.GetSheetValuesAsync(_sheetSettings.SpreadsheetId, dataRange);

        if (rows == null || rows.Count == 0)
            return null;

        foreach (var row in rows)
        {
            if (row.Count >= 3 && row[2]?.ToString()?.Trim() == invoiceNumber)
            {
                return new BillHistoryEntry
                {
                    CustomerName = row.ElementAtOrDefault(0)?.ToString(),
                    GSTNumber = row.ElementAtOrDefault(1)?.ToString(),
                    InvoiceNumber = row.ElementAtOrDefault(2)?.ToString(),
                    // Date = TryParseDate(row.ElementAtOrDefault(3)),
                    TotalBeforeGST = TryParseDecimal(row.ElementAtOrDefault(4)),
                    Qty = TryParseInt(row.ElementAtOrDefault(5)),
                    CGST = TryParseDecimal(row.ElementAtOrDefault(6)),
                    SGST = TryParseDecimal(row.ElementAtOrDefault(7)),
                    IGST = TryParseDecimal(row.ElementAtOrDefault(8)),
                    GrandTotal = TryParseDecimal(row.ElementAtOrDefault(9))
                };
            }
        }

        return null;
    }
    private static decimal? TryParseDecimal(object? value)
    {
        return decimal.TryParse(value?.ToString(), out var result) ? result : null;
    }

    private static int? TryParseInt(object? value)
    {
        return int.TryParse(value?.ToString(), out var result) ? result : null;
    }

    private static DateOnly? TryParseDate(object? value)
    {
        if (DateOnly.TryParse(value?.ToString(), out var date))
            return date;

        if (DateTime.TryParse(value?.ToString(), out var dt))
            return DateOnly.FromDateTime(dt);

        return null;
    }
}
