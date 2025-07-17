using InvoiceGenerator.Api.Models;

namespace InvoiceGenerator.Api.Services.Interfaces;

public interface IPurchaseCustomerService
{
    Task<List<purchaseCustomerDetails>> SearchCustomersAsync(string partnerName, string query);
    Task UpsertCustomerAsync(string partnerName, purchaseCustomerDetails customer);
}
