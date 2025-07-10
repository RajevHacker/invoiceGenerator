using InvoiceGenerator.Api.Models;
using InvoiceGenerator.Api.Services.Interfaces;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Apis.Sheets.v4.Data;

namespace InvoiceGenerator.Api.Services;

public class CustomerInfoService : ICustomerInfoService
{
    private readonly IGoogleSheetsService _googleSheetsService;
    private readonly SheetSettings _sheetSettings;
    private readonly IPartnerConfigurationResolver _partnerConfigResolver;

    public CustomerInfoService(IGoogleSheetsService googleSheetsService, IOptions<SheetSettings> options, IPartnerConfigurationResolver partnerConfig)
    {
        _googleSheetsService = googleSheetsService;
        _sheetSettings = options.Value;
        _partnerConfigResolver = partnerConfig;
    }

    public async Task AddCustomerAsync(CustomerInfo customer, string partnerName)
    {
        var _sheetSetting = _partnerConfigResolver.GetSettings(partnerName).SheetSettings;
        var spreadsheetId = _sheetSetting.SpreadsheetId;
        var sheetName = _sheetSetting.Sheets["CustomerDetails"];
        var range = $"{sheetName}!A1:G1";
        int nextRowIndex = await _googleSheetsService.GetNextRowIndexAsync(spreadsheetId, sheetName);
        var cusFormula = $"=SUMIF(BillHistory!A:A,A{nextRowIndex},BillHistory!O:O)";

        var row = new List<object>
        {
            customer.Name ?? "",
            customer.Address ?? "",
            customer.ContactNumber ?? "",
            customer.State ?? "",
            customer.StateCode ?? "",
            customer.GSTNo ?? "",
            customer.Email ?? "",
            cusFormula
        };

        await _googleSheetsService.AppendRowAsync(spreadsheetId, range, row);
    }

    public async Task<IList<CustomerInfo>> GetAllCustomersAsync(string partnerName)
    {
        var _sheetSetting = _partnerConfigResolver.GetSettings(partnerName).SheetSettings;

        var spreadsheetId = _sheetSetting.SpreadsheetId;
        var sheetName = _sheetSetting.Sheets["CustomerDetails"];
        var range = $"{sheetName}!A2:G";

        var values = await _googleSheetsService.GetSheetValuesAsync(spreadsheetId, range);

        var customers = new List<CustomerInfo>();
        if (values != null)
        {
            foreach (var row in values)
            {
                customers.Add(new CustomerInfo
                {
                    Name = row.ElementAtOrDefault(0)?.ToString() ?? "",
                    Address = row.ElementAtOrDefault(1)?.ToString() ?? "",
                    ContactNumber = row.ElementAtOrDefault(2)?.ToString() ?? "",
                    State = row.ElementAtOrDefault(3)?.ToString() ?? "",
                    StateCode = row.ElementAtOrDefault(4)?.ToString() ?? "",
                    GSTNo = row.ElementAtOrDefault(5)?.ToString() ?? "",
                    Email = row.ElementAtOrDefault(6)?.ToString() ?? ""
                });
            }
        }
        return customers;
    }

    public async Task<CustomerInfo?> GetCustomerByNameAsync(string name, string partnerName)
    {
        var customers = await GetAllCustomersAsync(partnerName);        
        var customer = customers.FirstOrDefault(c => 
            string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase));
        return customer;
    }

    public async Task UpdateCustomerByNameAsync(string name, CustomerInfo updated, string partnerName)
    {
        var _sheetSetting = _partnerConfigResolver.GetSettings(partnerName).SheetSettings;
        var spreadsheetId = _sheetSetting.SpreadsheetId;
        var sheetName = _sheetSetting.Sheets["CustomerDetails"];
        var range = $"{sheetName}!A2:G";

        var values = await _googleSheetsService.GetSheetValuesAsync(spreadsheetId, range);

        if (values == null) return;

        int rowIndex = -1;
        for (int i = 0; i < values.Count; i++)
        {
            var row = values[i];
            var existingName = row.ElementAtOrDefault(0)?.ToString();
            if (string.Equals(existingName, name, StringComparison.OrdinalIgnoreCase))
            {
                rowIndex = i + 2;
                break;
            }
        }

        if (rowIndex == -1) return; // Not found

        // Ensure updated name is the same
        updated.Name = name;

        var updatedRow = new List<object>
        {
            updated.Name ?? "",
            updated.Address ?? "",
            updated.ContactNumber ?? "",
            updated.State ?? "",
            updated.StateCode ?? "",
            updated.GSTNo ?? "",
            updated.Email ?? ""
        };

        var updateRange = $"{sheetName}!A{rowIndex}:G{rowIndex}";

        await _googleSheetsService.UpdateRowAsync(spreadsheetId, updateRange, updatedRow);
    }

    public async Task DeleteCustomerAsync(string gstNo)
    {
        var spreadsheetId = _sheetSettings.SpreadsheetId;
        var sheetName = _sheetSettings.Sheets["CustomerDetails"];
        var range = $"{sheetName}!A2:G";

        var values = await _googleSheetsService.GetSheetValuesAsync(spreadsheetId, range);

        if (values == null) return;

        int rowIndex = -1;
        for (int i = 0; i < values.Count; i++)
        {
            var row = values[i];
            if (row.ElementAtOrDefault(5) == gstNo)
            {
                rowIndex = i + 2;
                break;
            }
        }

        if (rowIndex == -1) return; // Not found

        // Clear the row by writing empty values
        var emptyRow = new List<object> { "", "", "", "", "", "", "" };
        var clearRange = $"{sheetName}!A{rowIndex}:G{rowIndex}";

        await _googleSheetsService.UpdateRowAsync(spreadsheetId, clearRange, emptyRow);
    }
}