using System.Text;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using InvoiceGenerator.Api.Models;
using InvoiceGenerator.Api.Services;
using InvoiceGenerator.Api.Services.Interfaces;
using Newtonsoft.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var env = builder.Environment;
var key = Environment.GetEnvironmentVariable("CONFIG_KEY");

var decryptedJson = BlowFishDecryption.JsonDecryptor.DecryptFile($"secure.{env.EnvironmentName}.appsettings.json", key);
var decryptedStream = new MemoryStream(Encoding.UTF8.GetBytes(decryptedJson));
builder.Services.AddHttpClient();
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
    .AddJsonStream(decryptedStream); 
builder.Services.Configure<SheetSettings>(
    builder.Configuration.GetSection("SheetSettings"));
builder.Services.Configure<GoogleDriveSettings>(
    builder.Configuration.GetSection("GoogleDriveSettings"));
builder.Services.Configure<GoogleApiSettings>(
    builder.Configuration.GetSection("GoogleApi")
);
builder.Services.AddSingleton(sp =>
{
    var factory = sp.GetRequiredService<GoogleServiceFactory>();
    return factory.CreateSheetsService();
});// ToDO: I am not sure why we have this kind
builder.Services.AddScoped<DriveService>(provider =>
{
    var factory = provider.GetRequiredService<GoogleServiceFactory>();
    return factory.CreateDriveService();
});

builder.Services.AddSingleton<GoogleServiceFactory>(); 
builder.Services.AddTransient<IGoogleSheetsService, GoogleSheetsService>();
builder.Services.AddTransient<IPaymentSheetService, PaymentSheetService>();
builder.Services.AddTransient<IBillHistorySheetService, BillHistorySheetService>();
builder.Services.AddTransient<ICustomerInfoService, CustomerInfoService>();
builder.Services.AddTransient<IProductService, ProductService>();
builder.Services.AddTransient<IInvoiceSheetWriter, InvoiceSheetWriter>();
builder.Services.AddTransient<InvoiceService>();
builder.Services.AddSingleton<IInvoiceNumberGenerator, InvoiceNumberGenerator>();
builder.Services.AddTransient<IFileDownloader, FileDownloader>();
builder.Services.AddScoped<IGoogleSheetsExporter, GoogleSheetsExporter>();
builder.Services.AddScoped<IDriveUploader, GoogleDriveUploader>();
builder.Services.AddScoped<IInvoicePdfExporter, InvoicePdfExporter>();
builder.Services.AddTransient<IGetInvoiceSummary, GetInvoiceSummaryService>();
builder.Services.AddTransient<IInvoiceCancellationService, InvoiceCancellationService>();
builder.Services.AddAuthentication(); // Add config if needed
builder.Services.AddAuthorization();
builder.Services.AddControllers();

var app = builder.Build();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
//CONFIG_KEY=your_secret_key ASPNETCORE_ENVIRONMENT=Development dotnet run