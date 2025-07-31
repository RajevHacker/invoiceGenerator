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
        Console.WriteLine(searchValue);
        Console.WriteLine(partnerName);
        Console.WriteLine(sheetName);

        var _config = _partnerConfig.GetSettings(partnerName).SheetSettings;

        string searchColumn;

        if (sheetName == "CustomerDetails")
        {
            searchColumn = "A";
        }
        else if (sheetName == "BillHistory")
        {
            searchColumn = "C";
        }
        else if (sheetName == "Products")
        {
            searchColumn = "A";
        }
        else
        {
            searchColumn = "A";
        }
        Console.WriteLine(searchColumn);

        var queryValue = await _googleSheetsService.GetColumnValuesAsync(
            _config.SpreadsheetId, 
            _config.Sheets[sheetName], 
            searchColumn
        );

        var matched = queryValue
            .Where(c => c.StartsWith(searchValue, StringComparison.OrdinalIgnoreCase))
            .Take(10)
            .ToList();

        if (!matched.Any())
        {
            return new List<string> { "Data not found. Please add new data." };
        }

        // Only write matched[0] if list is not empty
        Console.WriteLine(matched[0]);

        return matched;
    }
}
