using InvoiceGenerator.Api.Models.InvoiceSummary;
using InvoiceGenerator.Api.Services.Interfaces;

namespace InvoiceGenerator.Api.Services;

public class getPurchaseListService : IGetPurchaseList
{
    public readonly IPartnerConfigurationResolver _partnerConfigurationResolver;
    private readonly IGoogleSheetsService _googleSheetsService;
    public getPurchaseListService(IPartnerConfigurationResolver partnerConfigResolver, IGoogleSheetsService gsheetService)
    {
        _partnerConfigurationResolver = partnerConfigResolver;
        _googleSheetsService = gsheetService;
    }
    public async Task<IList<purchaseOrderEntry>> getPurchaseList(string partnerName, string? consumerName, DateTime? startDate, DateTime? endDate)
    {
        {
            var _config = _partnerConfigurationResolver.GetSettings(partnerName).SheetSettings;
            string spreadsheetId = _config.SpreadsheetId;
            string sheetName = _config.Sheets["PurchaseList"];
            string range = $"{sheetName}!A2:J"; // Assuming headers are in row 1

            var values = await _googleSheetsService.ReadRangeAsync(spreadsheetId, range);
            var result = new List<purchaseOrderEntry>();

            // Default date range: last 30 days
            DateTime start = startDate ?? DateTime.UtcNow.AddDays(-30);
            DateTime end = endDate ?? DateTime.UtcNow;

            foreach (var row in values)
            {
                if (row.Count < 10) continue; // Skip incomplete rows
                var record = new purchaseOrderEntry
                {
                    CustomerName = row[0]?.ToString() ?? "",
                    GSTNumber = row[1]?.ToString() ?? "",
                    InvoiceNumber = row[2]?.ToString() ?? "",
                    Date = DateTime.TryParse(row[3]?.ToString(), out var parsedDate) ? DateOnly.FromDateTime(parsedDate) : null,
                    hsnCode = row[4]?.ToString() ?? "",
                    Qty = int.TryParse(row[5]?.ToString(), out var qty) ? qty : 0,
                    TotalBeforeGST = decimal.TryParse(row[6]?.ToString(), out var amt) ? amt : 0,
                    CGST = decimal.TryParse(row[7]?.ToString(), out var cgst) ? cgst : 0,
                    SGST = decimal.TryParse(row[8]?.ToString(), out var sgst) ? sgst : 0,
                    GrandTotal = decimal.TryParse(row[9]?.ToString(), out var total) ? total : 0,
                };
                DateOnly startDateOnly = DateOnly.FromDateTime(start);
                DateOnly endDateOnly = DateOnly.FromDateTime(end);

                if (record.Date is null || record.Date < startDateOnly || record.Date > endDateOnly)
                    continue;

                if (!string.IsNullOrWhiteSpace(consumerName) &&
                    !record.CustomerName.Equals(consumerName, StringComparison.OrdinalIgnoreCase))
                    continue;

                result.Add(record);
            }
            return result;
        }
    }
}
