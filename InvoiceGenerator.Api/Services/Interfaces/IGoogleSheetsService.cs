namespace InvoiceGenerator.Api.Services.Interfaces;

public interface IGoogleSheetsService
{
    Task<IList<IList<object>>> ReadRangeAsync(string spreadsheetId, string range);
    Task AppendRowAsync(string spreadsheetId, string range, IList<object> rowValues);
    Task UpdateRowAsync(string spreadsheetId, string range, IList<object> rowValues);
    Task<int> GetNextRowIndexAsync(string spreadsheetId, string sheetName);
    Task<IList<IList<object>>> GetSheetValuesAsync(string spreadsheetId, string range);
    Task UpdateCellAsync(string spreadsheetId, string cell, object value);
    Task BatchUpdateAsync(string spreadsheetId, Dictionary<string, object> updates);
    Task<List<string>> GetColumnValuesAsync(string spreadsheetId, string sheetName, string columnLetter);
}