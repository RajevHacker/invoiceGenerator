using InvoiceGenerator.Api.Models;

namespace InvoiceGenerator.Api.Services.Interfaces;

public interface IGetDashboardSummaryService
{
    Task<DashboardSummary> GetDashboardSummaryAsync(string partnerName);
}
