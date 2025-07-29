using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util;
using Google.Apis.Util.Store;
using InvoiceGenerator.Api.Services.Interfaces;

namespace InvoiceGenerator.Api.Services;

public class GoogleSheetsService : IGoogleSheetsService
{
    private readonly ILogger<GoogleSheetsService> _logger;
    private SheetsService _sheetsService;

    private const string ApplicationName = "InvoiceGeneratorOAuth";
    private const string CredentialsFile = "credentials.json";
    private const string TokenFolder = "tokens";

    private readonly string[] Scopes = { SheetsService.Scope.Spreadsheets };

    public GoogleSheetsService(ILogger<GoogleSheetsService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    public async Task InitSheetsServiceAsync()
    {
        if (_sheetsService != null) return;

        try
        {
            using var stream = new FileStream(CredentialsFile, FileMode.Open, FileAccess.Read);

            var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.FromStream(stream).Secrets,
                Scopes,
                "user",
                CancellationToken.None,
                new FileDataStore(TokenFolder, true));

            _sheetsService = new SheetsService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName
            });

            _logger.LogInformation("✅ Google Sheets API authorized.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to initialize Google Sheets API.");
            throw;
        }
    }
    public async Task<string> GetAccessTokenAsync()
    {
        await InitSheetsServiceAsync();

        if (_sheetsService.HttpClientInitializer is UserCredential credential)
        {
            if (credential.Token.IsExpired(SystemClock.Default))
            {
                await credential.RefreshTokenAsync(CancellationToken.None);
            }

            return credential.Token.AccessToken;
        }

        throw new InvalidOperationException("HttpClientInitializer is not a UserCredential.");
    }
    public async Task<IList<IList<object>>> ReadRangeAsync(string spreadsheetId, string range)
    {
        await InitSheetsServiceAsync();

        try
        {
            var request = _sheetsService.Spreadsheets.Values.Get(spreadsheetId, range);
            var response = await request.ExecuteAsync();
            return response.Values ?? new List<IList<object>>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to read range {Range}", range);
            return new List<IList<object>>();
        }
    }

    public async Task AppendRowAsync(string spreadsheetId, string range, IList<object> rowValues)
    {
        await InitSheetsServiceAsync();

        try
        {
            var valueRange = new ValueRange { Values = new List<IList<object>> { rowValues } };
            var request = _sheetsService.Spreadsheets.Values.Append(valueRange, spreadsheetId, range);
            request.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
            await request.ExecuteAsync();
            _logger.LogInformation("✅ Appended row to {Range}", range);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to append row to {Range}", range);
            throw;
        }
    }

    public async Task UpdateRowAsync(string spreadsheetId, string range, IList<object> rowValues)
    {
        await InitSheetsServiceAsync();

        try
        {
            var valueRange = new ValueRange { Values = new List<IList<object>> { rowValues } };
            var request = _sheetsService.Spreadsheets.Values.Update(valueRange, spreadsheetId, range);
            request.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
            await request.ExecuteAsync();
            _logger.LogInformation("✅ Updated row in {Range}", range);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to update row in {Range}", range);
            throw;
        }
    }

    public async Task<int> GetNextRowIndexAsync(string spreadsheetId, string sheetName)
    {
        var range = $"{sheetName}!A:A";
        var values = await ReadRangeAsync(spreadsheetId, range);
        return (values?.Count ?? 0) + 1;
    }

    public async Task<IList<IList<object>>> GetSheetValuesAsync(string spreadsheetId, string range)
    {
        return await ReadRangeAsync(spreadsheetId, range);
    }

    public async Task UpdateCellAsync(string spreadsheetId, string cell, object value)
    {
        await InitSheetsServiceAsync();

        try
        {
            var valueRange = new ValueRange
            {
                Values = new List<IList<object>> { new List<object> { value } }
            };

            var request = _sheetsService.Spreadsheets.Values.Update(valueRange, spreadsheetId, cell);
            request.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
            await request.ExecuteAsync();
            _logger.LogInformation("✅ Updated cell {Cell}", cell);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to update cell {Cell}", cell);
            throw;
        }
    }

    public async Task BatchUpdateAsync(string spreadsheetId, Dictionary<string, object> updates)
    {
        await InitSheetsServiceAsync();

        try
        {
            var data = updates.Select(kvp => new ValueRange
            {
                Range = kvp.Key,
                Values = new List<IList<object>> { new List<object> { kvp.Value } }
            }).ToList();

            var requestBody = new BatchUpdateValuesRequest
            {
                Data = data,
                ValueInputOption = "USER_ENTERED"
            };

            var request = _sheetsService.Spreadsheets.Values.BatchUpdate(requestBody, spreadsheetId);
            await request.ExecuteAsync();
            _logger.LogInformation("✅ Batch update completed.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed batch update.");
            throw;
        }
    }

    public async Task<List<string>> GetColumnValuesAsync(string spreadsheetId, string sheetName, string columnLetter)
    {
        var range = $"{sheetName}!{columnLetter}:{columnLetter}";
        var values = await ReadRangeAsync(spreadsheetId, range);

        return values.Select(row => row.FirstOrDefault()?.ToString() ?? string.Empty).ToList();
    }
}
