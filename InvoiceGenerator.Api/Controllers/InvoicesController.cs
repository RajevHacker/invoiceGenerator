using InvoiceGenerator.Api.Models;
using InvoiceGenerator.Api.Models.InvoiceSummary;
using InvoiceGenerator.Api.Services;
using InvoiceGenerator.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.Bcpg.Sig;
using System.Reflection.Metadata.Ecma335;

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
    private readonly ILogger<InvoicesController> _logger;
    private readonly IInvoiceCancellationService _invoiceCancellationService;
    private readonly ISearchValueService _searchValueService;
    private readonly IGetBillHistortyInfo _getInvoiceDetail;
    private readonly IpurchaseOrderEntryService _purchaseOrderEntry;
    private readonly IAddPurchaseConsumerRecord _addPurchaseConsumerRecord;
    private readonly IpurchaseInvoiceList _purchaseInvoiceList;
    private readonly IPurchaseCustomerService _purchaseCustomerService;
    private readonly IGetPurchaseList _getPurchaseList;
    private readonly IGetSalesList _getSalesList;
    private readonly IInvoiceNumberGenerator _invoiceNumberGenerator;
    private readonly IGetRecentPaymentTransaction _getRecentPaymentTransaction;
    
    public InvoicesController(IPaymentSheetService paymentSheetService, IBillHistorySheetService billHistorySheetService,
                             ICustomerInfoService customerService, IProductService productService, 
                             InvoiceService invoiceService, ILogger<InvoicesController> logger, 
                             IInvoiceCancellationService invoiceCancellation, ISearchValueService searchValueService, 
                             IGetBillHistortyInfo getBillHistoryInfo, IpurchaseOrderEntryService purchaseOrder, 
                             IAddPurchaseConsumerRecord addPurchaseConsumerRecord, IpurchaseInvoiceList purchaseInvList, 
                             IPurchaseCustomerService purchaseCustomerService, IGetPurchaseList getPurchaseList,
                             IGetSalesList getSalesList, IInvoiceNumberGenerator invoiceNumberGenerator,
                             IGetRecentPaymentTransaction getRecentPaymentTransaction) 
    {
        _paymentSheetService = paymentSheetService;
        _billHistorySheetService = billHistorySheetService;
        _customerService = customerService;
        _productService = productService;
        _invoiceService = invoiceService;
        _logger = logger;
        _invoiceCancellationService = invoiceCancellation;
        _searchValueService = searchValueService;
        _getInvoiceDetail = getBillHistoryInfo;
        _purchaseOrderEntry = purchaseOrder;
        _addPurchaseConsumerRecord =  addPurchaseConsumerRecord;
        _purchaseInvoiceList = purchaseInvList;
        _purchaseCustomerService = purchaseCustomerService;
        _getPurchaseList = getPurchaseList;
        _getSalesList = getSalesList;
        _invoiceNumberGenerator = invoiceNumberGenerator;
        _getRecentPaymentTransaction = getRecentPaymentTransaction;
    }

    [HttpPost("PaymentEntry")]
    public async Task<IActionResult> AddPayment([FromQuery] string partnerName, [FromBody] PaymentEntry payment, [FromQuery] string paymentType)
    {
        await _paymentSheetService.AppendPaymentAsync(partnerName, payment, paymentType);

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
    public async Task<IActionResult> AddCustomer([FromBody] CustomerInfo customer, [FromQuery] string partnerName)
    {
        await _customerService.AddCustomerAsync(customer, partnerName);
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
    public async Task<IActionResult> UpdateCustomer([FromBody] CustomerInfo updatedCustomerData, [FromQuery] string partnerName)
    {
        string name = updatedCustomerData.Name;
        await _customerService.UpdateCustomerByNameAsync(name, updatedCustomerData, partnerName);
        return Ok();
    }

    [HttpDelete("gst/{gstNo}")]
    public async Task<IActionResult> DeleteCustomer(string gstNo)
    {
        await _customerService.DeleteCustomerAsync(gstNo);
        return Ok();
    }
    // [HttpGet("getAllProducts")]
    // public async Task<IActionResult> GetAll()
    // {
    //     var products = await _productService.GetAllProductsAsync();
    //     return Ok(products);
    // }

    [HttpPost("addProduct")]
    public async Task<IActionResult> Add([FromBody] Product product, [FromQuery] string partnerName)
    {
        if (string.IsNullOrWhiteSpace(product.Name))
            return BadRequest("Product name is required");

        await _productService.AddProductAsync(product, partnerName);
        return Ok();
    }
    // To see in the UI what Invoice is going to get cancelled
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
    [HttpPost("AddPurchaseOrder")]
    public async Task<IActionResult> addPurchaseOrder([FromQuery] string partnerName, [FromBody] purchaseOrderEntry entry)
    {
        await _purchaseOrderEntry.AppendPurchaseOrderAsync(partnerName, entry);
        return Ok("Added Purchase Order.");
    }
    [HttpPost("AddPurchaseConsumer")]
    public async Task<IActionResult> addPurchaseConsumer([FromQuery] string partnerName, [FromBody] purchaseOrderConsumer consumerDetail)
    {           
        await _addPurchaseConsumerRecord.AppendPurchaseOrderAsync(partnerName, consumerDetail);
        return Ok("Consumer Added");
    }
    [HttpGet("getPurchaseCustomerGST")]
    public async Task<IActionResult> getPurchaseConsumerGST([FromQuery] string consumerName, [FromQuery] string partnerName)
    {
        var customerDetail = await _purchaseCustomerService.SearchCustomersAsync(partnerName,consumerName);
        return Ok(customerDetail);
    }
    [HttpPatch("updatePurchaseCustomerGST")]
    public async Task<IActionResult> upsertPurchaseConsumerGST([FromQuery] string partnerName, [FromBody] purchaseCustomerDetails consumerDetails)
    {
        await _purchaseCustomerService.UpsertCustomerAsync(partnerName,consumerDetails);
        return Ok();
    }
    [HttpGet("purchasePaymentPending")]
    public async Task<IActionResult> GetUnpaidInvoices([FromQuery] string customerName, [FromQuery] string partnerName)
    {
        if (string.IsNullOrWhiteSpace(customerName))
            return BadRequest("Customer name is required.");
        var invoices = await _purchaseInvoiceList.GetUnpaidOrPartiallyPaidInvoicesAsync(customerName, partnerName);
        return Ok(invoices);
    }
    [HttpGet("GetPurchaseList")]
    public async Task<IActionResult> GetPurchaseList([FromQuery] string partnerName,
        [FromQuery] string? customerName,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        var result = await _getPurchaseList.getPurchaseList(partnerName, customerName, startDate, endDate);
        return Ok(result);
    }
    [HttpGet("GetSalesList")]
    public async Task<IActionResult> GetSalesList([FromQuery] string partnerName,
        [FromQuery] string? customerName,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        System.Console.WriteLine("SALESLIST");
        var result = await _getSalesList.getSalesList(partnerName, customerName, startDate, endDate);
        string jsonResult = JsonSerializer.Serialize(result, new JsonSerializerOptions
        {
            WriteIndented = true // Makes it pretty-printed in logs
        });
        Console.WriteLine("Sales List Result:\n" + jsonResult);
        return Ok(result);
    }
    [HttpGet("GetInvoiceNumber")]
    public async Task<string> GetNextInvoiceNumber(string partnerName)
    {
        var invoiceNumber = await _invoiceNumberGenerator.GenerateNextInvoiceNumberAsync(partnerName);
        return invoiceNumber;
    }
    [HttpGet("GetRecentPaymentTransaction")]
    public async Task<IActionResult> GetRecentPaymentTransaction(string partnerName, string paymentType)
    {
        var result = await _getRecentPaymentTransaction.getPaymentReport(partnerName,paymentType);
        return Ok(result);
    }
}
