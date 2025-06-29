using InvoiceGenerator.Api.Models;

namespace InvoiceGenerator.Api.Services.Interfaces;

public interface IInvoiceSheetWriter
{
    Task WriteInvoiceToSheetAsync(InvoiceRequest request);
    Task ClearInvoiceFieldsAsync();
}