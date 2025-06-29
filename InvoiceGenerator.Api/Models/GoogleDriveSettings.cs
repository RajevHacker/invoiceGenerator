namespace InvoiceGenerator.Api.Models;
public class GoogleDriveSettings
{
    public string GeneratedInvoicesFolderId { get; set; } = default!;
    public string CancelledInvoicesFolderId { get; set; } = default!;
}