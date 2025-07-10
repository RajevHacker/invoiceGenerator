namespace InvoiceGenerator.Api.Models;

public class CustomerInfo
{
    public string Name { get; set; }
    public string Address { get; set; }
    public string ContactNumber { get; set; }
    public string State { get; set; }
    public string StateCode { get; set; }
    public string GSTNo { get; set; }
    public string Email { get; set; }
    public string? custFormula { get; set; }
}