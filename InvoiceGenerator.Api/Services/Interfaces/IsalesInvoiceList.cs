using InvoiceGenerator.Api.Models;

namespace InvoiceGenerator.Api.Services.Interfaces;

public interface IsalesInvoiceList
{
    Task<List<purchaseInvoiceList>> GetUnpaidOrPartiallyPaidInvoicesAsync(string customerName, string partnerName);
}