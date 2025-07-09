using InvoiceGenerator.Api.Models;

namespace InvoiceGenerator.Api.Services.Interfaces;

public interface ICustomerInfoService
{
    Task AddCustomerAsync(CustomerInfo customer);
    Task<IList<CustomerInfo>> GetAllCustomersAsync(string partnerName);
    Task<CustomerInfo?> GetCustomerByNameAsync(string name, string partnerName);
    public Task UpdateCustomerByNameAsync(string name, CustomerInfo updated, string partnerName);
    Task DeleteCustomerAsync(string gstNo);
}