using System.Text;
using InvoiceGenerator.Api.Models;
using InvoiceGenerator.Api.Services;
using InvoiceGenerator.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

// Force default environment to Development if not set
var envName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
              ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
              ?? "Development";

Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", envName);
Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", envName);

var builder = WebApplication.CreateBuilder(args);

var env = builder.Environment;
var key = Environment.GetEnvironmentVariable("CONFIG_KEY");

// Load encrypted config from env or file
var encryptedJson = Environment.GetEnvironmentVariable("ENCRYPTED_JSON_CONFIG");
if (string.IsNullOrEmpty(encryptedJson))
    throw new Exception("ENCRYPTED_JSON_CONFIG is not set.");

// If ENCRYPTED_JSON_CONFIG points to a file, use file decrypt; otherwise, treat it as raw content
string decryptedJson;
// if (File.Exists(encryptedJson))
// {
    decryptedJson = BlowFishDecryption.JsonDecryptor.DecryptFile(encryptedJson, key);
// }
// else
// {
//     decryptedJson = BlowFishDecryption.JsonDecryptor.DecryptContent(encryptedJson, key); // You’ll need to implement this overload
// }

var decryptedStream = new MemoryStream(Encoding.UTF8.GetBytes(decryptedJson));
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
    .AddJsonStream(decryptedStream);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// Now bind JwtSettings after the decrypted config is loaded
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();

// Validate JwtSettings to avoid null reference exceptions
if (jwtSettings == null || string.IsNullOrEmpty(jwtSettings.SecretKey))
{
    throw new Exception("JWT settings are not configured properly.");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// Register other settings now
builder.Services.Configure<SheetSettings>(builder.Configuration.GetSection("SheetSettings"));
builder.Services.Configure<GoogleDriveSettings>(builder.Configuration.GetSection("GoogleDriveSettings"));
builder.Services.Configure<DriveOAuthCred>(builder.Configuration.GetSection("DriveOAuthCred"));

// Register your services
builder.Services.AddHttpClient();
builder.Services.AddSingleton<GoogleAuthorizationService>();
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
builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddSingleton<IPartnerConfigurationResolver, PartnerConfigurationResolver>();
builder.Services.AddTransient<ISearchValueService, searchValueService>();
builder.Services.AddTransient<IGetBillHistortyInfo, GetBillHistortyInfoService>();
builder.Services.AddTransient<IpurchaseOrderEntryService, purchaseOrderEntryService>();
builder.Services.AddTransient<IAddPurchaseConsumerRecord, addPurchaseConsumerRecord>();
builder.Services.AddTransient<IpurchaseInvoiceList,purchaseInvoiceListService>();
builder.Services.AddTransient<IPurchaseCustomerService, PurchaseCustomerService>();
builder.Services.AddTransient<IGetPurchaseList, getPurchaseListService>();
builder.Services.AddTransient<IGetSalesList, getSalesListService>();
builder.Services.AddTransient<IGetRecentPaymentTransaction, GetRecentPaymentTransaction>();
builder.Services.AddTransient<IGetDashboardSummaryService, GetDashboardSummaryService>();
builder.Services.AddTransient<IsalesInvoiceList, salesInvoiceListService>();
builder.Services.AddTransient<IresetFinancialYearInterface, resetFinancialYearService>();

builder.Services.AddAuthorization();
builder.Services.AddControllers();

var app = builder.Build();
app.UseCors("AllowAll");
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
