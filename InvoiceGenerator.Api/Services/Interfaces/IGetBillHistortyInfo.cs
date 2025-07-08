using InvoiceGenerator.Api.Models;

namespace InvoiceGenerator.Api.Services.Interfaces;

public interface IGetBillHistortyInfo
{
    Task<BillHistoryEntry> GetBillHistoryInfo(string invoiceNumber, string partnerName);
}
