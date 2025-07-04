using Microsoft.AspNetCore.Mvc;

namespace InvoiceGenerator.Api.Services.Interfaces;

public interface ISearchValueService
{
    Task<List<string>> SearchValueAsync(string partnerName, string sheetName, string searchValue);
}
