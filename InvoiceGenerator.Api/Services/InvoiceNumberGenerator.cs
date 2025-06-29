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

        private static readonly string FinancialYear = "25-26";
        private static readonly string Prefix = "KT";

        public InvoiceNumberGenerator(IGoogleSheetsService sheetsService, IOptions<SheetSettings> options)
        {
            _sheetsService = sheetsService;
            _settings = options.Value;
        }

        public async Task<string> GenerateNextInvoiceNumberAsync()
        {
            string sheetName = _settings.Sheets["BillHistory"];
            string spreadsheetId = _settings.SpreadsheetId;

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