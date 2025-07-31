using InvoiceGenerator.Api.Models;
using InvoiceGenerator.Api.Services.Interfaces;

namespace InvoiceGenerator.Api.Services;

 public class GetDashboardSummaryService : IGetDashboardSummaryService
{
    public readonly IPartnerConfigurationResolver _partnerConfigurationResolver;
    private readonly IGoogleSheetsService _googleSheetsService;
    public GetDashboardSummaryService(IPartnerConfigurationResolver partnerResolver, IGoogleSheetsService googleSheetsService)
    {
        _partnerConfigurationResolver = partnerResolver;
        _googleSheetsService = googleSheetsService;
    }

    public async Task<DashboardSummary> GetDashboardSummaryAsync(string partnerName)
    {
        var _config = _partnerConfigurationResolver.GetSettings(partnerName).SheetSettings;
        string spreadsheetId = _config.SpreadsheetId;
        string userSheetName = _config.Sheets["User"];
        string range = $"{userSheetName}!C1:D2";
        // GET THE DATA FROM USER
        var values = await _googleSheetsService.ReadRangeAsync(spreadsheetId, range);
        decimal totalOutstanding = decimal.TryParse(values[0][1]?.ToString(), out decimal o) ? o : 0m;
        decimal totalPurchasePaymentPending = decimal.TryParse(values[1][1]?.ToString(), out decimal p) ? p : 0m;
        
        string customerDetailsSheetName = _config.Sheets["CustomerDetails"];
        string custDetailsRange = $"{customerDetailsSheetName}!A2:H";
        var custDetailsValues = await _googleSheetsService.ReadRangeAsync(spreadsheetId, custDetailsRange);

        var salesDuePayment = new List<DuePayment>();
        foreach (var row in custDetailsValues)
        {
            // Ensure the row has enough columns for Name (index 0) and Outstanding Balance (index 7)
            if (row.Count >= 8)
            {
                // Extract the name (column A)
                string name = row[0]?.ToString() ?? string.Empty;

                // Extract and parse the outstanding balance (column H)
                decimal balance = decimal.TryParse(row[7]?.ToString(), out decimal parsedBalance) ? parsedBalance : 0m;
                
                // Add the data to your list
                if (balance > 0)
                {
                    salesDuePayment.Add(new DuePayment
                    {
                        Name = name,
                        Amount = balance
                    });
                }
            }
        }

        string purchaseDetailsSheetName = _config.Sheets["PurchaseCustomer"];
        string purchaseDetailsRange = $"{purchaseDetailsSheetName}!A2:C";
        var purchaseDetailsValues = await _googleSheetsService.ReadRangeAsync(spreadsheetId, purchaseDetailsRange);

        var purchaseDuePayment = new List<DuePayment>();
        foreach (var row in purchaseDetailsValues)
        {
            // Ensure the row has enough columns for Name (index 0) and Outstanding Balance (index 7)
            if (row.Count >= 3)
            {
                // Extract the name (column A)
                string name = row[0]?.ToString() ?? string.Empty;

                // Extract and parse the outstanding balance (column H)
                decimal balance = decimal.TryParse(row[2]?.ToString(), out decimal parsedBalance) ? parsedBalance : 0m;
                
                // Add the data to your list
                if (balance > 0)
                {
                    purchaseDuePayment.Add(new DuePayment
                    {
                        Name = name,
                        Amount = balance
                    });
                }
            }
        }
        
        // Create a hardcoded instance of the DashboardSummary model
        var dashboardData = new DashboardSummary
        {
            TotalOutstanding = totalOutstanding,
            TotalPurchasePaymentPending = totalPurchasePaymentPending,
            TotOutstandingPayment = salesDuePayment,
            VendorPaymentDues = purchaseDuePayment
        };

        // Return the hardcoded data wrapped in a completed Task
        // Task.FromResult is the correct way to return an already-computed result from an async method.
        return await Task.FromResult(dashboardData);
    }
}