using InvoiceGenerator.Api.Models.InvoiceSummary;

namespace InvoiceGenerator.Api.Services.Interfaces;

public interface IpurchaseOrderEntryService
{
    Task AppendPurchaseOrderAsync(string partnerName, purchaseOrderEntry entry);
}
