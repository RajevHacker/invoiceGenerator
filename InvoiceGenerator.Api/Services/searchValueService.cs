using InvoiceGenerator.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceGenerator.Api.Services;

public class searchValueService : ISearchValueService
{
    private readonly IGoogleSheetsService _googleSheetsService;
    private readonly IPartnerConfigurationResolver _partnerConfig;
    public searchValueService(IGoogleSheetsService googleSheetsService, IPartnerConfigurationResolver partnerConfig)
    {
        _googleSheetsService = googleSheetsService;
        _partnerConfig = partnerConfig;
    }
    public async Task<List<string>> SearchValueAsync(string partnerName, string sheetName, string searchValue)
    {
        var _config = _partnerConfig.GetSettings(partnerName).SheetSettings;        
        string searchColumn; 
        
        if (sheetName == "CustomerDetails")
        {
            searchColumn = "B";
        }
        else
        {
            searchColumn = "A";
        }
        var queryValue = await _googleSheetsService.GetColumnValuesAsync(_config.SpreadsheetId, _config.Sheets[sheetName], searchColumn);
        var matched = queryValue
            .Where(c => c.StartsWith(searchValue, StringComparison.OrdinalIgnoreCase))
            .Take(10)
            .ToList();
        return matched;
    }
}
