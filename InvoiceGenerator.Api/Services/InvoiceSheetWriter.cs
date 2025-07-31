using InvoiceGenerator.Api.Models;
using InvoiceGenerator.Api.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace InvoiceGenerator.Api.Services;

public class InvoiceSheetWriter : IInvoiceSheetWriter
{
    private readonly IGoogleSheetsService _sheetsService;
    private readonly SheetSettings _settings;
    private readonly IPartnerConfigurationResolver _partnerConfigurationResolver;

    public InvoiceSheetWriter(IGoogleSheetsService sheetsService, IOptions<SheetSettings> options, IPartnerConfigurationResolver partnerConfigResolver)  
    {
        _sheetsService = sheetsService;
        _settings = options.Value;
        _partnerConfigurationResolver = partnerConfigResolver;
    }
    public async Task WriteInvoiceToSheetAsync(string partnerName, InvoiceRequest request)
    {
        var _sheetConfig = _partnerConfigurationResolver.GetSettings(partnerName).SheetSettings;

        string sheetName = _sheetConfig.Sheets["Invoice"];
        string spreadsheetId = _sheetConfig.SpreadsheetId;

        var updates = new Dictionary<string, object>
        {
            [$"{sheetName}!B9"] = request.Name,
            [$"{sheetName}!E9"] = request.invoiceNumber,
            // [$"{sheetName}!E10"] = request.CurrentDate.ToString("dd-MMM-yyyy"),
            [$"{sheetName}!E10"] = request.CurrentDate != default
                ? request.CurrentDate.ToString("dd-MMM-yyyy")
                : "",
            [$"{sheetName}!E12"] = request.NoOfBales,
            [$"{sheetName}!E13"] = request.Transport
        };

        foreach (var (item, index) in request.Items.Select((x, i) => (x, i)))
        {
            int row = 16 + index;
            updates[$"{sheetName}!B{row}"] = item.ProductName;
            updates[$"{sheetName}!D{row}"] = item.Qty;
            updates[$"{sheetName}!E{row}"] = item.Price;
        }

        await _sheetsService.BatchUpdateAsync(spreadsheetId, updates);
    }

    public async Task ClearInvoiceFieldsAsync(string partnerName)
    {
        var _config = _partnerConfigurationResolver.GetSettings(partnerName);
        string sheetName = _config.SheetSettings.Sheets["Invoice"];
        string spreadsheetId =_config.SheetSettings.SpreadsheetId;

        var clears = new Dictionary<string, object>
        {
            [$"{sheetName}!B9"] = "",
            [$"{sheetName}!E9"] = "",
            [$"{sheetName}!E10"] = "",
            [$"{sheetName}!E12"] = "",
            [$"{sheetName}!E13"] = ""
        };

        for (int row = 16; row <= 31; row++)
        {
            clears[$"{sheetName}!B{row}"] = "";
            clears[$"{sheetName}!D{row}"] = "";
            clears[$"{sheetName}!E{row}"] = "";
        }

        await _sheetsService.BatchUpdateAsync(spreadsheetId, clears);
    }
}