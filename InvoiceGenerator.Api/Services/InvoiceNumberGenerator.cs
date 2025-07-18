using InvoiceGenerator.Api.Models;
using InvoiceGenerator.Api.Services.Interfaces;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;

namespace InvoiceGenerator.Api.Services
{
    public class InvoiceNumberGenerator : IInvoiceNumberGenerator
    {
        private readonly IGoogleSheetsService _sheetsService;
        private readonly SheetSettings _settings;
        private readonly IPartnerConfigurationResolver _partnerConfigurationResolver;
        private static readonly string FinancialYear = "25-26";
        private static readonly string Prefix = "KT";

        public InvoiceNumberGenerator(IGoogleSheetsService sheetsService, 
                                        IOptions<SheetSettings> options,
                                        IPartnerConfigurationResolver partnerConfigurationResolver)
        {
            _sheetsService = sheetsService;
            _settings = options.Value;
            _partnerConfigurationResolver = partnerConfigurationResolver;
        }

        public async Task<string> GenerateNextInvoiceNumberAsync(string partnerName)
        {
            var _sheetConfig = _partnerConfigurationResolver.GetSettings(partnerName).SheetSettings;
            // var _sheetIDConfig = _partnerConfigurationResolver.GetSettings(partnerName).SheetSettings.SpreadsheetId;
            string sheetName = _sheetConfig.Sheets["BillHistory"];

            string spreadsheetId = _sheetConfig.SpreadsheetId;

            // Column C contains invoice numbers
            string range = $"{sheetName}!C:C";
            var result = await _sheetsService.GetSheetValuesAsync(spreadsheetId, range);

            // Flatten and filter out empty/null rows
           var allInvoiceNumbers = result
            .Select(row => row.FirstOrDefault())
            .OfType<string>()
            .Where(val => !string.IsNullOrWhiteSpace(val))
            .ToList();

            if (allInvoiceNumbers.Count() == 0)
            {
                return $"{Prefix}/{FinancialYear}/0001";
            }

            string lastInvoice = allInvoiceNumbers.Last();

            // Match format: KT/25-26/0004
            var match = Regex.Match(lastInvoice, @$"^{Prefix}/{FinancialYear}/(\d{{4}})$");

            int lastSerial = match.Success ? int.Parse(match.Groups[1].Value) : 0;

            string nextSerial = (lastSerial + 1).ToString("D4");

            return $"{Prefix}/{FinancialYear}/{nextSerial}";
        }
    }
}