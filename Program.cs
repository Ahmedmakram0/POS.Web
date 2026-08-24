using System.Globalization;
using System.Text.Json.Serialization;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using POS.Web.Authorization;
using POS.Web.Data;
using POS.Web.Models.Identity;
using POS.Web.Services.Catalog;
using POS.Web.Services.Customers;
using POS.Web.Services.Financial;
using POS.Web.Services.Media;
using POS.Web.Services.Purchasing;
using POS.Web.Services.Reporting;
using POS.Web.Services.Sales;
using POS.Web.Services.Settings;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(options => options.Filters.Add<PageAccessFilter>())
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Host=localhost;Database=POSWebDb;Username=posweb;Password=posweb"));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequiredLength = 8;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.SignIn.RequireConfirmedEmail = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

builder.Services.AddAuthorization();

builder.Services.AddAntiforgery(options => options.HeaderName = "X-CSRF-TOKEN");

builder.Services.AddScoped<IFinancialAccountService, FinancialAccountService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IStoreService, StoreService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<ISaleService, SaleService>();
builder.Services.AddScoped<ISupplierService, SupplierService>();
builder.Services.AddScoped<IPurchaseInvoiceService, PurchaseInvoiceService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<ISettingsService, SettingsService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

builder.Services.AddSingleton(_ =>
{
    var cloudinaryConfig = builder.Configuration.GetSection("Cloudinary");
    var account = new Account(
        cloudinaryConfig["CloudName"],
        cloudinaryConfig["ApiKey"],
        cloudinaryConfig["ApiSecret"]);
    return new Cloudinary(account);
});
builder.Services.AddScoped<IProductImageService, CloudinaryProductImageService>();

var app = builder.Build();

// Arabic-only, RTL culture across the app. Numbers stay Western-digit/dot-decimal so they
// round-trip correctly with HTML number inputs (which always submit "7.5", never "٧٫٥")
// and with the model binder, which parses using this same culture.
var arCulture = (CultureInfo)new CultureInfo("ar-EG").Clone();
arCulture.NumberFormat.NumberDecimalSeparator = ".";
arCulture.NumberFormat.NumberGroupSeparator = ",";
arCulture.NumberFormat.CurrencyDecimalSeparator = ".";
arCulture.NumberFormat.CurrencyGroupSeparator = ",";
arCulture.NumberFormat.DigitSubstitution = DigitShapes.None;
arCulture.NumberFormat.NativeDigits = ["0", "1", "2", "3", "4", "5", "6", "7", "8", "9"];
CultureInfo.DefaultThreadCurrentCulture = arCulture;
CultureInfo.DefaultThreadCurrentUICulture = arCulture;
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(arCulture),
    SupportedCultures = new[] { arCulture },
    SupportedUICultures = new[] { arCulture }
});

using (var scope = app.Services.CreateScope())
{
    await SeedData.InitializeAsync(scope.ServiceProvider);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // serves runtime-uploaded files (e.g. wwwroot/uploads) that MapStaticAssets' build-time manifest doesn't cover.
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
