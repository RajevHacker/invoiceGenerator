using InvoiceGenerator.Api.Models;

namespace InvoiceGenerator.Api.Services.Interfaces;

public interface IAddPurchaseConsumerRecord
{
    Task AppendPurchaseOrderAsync(string partnerName, purchaseOrderConsumer poConsumer);
}
