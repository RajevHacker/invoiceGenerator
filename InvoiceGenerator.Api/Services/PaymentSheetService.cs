using InvoiceGenerator.Api.Services.Interfaces;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using InvoiceGenerator.Api.Models;
using InvoiceGenerator.Api.Services.Interfaces;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace InvoiceGenerator.Api.Services;

public class PaymentSheetService : IPaymentSheetService
{
    private readonly SheetsService _sheetsService;
    private readonly SheetSettings _sheetSettings;

    public PaymentSheetService(GoogleServiceFactory factory, IOptions<SheetSettings> options, SheetsService sheetsService, IOptions<SheetSettings> sheetSettings)
    {
        _sheetsService = factory.CreateSheetsService();
        _sheetSettings = options.Value;
        // _sheetSettings = sheetSettings.Value;
    }

    public async Task AppendPaymentAsync(PaymentEntry payment)
    {
        var values = new List<IList<object>>
        {
            new List<object>
            {
                payment.CustomerName,
                payment.Date,
                payment.BankName,
                payment.Amount
            }
        };

        string sheetName = _sheetSettings.Sheets["Payments"];
        string range = $"{sheetName}!A1:D1";

        var valueRange = new ValueRange { Values = values };

        var appendRequest = _sheetsService.Spreadsheets.Values.Append(valueRange, _sheetSettings.SpreadsheetId, range);
        appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
        appendRequest.InsertDataOption = SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum.INSERTROWS;

        await appendRequest.ExecuteAsync();
    }
}