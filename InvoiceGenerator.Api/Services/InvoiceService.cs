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

    private readonly IPartnerConfigurationResolver _partnerConfigurationResolver;
    public InvoiceService(IInvoiceSheetWriter invoiceSheetWriter, 
                            IInvoicePdfExporter pdfExporter, 
                            IGetInvoiceSummary getInvoiceSummary, 
                            IBillHistorySheetService billHistorySheetService, 
                            IInvoiceNumberGenerator invoiceNumber, 
                            IOptions<SheetSettings> sheetOptions, 
                            IOptions<GoogleDriveSettings> driveSettings,
                            IPartnerConfigurationResolver partnerConfig)
    {
        _sheetWriter = invoiceSheetWriter;
        _pdfExporter = pdfExporter;
        _getInvoiceSummary = getInvoiceSummary;
        _billHistorySheetService = billHistorySheetService;
        _invoiceNumberGenerator = invoiceNumber;
        _sheetSettings = sheetOptions.Value;
        _googleDriveSettings = driveSettings.Value;
        _partnerConfigurationResolver = partnerConfig;
    }
     

    public async Task<InvoiceResponse> ProcessInvoiceAsync(string partnerName, InvoiceRequest request)
    {
        var _sheetConfig = _partnerConfigurationResolver.GetSettings(partnerName);
        var invoiceNumber = await _invoiceNumberGenerator.GenerateNextInvoiceNumberAsync(partnerName);     
        request.invoiceNumber = invoiceNumber;
        await _sheetWriter.WriteInvoiceToSheetAsync(partnerName, request);
        BillHistoryEntry summary =  await _getInvoiceSummary.GetInvoiceSummaryAsync(partnerName);
        _billHistorySheetService.AppendBillHistoryAsync(partnerName, summary);
        var InvoiceNumber = summary.InvoiceNumber;
        string spreadsheetId = _sheetConfig.SheetSettings.SpreadsheetId;
        int gid = 0; 
        string fileName = $"{InvoiceNumber}.pdf";    
        // string folderId = _googleDriveSettings.GeneratedInvoicesFolderId;
        string folderId = _sheetConfig.GoogleDriveSettings.GeneratedInvoicesFolderId;
        var url = await _pdfExporter.ExportAndUploadInvoiceAsync(spreadsheetId, gid, fileName, folderId);
        await _sheetWriter.ClearInvoiceFieldsAsync(partnerName);

        return new InvoiceResponse
        {
            InvoiceNumber = InvoiceNumber,
            FileUrl = url
        };
        
    }
}
