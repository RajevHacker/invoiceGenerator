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
    private readonly IPartnerConfigurationResolver _partnerConfig;

    public PaymentSheetService(GoogleServiceFactory factory,
                                 SheetsService sheetsService, 
                                 IPartnerConfigurationResolver partnerConfig)
    {
        _sheetsService = factory.CreateSheetsService();
        _partnerConfig = partnerConfig;
    }

    public async Task AppendPaymentAsync(string partnerName, PaymentEntry payment, string paymentType)
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
        var _config = _partnerConfig.GetSettings(partnerName).SheetSettings;
        string sheetName = paymentType == "Payments" ? _config.Sheets["Payments"] : _config.Sheets["PurchasePayment"];
        string range = $"{sheetName}!A1:D1";

        var valueRange = new ValueRange { Values = values };

        var appendRequest = _sheetsService.Spreadsheets.Values.Append(valueRange, _config.SpreadsheetId, range);
        appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
        appendRequest.InsertDataOption = SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum.INSERTROWS;

        await appendRequest.ExecuteAsync();
    }
}