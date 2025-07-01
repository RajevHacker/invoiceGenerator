using InvoiceGenerator.Api.Models;

namespace InvoiceGenerator.Api.Services.Interfaces;

public interface IBillHistorySheetService
{
    Task AppendBillHistoryAsync(string partnerName, BillHistoryEntry entry);
}