using InvoiceGenerator.Api.Models;
using InvoiceGenerator.Api.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InvoiceGenerator.Api.Services;
public class PaymentSheetService : IPaymentSheetService
{
    private readonly IGoogleSheetsService _sheetsService;
    private readonly IPartnerConfigurationResolver _partnerConfig;

    public PaymentSheetService(IGoogleSheetsService sheetsService, 
                                    IPartnerConfigurationResolver partnerConfig)
    {
        _sheetsService = sheetsService;
        _partnerConfig = partnerConfig;
    }

    public async Task AppendPaymentAsync(string partnerName, PaymentEntry payment, string paymentType)
    {
        var values = new List<object>
        {
            payment.CustomerName,
            payment.Date,
            payment.BankName,
            payment.Amount
        };

        var _config = _partnerConfig.GetSettings(partnerName).SheetSettings;
        string sheetName = paymentType == "Payments" ? _config.Sheets["Payments"] : _config.Sheets["PurchasePayment"];
        string range = $"{sheetName}!A1:D1";

        await _sheetsService.AppendRowAsync(_config.SpreadsheetId, range, values);
    }
}
