using Google.Apis.Sheets.v4;
using InvoiceGenerator.Api.Models;
using InvoiceGenerator.Api.Services.Interfaces;

namespace InvoiceGenerator.Api.Services;

public class PurchaseCustomerService : IPurchaseCustomerService
{
    private readonly IPartnerConfigurationResolver _partnerConfigurationResolver;
    private readonly SheetsService _sheetsService;
    private readonly IGoogleSheetsService _googleSheetsService;
    public PurchaseCustomerService(IPartnerConfigurationResolver partnerConfig, GoogleServiceFactory serviceFactory, IGoogleSheetsService googleSheetsService)
    {
         _sheetsService = serviceFactory.CreateSheetsService();
        _partnerConfigurationResolver = partnerConfig;
        _googleSheetsService = googleSheetsService;
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

    public async Task UpsertCustomerAsync(string partnerName, purchaseCustomerDetails customer)
    {
        var _config = _partnerConfigurationResolver.GetSettings(partnerName).SheetSettings;
        var sheetName = _config.Sheets["PurchaseCustomer"];
        var sheetId = _config.SpreadsheetId;

        string readRange = $"{sheetName}!A:C";
        var existingRows = await _googleSheetsService.ReadRangeAsync(sheetId, readRange);

        int? matchingRowIndex = null;

        for (int i = 0; i < existingRows.Count; i++)
        {
            var row = existingRows[i];
            if (row.Count > 0 && row[0].ToString().Equals(customer.Name, StringComparison.OrdinalIgnoreCase))
            {
                matchingRowIndex = i + 1; // 1-based index for Sheets
                break;
            }
        }

        if (matchingRowIndex.HasValue)
        {
            // Update GST (column B) of existing customer
            string updateRange = $"{sheetName}!B{matchingRowIndex}";
            await _googleSheetsService.UpdateCellAsync(sheetId, updateRange, customer.gstNumber);
        }
        else
        {
            // Append new customer row
            int nextRow = await _googleSheetsService.GetNextRowIndexAsync(sheetId, sheetName);
            string formula = $"=SUMIF(PurchaseList!A:A,A{nextRow},PurchaseList!O:O)";
            await _googleSheetsService.AppendRowAsync(
                sheetId,
                $"{sheetName}!A:C", // Range
                new List<object> { customer.Name, customer.gstNumber, formula }
            );
        }
    }
}
