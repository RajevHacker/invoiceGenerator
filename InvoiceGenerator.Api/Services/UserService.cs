using InvoiceGenerator.Api.Models;
using InvoiceGenerator.Api.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace InvoiceGenerator.Api.Services;

public class UserService : IUserService
{
    private readonly IGoogleSheetsService _googleSheetsService;
    private readonly IPartnerConfigurationResolver _partnerResolver;
    public UserService(
        IGoogleSheetsService googleSheetsService,
        IPartnerConfigurationResolver partnerResolver)
    {
        _googleSheetsService = googleSheetsService;
        _partnerResolver = partnerResolver;
    }

    public async Task<bool> ValidateUserAsync(string partnerName, string username, string password)
    {
        var _config = _partnerResolver.GetSettings(partnerName).SheetSettings;
        var spreadsheetId = _config.SpreadsheetId;
        var sheetName = _config.Sheets["User"];
        var range = $"{sheetName}!A2:B100";

        var users = await _googleSheetsService.ReadRangeAsync(spreadsheetId, range);
        if (users == null || users.Count == 0) return false;

        foreach (var row in users)
        {
            if (row.Count >= 2)
            {
                var sheetUsername = row[0]?.ToString();
                var sheetPassword = row[1]?.ToString();

                if (string.Equals(sheetUsername, username, StringComparison.OrdinalIgnoreCase) &&
                    sheetPassword == password)
                    return true;
            }
        }

        return false;
    }
}