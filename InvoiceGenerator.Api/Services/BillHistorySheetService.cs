using InvoiceGenerator.Api.Models;
using InvoiceGenerator.Api.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace InvoiceGenerator.Api.Services;

public class BillHistorySheetService : IBillHistorySheetService
{
    private readonly IGoogleSheetsService _googleSheetsService;
    private readonly SheetSettings _sheetSettings;

    public BillHistorySheetService(IGoogleSheetsService googleSheetsService, IOptions<SheetSettings> options)
    {
        _googleSheetsService = googleSheetsService;
        _sheetSettings = options.Value;
    }

    public async Task AppendBillHistoryAsync(BillHistoryEntry entry)
    {
        var spreadsheetId = _sheetSettings.SpreadsheetId;
        var sheetName = _sheetSettings.Sheets["BillHistory"];

        // Step 1: Get current row count to determine the next row index
        int nextRow = await _googleSheetsService.GetNextRowIndexAsync(spreadsheetId, sheetName);

        // Step 2: Construct the formula for Qn (TotalBalance = Mn - Ln)
        string InvoiceIndex = $"=COUNTIFS(A$2:A{nextRow}, A{nextRow})";
        string TotalBillPerCustomer = $"=SUMIFS(J:J, A:A, A{nextRow}, K:K, \"<=\"&K{nextRow})";
        string TotalPaidByCustomer = $"=SUMIF(Payments!A:A, A{nextRow}, Payments!D:D)";
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
            entry.TotalBeforeGST?.ToString() ?? "",
            entry.Qty?.ToString() ?? "",
            entry.CGST?.ToString() ?? "",
            entry.SGST?.ToString() ?? "",
            entry.IGST?.ToString() ?? "",
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
