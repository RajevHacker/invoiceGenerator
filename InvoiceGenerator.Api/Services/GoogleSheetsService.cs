using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using InvoiceGenerator.Api.Services.Interfaces;

namespace InvoiceGenerator.Api.Services;

public class GoogleSheetsService : IGoogleSheetsService
{
    private readonly SheetsService _sheetsService;

    public GoogleSheetsService(GoogleServiceFactory serviceFactory)
    {
        _sheetsService = serviceFactory.CreateSheetsService();
    }

    public async Task<IList<IList<object>>> ReadRangeAsync(string spreadsheetId, string range)
    {
        var request = _sheetsService.Spreadsheets.Values.Get(spreadsheetId, range);
        var response = await request.ExecuteAsync();
        return response.Values ?? new List<IList<object>>();
    }

    public async Task AppendRowAsync(string spreadsheetId, string range, IList<object> row)
    {
        var valueRange = new Google.Apis.Sheets.v4.Data.ValueRange
        {
            Values = new List<IList<object>> { row }
        };

        var request = _sheetsService.Spreadsheets.Values.Append(valueRange, spreadsheetId, range);
        request.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
        request.InsertDataOption = SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum.INSERTROWS;

        await request.ExecuteAsync();
    }

    public async Task UpdateRowAsync(string spreadsheetId, string range, IList<object> rowValues)
    {
        var valueRange = new ValueRange
        {
            Values = new List<IList<object>> { rowValues }
        };

        var request = _sheetsService.Spreadsheets.Values.Update(valueRange, spreadsheetId, range);
        request.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;
        await request.ExecuteAsync();
    }
    public async Task<int> GetNextRowIndexAsync(string spreadsheetId, string sheetName)
    {
        string range = $"{sheetName}!A:A"; // Column A is enough to count filled rows
        var request = _sheetsService.Spreadsheets.Values.Get(spreadsheetId, range);
        var response = await request.ExecuteAsync();

        int rowCount = response.Values?.Count ?? 0;

        // Add 1 to get the next row index (Google Sheets is 1-based)
        return rowCount + 1;
    }
    public async Task<IList<IList<object>>> GetSheetValuesAsync(string spreadsheetId, string range)
    {
        var request = _sheetsService.Spreadsheets.Values.Get(spreadsheetId, range);
        var response = await request.ExecuteAsync();

        return response.Values ?? new List<IList<object>>();
    }
    public async Task UpdateCellAsync(string spreadsheetId, string cell, object value)
    {
        var valueRange = new ValueRange
        {
            Range = cell,
            Values = new List<IList<object>> { new List<object> { value } }
        };

        var updateRequest = _sheetsService.Spreadsheets.Values.Update(valueRange, spreadsheetId, cell);
        updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;

        await updateRequest.ExecuteAsync();
    }
    public async Task BatchUpdateAsync(string spreadsheetId, Dictionary<string, object> updates)
    {
        var data = updates.Select(kvp => new Google.Apis.Sheets.v4.Data.ValueRange
        {
            Range = kvp.Key,
            Values = new List<IList<object>> { new List<object> { kvp.Value ?? "" } }
        }).ToList();

        var requestBody = new Google.Apis.Sheets.v4.Data.BatchUpdateValuesRequest
        {
            ValueInputOption = "USER_ENTERED",
            Data = data
        };

        var request = _sheetsService.Spreadsheets.Values.BatchUpdate(requestBody, spreadsheetId);
        await request.ExecuteAsync();
    }
    public async Task<List<string>> GetColumnValuesAsync(string spreadsheetId, string sheetName, string columnLetter)
    {
        string range = $"{sheetName}!{columnLetter}:{columnLetter}";
        var request = _sheetsService.Spreadsheets.Values.Get(spreadsheetId, range);
        var response = await request.ExecuteAsync();

        var values = response.Values?
            .Select(row => row.FirstOrDefault()?.ToString())
            .Where(val => !string.IsNullOrWhiteSpace(val))
            .ToList();

        return values ?? new List<string>();
    }
}