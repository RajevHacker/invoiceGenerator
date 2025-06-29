using System.Runtime.InteropServices;
using InvoiceGenerator.Api.Models;
using InvoiceGenerator.Api.Services.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;

namespace InvoiceGenerator.Api.Services;

public class InvoiceService
{
    private readonly IInvoiceSheetWriter _sheetWriter;
    private readonly IInvoicePdfExporter _pdfExporter;
    private readonly IGetInvoiceSummary _getInvoiceSummary;
    private readonly IBillHistorySheetService _billHistorySheetService;
    private readonly IInvoiceNumberGenerator _invoiceNumberGenerator;
     private readonly SheetSettings _sheetSettings;
     private readonly GoogleDriveSettings _googleDriveSettings;


    public InvoiceService(IInvoiceSheetWriter invoiceSheetWriter, IInvoicePdfExporter pdfExporter, IGetInvoiceSummary getInvoiceSummary, IBillHistorySheetService billHistorySheetService, IInvoiceNumberGenerator invoiceNumber, IOptions<SheetSettings> sheetOptions, IOptions<GoogleDriveSettings> driveSettings)
    {
        _sheetWriter = invoiceSheetWriter;
        _pdfExporter = pdfExporter;
        _getInvoiceSummary = getInvoiceSummary;
        _billHistorySheetService = billHistorySheetService;
        _invoiceNumberGenerator = invoiceNumber;
        _sheetSettings = sheetOptions.Value;
        _googleDriveSettings = driveSettings.Value;
    }
     

    public async Task<InvoiceResponse> ProcessInvoiceAsync(InvoiceRequest request)
    {
        var invoiceNumber = await _invoiceNumberGenerator.GenerateNextInvoiceNumberAsync();     
        request.invoiceNumber = invoiceNumber;
        await _sheetWriter.WriteInvoiceToSheetAsync(request);
        BillHistoryEntry summary =  await _getInvoiceSummary.GetInvoiceSummaryAsync();
        _billHistorySheetService.AppendBillHistoryAsync(summary);
        var InvoiceNumber = summary.InvoiceNumber;
        string spreadsheetId = _sheetSettings.SpreadsheetId;
        int gid = 0;         
        string fileName = $"{InvoiceNumber}.pdf";    
        string folderId = _googleDriveSettings.GeneratedInvoicesFolderId;
        var url = await _pdfExporter.ExportAndUploadInvoiceAsync(spreadsheetId, gid, fileName, folderId);
        await _sheetWriter.ClearInvoiceFieldsAsync();

        return new InvoiceResponse
        {
            InvoiceNumber = InvoiceNumber,
            FileUrl = url
        };
        
    }
}
