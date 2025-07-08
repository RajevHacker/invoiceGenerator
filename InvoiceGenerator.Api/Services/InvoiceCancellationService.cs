using InvoiceGenerator.Api.Models;
using InvoiceGenerator.Api.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace InvoiceGenerator.Api.Services
{
    public class InvoiceCancellationService : IInvoiceCancellationService
    {
        private readonly IDriveUploader _driveService;
        private readonly IGoogleSheetsService _sheetsService;
        private readonly IPartnerConfigurationResolver _partnerConfigurationResolver;

        public InvoiceCancellationService(
            IDriveUploader driveService,
            IGoogleSheetsService sheetsService,
            IOptions<GoogleDriveSettings> driveOptions,
            IOptions<SheetSettings> sheetOptions,
            IPartnerConfigurationResolver partnerConfigResolver)
        {
            _driveService = driveService;
            _sheetsService = sheetsService; 
            _partnerConfigurationResolver = partnerConfigResolver;
        }

        public async Task<bool> CancelInvoiceAsync(string invoiceNumber, string partnerName)
        {
            // Step 1: Find the file in Google Drive
            var _driveConfig = _partnerConfigurationResolver.GetSettings(partnerName).GoogleDriveSettings;
            var _sheetConfig = _partnerConfigurationResolver.GetSettings(partnerName).SheetSettings;
            var files = await _driveService.ListFilesInFolderAsync(_driveConfig.GeneratedInvoicesFolderId);
            var file = files.FirstOrDefault(f => f.Name.Contains(invoiceNumber, StringComparison.OrdinalIgnoreCase));

            if (file == null)
                return false; // File not found

            // Step 2: Move it to "Cancelled Invoices" folder
            await _driveService.MoveFileAsync(file.Id, _driveConfig.GeneratedInvoicesFolderId, _driveConfig.CancelledInvoicesFolderId);

            // Step 3: Find the row of invoiceNumber in BillHistory sheet (Invoice # in column C)
            
            string sheetName = _sheetConfig.Sheets["BillHistory"];
            string invoiceRange = $"{sheetName}!C:C"; // Invoice numbers in column C

            var rows = await _sheetsService.GetSheetValuesAsync(_sheetConfig.SpreadsheetId, invoiceRange);

            for (int i = 0; i < rows.Count; i++)
            {
                var value = rows[i].FirstOrDefault();
                if (value != null && value.ToString().Equals(invoiceNumber, StringComparison.OrdinalIgnoreCase))
                {
                    int rowIndex = i + 1; // Sheet rows are 1-indexed

                    // Prepare dictionary of updates: columns A-J (1-10)
                    var updates = new Dictionary<string, object>
                    {
                        [$"{sheetName}!A{rowIndex}"] = "Cancelled",    // CustomerName
                        [$"{sheetName}!C{rowIndex}"] = invoiceNumber,  // Invoice Number (keep original)
                        [$"{sheetName}!E{rowIndex}"] = 0,             // Total before GST
                        [$"{sheetName}!F{rowIndex}"] = 0,             // Qty
                        [$"{sheetName}!G{rowIndex}"] = 0,             // CGST
                        [$"{sheetName}!H{rowIndex}"] = 0,             // SGST
                        [$"{sheetName}!I{rowIndex}"] = 0,             // IGST
                        [$"{sheetName}!J{rowIndex}"] = 0              // Grand Total
                    };

                    // You can decide what values to keep or clear. Here, I clear all except InvoiceNumber and set CustomerName as "Cancelled".

                    await _sheetsService.BatchUpdateAsync(_sheetConfig.SpreadsheetId, updates);

                    return true;
                }
            }
            return false;
        }
    }
}