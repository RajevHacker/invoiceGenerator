using Google.Apis.Sheets.v4;
using InvoiceGenerator.Api.Models;
using InvoiceGenerator.Api.Services.Interfaces;

namespace InvoiceGenerator.Api.Services;

public class PurchaseCustomerService : IPurchaseCustomerService
{
    private readonly IPartnerConfigurationResolver _partnerConfigurationResolver;
    private readonly SheetsService _sheetsService;
    public PurchaseCustomerService(IPartnerConfigurationResolver partnerConfig, GoogleServiceFactory serviceFactory)
    {
         _sheetsService = serviceFactory.CreateSheetsService();
        _partnerConfigurationResolver = partnerConfig;
    }
    public async Task<List<purchaseCustomerDetails>> SearchCustomersAsync(string partnerName, string consumerName)
    {
        var _config = _partnerConfigurationResolver.GetSettings(partnerName).SheetSettings;
        var sheetName = _config.Sheets["PurchaseCustomer"];
        var sheetId = _config.SpreadsheetId;
        string range = $"{sheetName}!A:B"; // Assuming A = Name, B = GST

        var request = _sheetsService.Spreadsheets.Values.Get(sheetId, range);
        var response = await request.ExecuteAsync();

        var matched = response.Values?
            .Where(row => row.Count >= 1 && !string.IsNullOrWhiteSpace(row[0]?.ToString()))
            .Where(row => row[0].ToString().StartsWith(consumerName, StringComparison.OrdinalIgnoreCase))
            .Take(10)
            .Select(row => new purchaseCustomerDetails
            {
                Name = row[0].ToString(),
                gstNumber = row.Count > 1 ? row[1]?.ToString() ?? "" : ""
            })
            .ToList();

        return matched ?? new List<purchaseCustomerDetails>();
    }

    public Task UpsertCustomerAsync(string partnerName,purchaseCustomerDetails customer)
    {
        throw new NotImplementedException();
    }
}
