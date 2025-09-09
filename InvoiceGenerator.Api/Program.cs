using System.Text;
using InvoiceGenerator.Api.Models;
using InvoiceGenerator.Api.Services;
using InvoiceGenerator.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var env = builder.Environment;
var key = Environment.GetEnvironmentVariable("CONFIG_KEY");

if (string.IsNullOrEmpty(key))
    throw new Exception("CONFIG_KEY is not set.");

// --- Load and decrypt config file ---
var secureFilePath = Path.Combine(AppContext.BaseDirectory, "Config", $"secure.{env.EnvironmentName}.appsettings.json");

if (!File.Exists(secureFilePath))
    throw new FileNotFoundException($"Secure config file not found: {secureFilePath}");

var decryptedJson = BlowFishDecryption.JsonDecryptor.DecryptFile(secureFilePath, key);
var decryptedStream = new MemoryStream(Encoding.UTF8.GetBytes(decryptedJson));

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
    .AddJsonStream(decryptedStream);

// --- CORS ---
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

// --- JWT ---
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();

if (jwtSettings == null || string.IsNullOrEmpty(jwtSettings.SecretKey))
    throw new Exception("JWT settings are not configured properly.");

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

// --- Other services ---
builder.Services.Configure<SheetSettings>(builder.Configuration.GetSection("SheetSettings"));
builder.Services.Configure<GoogleDriveSettings>(builder.Configuration.GetSection("GoogleDriveSettings"));
builder.Services.Configure<DriveOAuthCred>(builder.Configuration.GetSection("DriveOAuthCred"));

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
builder.Services.AddTransient<IpurchaseInvoiceList, purchaseInvoiceListService>();
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
