using InvoiceGenerator.Api.Models;
using InvoiceGenerator.Api.Services.Interfaces;

namespace InvoiceGenerator.Api.Services;

public class addPurchaseConsumerRecord : IAddPurchaseConsumerRecord
{
    private readonly IGoogleSheetsService _googleSheetsService;
    private readonly IPartnerConfigurationResolver _partnerConfigResolver;
    public addPurchaseConsumerRecord(IPartnerConfigurationResolver partnerConfigurationResolver, IGoogleSheetsService googleSheetsService)
    {
        _partnerConfigResolver = partnerConfigurationResolver;
        _googleSheetsService = googleSheetsService;
    }
    public async Task AppendPurchaseOrderAsync(string partnerName, purchaseOrderConsumer poConsumer)
    {
        var _sheetConfig = _partnerConfigResolver.GetSettings(partnerName).SheetSettings;
        var spreadsheetId = _sheetConfig.SpreadsheetId;
        var sheetName = _sheetConfig.Sheets["PurchaseCustomer"];
        var range = $"{sheetName}!A1:C1";
        int nextRowIndex = await _googleSheetsService.GetNextRowIndexAsync(spreadsheetId, sheetName);
        var cusFormula = $"=SUMIF(PurchaseList!A:A,A{nextRowIndex},PurchaseList!O:O)";
        var row = new List<object>
        {
            poConsumer.Name ?? "",
            poConsumer.gstNumber ?? "",
            cusFormula
        };

        await _googleSheetsService.AppendRowAsync(spreadsheetId, range, row);
    }
}
