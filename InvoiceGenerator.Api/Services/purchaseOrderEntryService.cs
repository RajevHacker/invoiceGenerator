using InvoiceGenerator.Api.Models.InvoiceSummary;
using InvoiceGenerator.Api.Services.Interfaces;

namespace InvoiceGenerator.Api.Services;

public class purchaseOrderEntryService : IpurchaseOrderEntryService
{
    private readonly IGoogleSheetsService _googleSheetsService;
    private readonly IPartnerConfigurationResolver _partnerConfigurationResolver;
    public purchaseOrderEntryService(IGoogleSheetsService googleSheetsService, IPartnerConfigurationResolver partnerConfigurationResolver)
    {
        _googleSheetsService = googleSheetsService;
        _partnerConfigurationResolver = partnerConfigurationResolver;
    }
    public async Task AppendPurchaseOrderAsync(string partnerName, purchaseOrderEntry entry)
    {
        var _sheetConfig = _partnerConfigurationResolver.GetSettings(partnerName).SheetSettings;
        var spreadsheetId = _sheetConfig.SpreadsheetId;
        var sheetName = _sheetConfig.Sheets["PurchaseList"];
        var purchasePaymentSheetname = _sheetConfig.Sheets["PurchasePayment"];
        // Step 1: Get current row count to determine the next row index
        int nextRow = await _googleSheetsService.GetNextRowIndexAsync(spreadsheetId, sheetName);
        // Step 2: Construct the formula for Qn (TotalBalance = Mn - Ln)
        string InvoiceIndex = $"=COUNTIFS(A$2:A{nextRow}, A{nextRow})";
        string TotalBillPerCustomer = $"=SUMIFS(J:J, A:A, A{nextRow}, K:K, \"<=\"&K{nextRow})";
        string TotalPaidByCustomer = $"=SUMIF({purchasePaymentSheetname}!A:A, A{nextRow}, {purchasePaymentSheetname}!D:D)";
        string CustomFormula = $"=MAX(0, MIN(J{nextRow}, M{nextRow} - (L{nextRow} - J{nextRow})))";
        string StatusFlag = $"=J{nextRow} - N{nextRow}";
        string PaymentStatus = $"=IF(O{nextRow}=0, \"Paid\", IF(N{nextRow}=0, \"Unpaid\", \"Partially Paid\"))";
        string totalBalanceFormula = $"=M{nextRow} - L{nextRow}";

        var billHistory = new List<object>
        {
            entry.CustomerName ?? "",
            entry.GSTNumber ?? "",
            entry.InvoiceNumber ?? "",
            entry.Date?.ToString("yyyy-MM-dd") ?? "",
            entry.hsnCode?.ToString() ?? "",
            entry.Qty?.ToString() ?? "",
            entry.TotalBeforeGST?.ToString() ?? "",
            entry.CGST?.ToString() ?? "",
            entry.SGST?.ToString() ?? "",
            entry.GrandTotal?.ToString() ?? "",
            InvoiceIndex,
            TotalBillPerCustomer,
            TotalPaidByCustomer,
            CustomFormula,
            StatusFlag,
            PaymentStatus,
            totalBalanceFormula
        };

        var range = $"{sheetName}!A1:Q1"; // Column headers range
        await _googleSheetsService.AppendRowAsync(spreadsheetId, range, billHistory);
    }
}
