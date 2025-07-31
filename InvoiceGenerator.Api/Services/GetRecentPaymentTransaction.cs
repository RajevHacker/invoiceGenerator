using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using InvoiceGenerator.Api.Models;
using InvoiceGenerator.Api.Services.Interfaces;

namespace InvoiceGenerator.Api.Services;

public class GetRecentPaymentTransaction : IGetRecentPaymentTransaction
{
    private readonly IPartnerConfigurationResolver _partnerConfigurationResolver;
    private readonly IGoogleSheetsService _googleSheetsService;
    public GetRecentPaymentTransaction(IPartnerConfigurationResolver partnerConfigurationResolver, 
                                        IGoogleSheetsService googleSheetsService)
    {
        _partnerConfigurationResolver = partnerConfigurationResolver;
        _googleSheetsService = googleSheetsService;
    }
    public async Task<IList<PaymentReport>> getPaymentReport(string partnerName, string paymentType)
    {
        var _config = _partnerConfigurationResolver.GetSettings(partnerName).SheetSettings;
        string spreadsheetId = _config.SpreadsheetId;
        string sheetName = _config.Sheets[$"{paymentType}"];
        string range = $"{sheetName}!A2:D";
        var values = await _googleSheetsService.ReadRangeAsync(spreadsheetId, range);

        var result = new List<PaymentReport>();

        foreach (var row in values)
        {
            // if (row.Count < 10) continue; // Skip incomplete rows
            var record = new PaymentReport
            {
                CustomerName = row[0]?.ToString() ?? "",
                Date = DateTime.TryParse(row[1]?.ToString(), out var parsedDate) ? DateOnly.FromDateTime(parsedDate) : null,
                Amount = decimal.TryParse(row[3]?.ToString(), out var amt) ? amt : 0,
            };

            result.Add(record);
        }
        var orderedReports = result.OrderByDescending(r => r.Date.Value);
        var paymentResult = orderedReports.Take(10).ToList();
        return paymentResult;
    }
}
