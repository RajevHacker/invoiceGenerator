using InvoiceGenerator.Api.Models;
namespace InvoiceGenerator.Api.Services.Interfaces;

public interface IGetSalesList
{
    public Task<IList<BillHistoryEntry>> getSalesList(string partnerName, string? consumerName, DateTime? startDate, DateTime? endDate);
}
