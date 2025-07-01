using InvoiceGenerator.Api.Models;

namespace InvoiceGenerator.Api.Services.Interfaces;

public interface IInvoiceSheetWriter
{
    Task WriteInvoiceToSheetAsync(string partnerName, InvoiceRequest request);
    Task ClearInvoiceFieldsAsync(string partnerName);
}