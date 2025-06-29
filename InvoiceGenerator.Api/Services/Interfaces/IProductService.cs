using InvoiceGenerator.Api.Models;

namespace InvoiceGenerator.Api.Services.Interfaces;

public interface IProductService
{
    Task<IList<Product>> GetAllProductsAsync();
    Task AddProductAsync(Product product);
}