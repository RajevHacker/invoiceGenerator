namespace InvoiceGenerator.Api.Models;

public class purchaseInvoiceList
{
    public string CustomerName { get; set; } = string.Empty;
    public string InvoiceNumber { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public decimal GrandTotal { get; set; }
    public decimal BalanceAmount { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
}