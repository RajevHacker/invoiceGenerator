using InvoiceGenerator.Api.Models;
using InvoiceGenerator.Api.Services;
using InvoiceGenerator.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.Bcpg.Sig;

namespace InvoiceGenerator.Api.Controllers;
[Authorize]
[ApiController]
[Route("[controller]")]
public class InvoicesController : ControllerBase
{
    private readonly IPaymentSheetService _paymentSheetService;
    private readonly IBillHistorySheetService _billHistorySheetService;
    private readonly ICustomerInfoService _customerService;
    private readonly IProductService _productService;
    private readonly InvoiceService _invoiceService;
    private readonly IInvoicePdfExporter _pdfExporter;
    private readonly ILogger<InvoicesController> _logger;
    private readonly IInvoiceCancellationService _invoiceCancellationService;
    private readonly ISearchValueService _searchValueService;
    private readonly IGetBillHistortyInfo _getInvoiceDetail;
    public InvoicesController(IPaymentSheetService paymentSheetService, IBillHistorySheetService billHistorySheetService, ICustomerInfoService customerService, IProductService productService, InvoiceService invoiceService, IInvoicePdfExporter pdfExporter, ILogger<InvoicesController> logger, IInvoiceCancellationService invoiceCancellation, ISearchValueService searchValueService, IGetBillHistortyInfo getBillHistoryInfo) 
    {
        _paymentSheetService = paymentSheetService;
        _billHistorySheetService = billHistorySheetService;
        _customerService = customerService;
        _productService = productService;
        _invoiceService = invoiceService;
        _pdfExporter = pdfExporter;
        _logger = logger;
        _invoiceCancellationService = invoiceCancellation;
        _searchValueService = searchValueService;
        _getInvoiceDetail = getBillHistoryInfo;
    }

    [HttpPost("PaymentEntry")]
    public async Task<IActionResult> AddPayment([FromQuery] string partnerName, [FromBody] PaymentEntry payment)
    {
        await _paymentSheetService.AppendPaymentAsync(partnerName, payment);

        return Ok("Payment added successfully.");
    }

    [HttpPost("InvoiceGenerator")]
    public async Task<IActionResult> CreateInvoice([FromQuery] string partnerName, [FromBody] InvoiceRequest request)
    {
        var InvoiceResponse = await _invoiceService.ProcessInvoiceAsync(partnerName, request);
        return Ok(InvoiceResponse);
    }

    [HttpPost("BillHistoryEntry")]
    public async Task<IActionResult> AppendBillHistory([FromQuery] string partnerName, [FromBody] BillHistoryEntry entry)
    {
        await _billHistorySheetService.AppendBillHistoryAsync(partnerName, entry);
        return Ok("Bill history entry added.");
    }
    [HttpPost("addCustomer")]
    public async Task<IActionResult> AddCustomer([FromBody] CustomerInfo customer)
    {
        await _customerService.AddCustomerAsync(customer);
        return Ok();
    }

    // [HttpGet("getAllCustomer")]
    // public async Task<ActionResult<IList<CustomerInfo>>> GetAllCustomers()
    // {
    //     return Ok(await _customerService.GetAllCustomersAsync());
    // }

    [HttpGet("getCustomerByName")]
    public async Task<ActionResult<CustomerInfo>> GetCustomerByCusName([FromQuery] string customerName,[FromQuery] string partnerName)
    {
        var customer = await _customerService.GetCustomerByNameAsync(customerName, partnerName);
        if (customer == null)
        {
            return Ok("Customer not found");
        }
        return Ok(customer);
    }

    [HttpPut("updateCustomerDetail")]
    public async Task<IActionResult> UpdateCustomer([FromBody] CustomerInfo updatedCustomerData)
    {
        string name = updatedCustomerData.Name;
        await _customerService.UpdateCustomerByNameAsync(name, updatedCustomerData);
        return Ok();
    }

    [HttpDelete("gst/{gstNo}")]
    public async Task<IActionResult> DeleteCustomer(string gstNo)
    {
        await _customerService.DeleteCustomerAsync(gstNo);
        return Ok();
    }
    [HttpGet("getAllProducts")]
    public async Task<IActionResult> GetAll()
    {
        var products = await _productService.GetAllProductsAsync();
        return Ok(products);
    }

    [HttpPost("addProduct")]
    public async Task<IActionResult> Add([FromBody] Product product)
    {
        if (string.IsNullOrWhiteSpace(product.Name))
            return BadRequest("Product name is required");

        await _productService.AddProductAsync(product);
        return Ok();
    }
    [HttpGet("getCancelInvoiceDetails")]
    public async Task<IActionResult> getCancelInvoiceDetails([FromQuery] string invoiceNumber, [FromQuery] string partnerName)
    {
        var a = await _getInvoiceDetail.GetBillHistoryInfo(invoiceNumber,partnerName);
        return Ok(a);
    }
    [HttpPost("cancel")]
    public async Task<IActionResult> CancelInvoice([FromQuery] string invoiceNumber, [FromQuery] string partnerName)
    {
        var result = await _invoiceCancellationService.CancelInvoiceAsync(invoiceNumber, partnerName);
        if (!result) return NotFound("Invoice not found or already cancelled.");
        return Ok("Invoice cancelled successfully.");
    }
    [HttpGet("SearchCustomers")]
    public async Task<IActionResult> SearchCustomers([FromQuery] string partnerName, [FromQuery] string sheetName,
        [FromQuery] string searchValue)
    {
        var result = await _searchValueService.SearchValueAsync(partnerName, sheetName, searchValue);
        return Ok(result);
    }
}
