using InvoiceGenerator.Api.Models;

namespace InvoiceGenerator.Api.Services.Interfaces;

public interface IGetInvoiceSummary
{
    public Task<BillHistoryEntry> GetInvoiceSummaryAsync();
}
