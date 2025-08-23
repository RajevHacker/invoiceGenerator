using InvoiceGenerator.Api.Services.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;

namespace InvoiceGenerator.Api.Services;

public class resetFinancialYearService : IresetFinancialYearInterface
{
    private readonly IGoogleSheetsService _sheetsService;
    public IPartnerConfigurationResolver _partnerconfig;

    public resetFinancialYearService(IGoogleSheetsService gsheetService, IPartnerConfigurationResolver partnerConfig)
    {
        _sheetsService = gsheetService;
        _partnerconfig = partnerConfig;
    }
    public async Task<string> resetFinancialYear(string partnerName, string newFinancialYear)
    {        
        var _config = _partnerconfig.GetSettings(partnerName).SheetSettings;
        var spreadsheetId = _config.SpreadsheetId;
        var UserSheetName = _config.Sheets["User"];
        string FinancialYearCell = $"{UserSheetName}!D3"; 
        await _sheetsService.UpdateCellAsync(spreadsheetId, FinancialYearCell, newFinancialYear);
        var billhistortSheetName = _config.Sheets["BillHistory"];
        int nextRow = await _sheetsService.GetNextRowIndexAsync(spreadsheetId, billhistortSheetName);
        string billHistoryInvoiceCell = $"{billhistortSheetName}!C{nextRow}"; 
        string newFinancialYearCell = $"{billhistortSheetName}!A{nextRow}"; 
        await _sheetsService.UpdateCellAsync(spreadsheetId, newFinancialYearCell, "NEW FINANCIAL YEAR START : " + newFinancialYear);
        await _sheetsService.UpdateCellAsync(spreadsheetId, billHistoryInvoiceCell, 0);
        return "success";
    }
}
