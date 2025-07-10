namespace InvoiceGenerator.Api.Models.InvoiceSummary;

public class purchaseOrderEntry
{
    public string CustomerName { get; set; }
    public string? GSTNumber { get; set; }
    public string? InvoiceNumber { get; set; }
    public DateOnly? Date { get; set; }
    public string hsnCode { get; set; }
    public decimal? TotalBeforeGST { get; set; }
    public int? Qty { get; set; }
    public decimal? CGST { get; set; }
    public decimal? SGST { get; set; }
    public decimal? GrandTotal { get; set; }
}
