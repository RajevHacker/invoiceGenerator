using InvoiceGenerator.Api.Models;
using InvoiceGenerator.Api.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace InvoiceGenerator.Api.Services;

public class salesInvoiceListService : IsalesInvoiceList
{
    private readonly IGoogleSheetsService _sheetsService;
    public IPartnerConfigurationResolver _partnerconfig;

    public salesInvoiceListService(IGoogleSheetsService sheetsService, IPartnerConfigurationResolver partnerConfig)
    {
        _sheetsService = sheetsService;
        _partnerconfig = partnerConfig;
    }
    public async Task<List<purchaseInvoiceList>> GetUnpaidOrPartiallyPaidInvoicesAsync(string customerName, string partnerName)
    {
        var _config = _partnerconfig.GetSettings(partnerName).SheetSettings;
        var SheetName = _config.Sheets["BillHistory"];
        var spreadsheetId = _config.SpreadsheetId;

        // Step 1: Get headers
        var headerRow = await _sheetsService.GetSheetValuesAsync(spreadsheetId, $"{SheetName}!A1:Z1");
        if (!headerRow.Any()) return new List<purchaseInvoiceList>();

        var headers = headerRow.First();
        var columnMap = new Dictionary<string, int>();
        for (int i = 0; i < headers.Count; i++)
        {
            var key = headers[i]?.ToString()?.Trim();
            if (!string.IsNullOrEmpty(key))
                columnMap[key] = i;
        }

        // Step 2: Get all rows
        var rows = await _sheetsService.GetSheetValuesAsync(spreadsheetId, $"{SheetName}!A2:Z");

        var results = new List<purchaseInvoiceList>();

        foreach (var row in rows)
        {
            var name = GetCell(row, columnMap, "CustomerName");
            var status = GetCell(row, columnMap, "Payment Status").ToLower();

            if (!string.Equals(name, customerName, StringComparison.OrdinalIgnoreCase)) continue;
            if (status != "unpaid" && status != "partially paid") continue;

            results.Add(new purchaseInvoiceList
            {
                CustomerName = name,
                InvoiceNumber = GetCell(row, columnMap, "Invoice Number"),
                Date = GetCell(row, columnMap, "Date"),
                GrandTotal = ParseDecimal(GetCell(row, columnMap, "Grand Total")),
                BalanceAmount = ParseDecimal(GetCell(row, columnMap, "StatusFlag")),
                PaymentStatus = GetCell(row, columnMap, "Payment Status")
            });
        }

        return results;
    }

    private string GetCell(IList<object> row, Dictionary<string, int> columnMap, string columnName)
    {
        if (columnMap.TryGetValue(columnName, out int index))
            return row.ElementAtOrDefault(index)?.ToString() ?? "";
        return "";
    }

    private decimal ParseDecimal(string value)
    {
        return decimal.TryParse(value, out var result) ? result : 0;
    }
}