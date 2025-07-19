using InvoiceGenerator.Api.Models.InvoiceSummary;

namespace InvoiceGenerator.Api.Services.Interfaces;

public interface IGetPurchaseList
{
    public Task<IList<purchaseOrderEntry>> getPurchaseList(string partnerName, string? consumerName, DateTime? startDate, DateTime? endDate);
}
