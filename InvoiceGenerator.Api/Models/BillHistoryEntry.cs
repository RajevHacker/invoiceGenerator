namespace InvoiceGenerator.Api.Models;

public class BillHistoryEntry
{
    public string CustomerName { get; set; }
    public string? GSTNumber { get; set; }
    public string? InvoiceNumber { get; set; }
    public DateOnly? Date { get; set; }
    public decimal? TotalBeforeGST { get; set; }
    public int? Qty { get; set; }
    public decimal? CGST { get; set; }
    public decimal? SGST { get; set; }
    public decimal? IGST { get; set; }
    public decimal? GrandTotal { get; set; }
    // public int? InvoiceIndex { get; set; }
    // public decimal? TotalBillPerCustomer { get; set; }
    // public decimal? TotalPaidByCustomer { get; set; }
    // public string? CustomFormula { get; set; }
    // public string? StatusFlag { get; set; }
    // public string? PaymentStatus { get; set; }
    // public decimal? TotalBalance { get; set; }
}
