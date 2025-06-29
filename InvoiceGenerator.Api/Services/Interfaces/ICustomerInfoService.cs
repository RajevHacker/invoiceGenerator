using InvoiceGenerator.Api.Models;

namespace InvoiceGenerator.Api.Services.Interfaces;

public interface ICustomerInfoService
{
    Task AddCustomerAsync(CustomerInfo customer);
    Task<IList<CustomerInfo>> GetAllCustomersAsync();
    Task<CustomerInfo?> GetCustomerByNameAsync(string gstNo);
    public Task UpdateCustomerByNameAsync(string name, CustomerInfo updated);
    Task DeleteCustomerAsync(string gstNo);
}