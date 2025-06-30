using InvoiceGenerator.Api.Models;
using InvoiceGenerator.Api.Services.Interfaces;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace InvoiceGenerator.Api.Services;

public class UserService : IUserService
{
    private readonly IGoogleSheetsService _googleSheetsService;
    private readonly string _spreadsheetId;
    private readonly string _userSheetRange;

    public UserService(IGoogleSheetsService googleSheetsService, IOptions<SheetSettings> sheetSettings)
    {
        _googleSheetsService = googleSheetsService;
        _spreadsheetId = sheetSettings.Value.SpreadsheetId;

        var sheetName = sheetSettings.Value.Sheets.ContainsKey("User") ? sheetSettings.Value.Sheets["User"] : "User";
        _userSheetRange = $"'{sheetName}'!A2:B100"; // Username in A, Password in B
    }

    public async Task<bool> ValidateUserAsync(string username, string password)
    {
        var users = await _googleSheetsService.ReadRangeAsync(_spreadsheetId, _userSheetRange);
        if (users == null || users.Count == 0) return false;

        foreach (var row in users)
        {
            if (row.Count >= 2)
            {
                var sheetUsername = row[0]?.ToString();
                var sheetPassword = row[1]?.ToString();

                if (string.Equals(sheetUsername, username, StringComparison.OrdinalIgnoreCase) && sheetPassword == password)
                    return true;
            }
        }
        return false;
    }
}