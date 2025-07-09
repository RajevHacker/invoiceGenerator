using InvoiceGenerator.Api.Models;
using InvoiceGenerator.Api.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace InvoiceGenerator.Api.Services;

public class ProductService : IProductService
{
    private readonly IGoogleSheetsService _googleSheetsService;
    private readonly SheetSettings _sheetSettings;
    private readonly IPartnerConfigurationResolver _partnerConfig;

    public ProductService(IGoogleSheetsService googleSheetsService, IOptions<SheetSettings> options, IPartnerConfigurationResolver partnerConfig)
    {
        _googleSheetsService = googleSheetsService;
        _sheetSettings = options.Value;
        _partnerConfig = partnerConfig;
    }

    public async Task<IList<Product>> GetAllProductsAsync()
    {
        var spreadsheetId = _sheetSettings.SpreadsheetId;
        var sheetName = _sheetSettings.Sheets["Products"];
        var range = $"{sheetName}!A2:A";

        var values = await _googleSheetsService.GetSheetValuesAsync(spreadsheetId, range);

        return values?
            .Where(row => row.Count > 0)
            .Select(row => new Product { Name = row[0]?.ToString() ?? "" })
            .ToList()
            ?? new List<Product>();
    }

    public async Task AddProductAsync(Product product, string partnerName)
    {
        var _SheetConfig = _partnerConfig.GetSettings(partnerName).SheetSettings;
        var spreadsheetId = _SheetConfig.SpreadsheetId;
        var sheetName = _SheetConfig.Sheets["Products"];
        var range = $"{sheetName}!A1";

        var row = new List<object> { product.Name ?? "" };
        await _googleSheetsService.AppendRowAsync(spreadsheetId, range, row);
    }
}