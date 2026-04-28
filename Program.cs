using Microsoft.EntityFrameworkCore;
using MyFirstApi.Data;
using MyFirstApi.DTOs;
using MyFirstApi.Services;
using Scalar.AspNetCore;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add Services 
builder.Services.AddSingleton<WeatherService>();
builder.Services.AddScoped<ItemService>();
builder.Services.AddScoped<ItemImportService>();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHangfireServer();
builder.Services.AddAntiforgery();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<PermissionService>();
builder.Services.AddScoped<VendorService>();
builder.Services.AddScoped<CustomerService>();
builder.Services.AddScoped<InventoryService>();
builder.Services.AddScoped<PurchaseService>();
builder.Services.AddScoped<SalesService>();
builder.Services.AddScoped<BomService>();
builder.Services.AddScoped<ReceiptService>();
builder.Services.AddScoped<ReportService>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorization();


var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}   

QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

app.UseHangfireDashboard("/jobs");
app.UseAntiforgery();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};



app.MapGet("/weatherforecast", (WeatherService weather) => weather.GenerateForecasts(5)).WithName("GetWeatherForecast");

app.MapGet("/weatherforecast/hot", (WeatherService weather, int minTemp = 25) =>
{
    var forecast = weather.GenerateForecasts();

    return forecast.Where(f => f.TemperatureC >= minTemp);
});

app.MapGet("/weatherforecast/summary/{word}", (WeatherService weather, string word) =>
{
    var forecast = weather.GenerateForecasts(100);

    return forecast.Where(f => f.Summary == word);
});



// Items endpoints
app.MapGet("/items", async (ItemService items) =>
    await items.GetAllAsync())
    .RequireAuthorization();

app.MapGet("/items/{id}", async (ItemService items, int id) =>
{
    var item = await items.GetByIdAsync(id);
    return item is null ? Results.NotFound() : Results.Ok(item);
})
    .RequireAuthorization();

app.MapPost("/items", async (ItemService items, ItemCreateDto dto) =>
{
    var created = await items.CreateAsync(dto);
    return Results.Created($"/items/{created.Id}", created);
})
    .RequireAuthorization();

app.MapPut("/items/{id}", async (ItemService items, int id, ItemUpdateDto dto) =>
{
    var updated = await items.UpdateAsync(id, dto);
    return updated is null ? Results.NotFound() : Results.Ok(updated);
});

app.MapDelete("/items/{id}", async (ItemService items, int id) =>
{
    var deleted = await items.DeleteAsync(id);
    return deleted ? Results.NoContent() : Results.NotFound();
});

// Vendors endpoints
app.MapGet("/vendors", async (VendorService vendors, PermissionService permissions, ClaimsPrincipal user) =>
{
    if (!await permissions.HasPermissionAsync(user, "Vendors", "View"))
        return Results.Forbid();

    return Results.Ok(await vendors.GetAllAsync());
})
    .RequireAuthorization();

app.MapGet("/vendors/{id}", async (VendorService vendors, PermissionService permissions, ClaimsPrincipal user, int id) =>
{
    if (!await permissions.HasPermissionAsync(user, "Vendors", "View"))
        return Results.Forbid();

    var vendor = await vendors.GetByIdAsync(id);
    return vendor is null ? Results.NotFound() : Results.Ok(vendor);
})
    .RequireAuthorization();

app.MapPost("/vendors", async (VendorService vendors, PermissionService permissions, ClaimsPrincipal user, VendorCreateDto dto) =>
{
    if (!await permissions.HasPermissionAsync(user, "Vendors", "Create"))
        return Results.Forbid();

    var created = await vendors.CreateAsync(dto);
    return Results.Created($"/vendors/{created.Id}", created);
})
    .RequireAuthorization();

app.MapPut("/vendors/{id}", async (VendorService vendors, PermissionService permissions, ClaimsPrincipal user, int id, VendorUpdateDto dto) =>
{
    if (!await permissions.HasPermissionAsync(user, "Vendors", "Edit"))
        return Results.Forbid();

    var updated = await vendors.UpdateAsync(id, dto);
    return updated is null ? Results.NotFound() : Results.Ok(updated);
})
    .RequireAuthorization();

app.MapDelete("/vendors/{id}", async (VendorService vendors, PermissionService permissions, ClaimsPrincipal user, int id) =>
{
    if (!await permissions.HasPermissionAsync(user, "Vendors", "Delete"))
        return Results.Forbid();

    var deleted = await vendors.DeleteAsync(id);
    return deleted ? Results.NoContent() : Results.NotFound();
})
    .RequireAuthorization();

// Customers endpoints
app.MapGet("/customers", async (CustomerService customers, PermissionService permissions, ClaimsPrincipal user) =>
{
    if (!await permissions.HasPermissionAsync(user, "Customers", "View"))
        return Results.Forbid();

    return Results.Ok(await customers.GetAllAsync());
})
    .RequireAuthorization();

app.MapGet("/customers/{id}", async (CustomerService customers, PermissionService permissions, ClaimsPrincipal user, int id) =>
{
    if (!await permissions.HasPermissionAsync(user, "Customers", "View"))
        return Results.Forbid();

    var customer = await customers.GetByIdAsync(id);
    return customer is null ? Results.NotFound() : Results.Ok(customer);
})
    .RequireAuthorization();

app.MapPost("/customers", async (CustomerService customers, PermissionService permissions, ClaimsPrincipal user, CustomerCreateDto dto) =>
{
    if (!await permissions.HasPermissionAsync(user, "Customers", "Create"))
        return Results.Forbid();

    var created = await customers.CreateAsync(dto);
    return Results.Created($"/customers/{created.Id}", created);
})
    .RequireAuthorization();

app.MapPut("/customers/{id}", async (CustomerService customers, PermissionService permissions, ClaimsPrincipal user, int id, CustomerUpdateDto dto) =>
{
    if (!await permissions.HasPermissionAsync(user, "Customers", "Edit"))
        return Results.Forbid();

    var updated = await customers.UpdateAsync(id, dto);
    return updated is null ? Results.NotFound() : Results.Ok(updated);
})
    .RequireAuthorization();

app.MapDelete("/customers/{id}", async (CustomerService customers, PermissionService permissions, ClaimsPrincipal user, int id) =>
{
    if (!await permissions.HasPermissionAsync(user, "Customers", "Delete"))
        return Results.Forbid();

    var deleted = await customers.DeleteAsync(id);
    return deleted ? Results.NoContent() : Results.NotFound();
})
    .RequireAuthorization();

// Inventory endpoints
app.MapGet("/inventory/{itemId}/stock", async (InventoryService inventory, PermissionService permissions, ClaimsPrincipal user, int itemId) =>
{
    if (!await permissions.HasPermissionAsync(user, "Inventory", "View"))
        return Results.Forbid();

    var stock = await inventory.GetStockLevelAsync(itemId);
    return stock is null ? Results.NotFound() : Results.Ok(stock);
})
    .RequireAuthorization();

app.MapGet("/inventory/{itemId}/ledger", async (InventoryService inventory, PermissionService permissions, ClaimsPrincipal user, int itemId) =>
{
    if (!await permissions.HasPermissionAsync(user, "Inventory", "View"))
        return Results.Forbid();

    var ledger = await inventory.GetLedgerAsync(itemId);
    return ledger is null ? Results.NotFound() : Results.Ok(ledger);
})
    .RequireAuthorization();

// Purchase endpoints
app.MapPost("/purchase", async (PurchaseService purchases, PermissionService permissions, ClaimsPrincipal user, PurchaseOrderCreateDto dto) =>
{
    if (!await permissions.HasPermissionAsync(user, "Purchase", "Create"))
        return Results.Forbid();

    var created = await purchases.CreateAsync(dto);
    return created is null ? Results.BadRequest() : Results.Created($"/purchase/{created.Id}", created);
})
    .RequireAuthorization();

app.MapGet("/purchase", async (PurchaseService purchases, PermissionService permissions, ClaimsPrincipal user) =>
{
    if (!await permissions.HasPermissionAsync(user, "Purchase", "View"))
        return Results.Forbid();

    return Results.Ok(await purchases.GetAllAsync());
})
    .RequireAuthorization();

app.MapGet("/purchase/{id}", async (PurchaseService purchases, PermissionService permissions, ClaimsPrincipal user, int id) =>
{
    if (!await permissions.HasPermissionAsync(user, "Purchase", "View"))
        return Results.Forbid();

    var order = await purchases.GetByIdAsync(id);
    return order is null ? Results.NotFound() : Results.Ok(order);
})
    .RequireAuthorization();

app.MapPut("/purchase/{id}", async (PurchaseService purchases, PermissionService permissions, ClaimsPrincipal user, int id, PurchaseOrderUpdateDto dto) =>
{
    if (!await permissions.HasPermissionAsync(user, "Purchase", "Edit"))
        return Results.Forbid();

    var updated = await purchases.UpdateAsync(id, dto);
    return updated is null ? Results.BadRequest() : Results.Ok(updated);
})
    .RequireAuthorization();

app.MapPut("/purchase/{id}/confirm", async (PurchaseService purchases, PermissionService permissions, ClaimsPrincipal user, int id) =>
{
    if (!await permissions.HasPermissionAsync(user, "Purchase", "Edit"))
        return Results.Forbid();

    var updated = await purchases.ConfirmAsync(id);
    return updated is null ? Results.BadRequest() : Results.Ok(updated);
})
    .RequireAuthorization();

app.MapPut("/purchase/{id}/receive", async (PurchaseService purchases, PermissionService permissions, ClaimsPrincipal user, int id) =>
{
    if (!await permissions.HasPermissionAsync(user, "Purchase", "Edit"))
        return Results.Forbid();

    var updated = await purchases.ReceiveAsync(id);
    return updated is null ? Results.BadRequest() : Results.Ok(updated);
})
    .RequireAuthorization();

// Sales endpoints
app.MapPost("/sales", async (SalesService sales, PermissionService permissions, ClaimsPrincipal user, SalesOrderCreateDto dto) =>
{
    if (!await permissions.HasPermissionAsync(user, "Sales", "Create"))
        return Results.Forbid();

    var created = await sales.CreateAsync(dto);
    return created is null ? Results.BadRequest() : Results.Created($"/sales/{created.Id}", created);
})
    .RequireAuthorization();

app.MapGet("/sales", async (SalesService sales, PermissionService permissions, ClaimsPrincipal user) =>
{
    if (!await permissions.HasPermissionAsync(user, "Sales", "View"))
        return Results.Forbid();

    return Results.Ok(await sales.GetAllAsync());
})
    .RequireAuthorization();

app.MapGet("/sales/{id}", async (SalesService sales, PermissionService permissions, ClaimsPrincipal user, int id) =>
{
    if (!await permissions.HasPermissionAsync(user, "Sales", "View"))
        return Results.Forbid();

    var order = await sales.GetByIdAsync(id);
    return order is null ? Results.NotFound() : Results.Ok(order);
})
    .RequireAuthorization();

app.MapPut("/sales/{id}", async (SalesService sales, PermissionService permissions, ClaimsPrincipal user, int id, SalesOrderUpdateDto dto) =>
{
    if (!await permissions.HasPermissionAsync(user, "Sales", "Edit"))
        return Results.Forbid();

    var updated = await sales.UpdateAsync(id, dto);
    return updated is null ? Results.BadRequest() : Results.Ok(updated);
})
    .RequireAuthorization();

app.MapPut("/sales/{id}/confirm", async (SalesService sales, PermissionService permissions, ClaimsPrincipal user, int id) =>
{
    if (!await permissions.HasPermissionAsync(user, "Sales", "Edit"))
        return Results.Forbid();

    var updated = await sales.ConfirmAsync(id);
    return updated is null ? Results.BadRequest() : Results.Ok(updated);
})
    .RequireAuthorization();

app.MapPut("/sales/{id}/deliver", async (SalesService sales, PermissionService permissions, ClaimsPrincipal user, int id) =>
{
    if (!await permissions.HasPermissionAsync(user, "Sales", "Edit"))
        return Results.Forbid();

    var updated = await sales.DeliverAsync(id);
    return updated is null ? Results.BadRequest() : Results.Ok(updated);
})
    .RequireAuthorization();

// BOM endpoints
app.MapPost("/bom", async (BomService boms, PermissionService permissions, ClaimsPrincipal user, BomCreateDto dto) =>
{
    if (!await permissions.HasPermissionAsync(user, "Production", "Create"))
        return Results.Forbid();

    var created = await boms.CreateAsync(dto);
    return created is null ? Results.BadRequest() : Results.Created($"/bom/{created.Id}", created);
})
    .RequireAuthorization();

app.MapGet("/bom", async (BomService boms, PermissionService permissions, ClaimsPrincipal user) =>
{
    if (!await permissions.HasPermissionAsync(user, "Production", "View"))
        return Results.Forbid();

    return Results.Ok(await boms.GetAllAsync());
})
    .RequireAuthorization();

app.MapGet("/bom/{id}", async (BomService boms, PermissionService permissions, ClaimsPrincipal user, int id) =>
{
    if (!await permissions.HasPermissionAsync(user, "Production", "View"))
        return Results.Forbid();

    var bom = await boms.GetByIdAsync(id);
    return bom is null ? Results.NotFound() : Results.Ok(bom);
})
    .RequireAuthorization();

app.MapPost("/bom/{id}/lines", async (BomService boms, PermissionService permissions, ClaimsPrincipal user, int id, BomLineCreateDto dto) =>
{
    if (!await permissions.HasPermissionAsync(user, "Production", "Edit"))
        return Results.Forbid();

    var created = await boms.AddLineAsync(id, dto);
    return created is null ? Results.BadRequest() : Results.Created($"/bom/{id}/lines/{created.Id}", created);
})
    .RequireAuthorization();

app.MapDelete("/bom/{id}/lines/{lineId}", async (BomService boms, PermissionService permissions, ClaimsPrincipal user, int id, int lineId) =>
{
    if (!await permissions.HasPermissionAsync(user, "Production", "Delete"))
        return Results.Forbid();

    var deleted = await boms.RemoveLineAsync(id, lineId);
    return deleted ? Results.NoContent() : Results.NotFound();
})
    .RequireAuthorization();

// Production endpoints
app.MapPost("/production", async (BomService boms, PermissionService permissions, ClaimsPrincipal user, ProductionOrderCreateDto dto) =>
{
    if (!await permissions.HasPermissionAsync(user, "Production", "Create"))
        return Results.Forbid();

    var created = await boms.CreateProductionAsync(dto);
    return created is null ? Results.BadRequest() : Results.Created($"/production/{created.Id}", created);
})
    .RequireAuthorization();

app.MapGet("/production", async (BomService boms, PermissionService permissions, ClaimsPrincipal user) =>
{
    if (!await permissions.HasPermissionAsync(user, "Production", "View"))
        return Results.Forbid();

    return Results.Ok(await boms.GetAllProductionAsync());
})
    .RequireAuthorization();

app.MapGet("/production/{id}", async (BomService boms, PermissionService permissions, ClaimsPrincipal user, int id) =>
{
    if (!await permissions.HasPermissionAsync(user, "Production", "View"))
        return Results.Forbid();

    var order = await boms.GetProductionByIdAsync(id);
    return order is null ? Results.NotFound() : Results.Ok(order);
})
    .RequireAuthorization();

app.MapPut("/production/{id}/start", async (BomService boms, PermissionService permissions, ClaimsPrincipal user, int id) =>
{
    if (!await permissions.HasPermissionAsync(user, "Production", "Edit"))
        return Results.Forbid();

    var updated = await boms.StartProductionAsync(id);
    return updated is null ? Results.BadRequest() : Results.Ok(updated);
})
    .RequireAuthorization();

app.MapPut("/production/{id}/complete", async (BomService boms, PermissionService permissions, ClaimsPrincipal user, int id) =>
{
    if (!await permissions.HasPermissionAsync(user, "Production", "Edit"))
        return Results.Forbid();

    var updated = await boms.CompleteProductionAsync(id);
    return updated is null ? Results.BadRequest() : Results.Ok(updated);
})
    .RequireAuthorization();

// Sales receipt endpoints
app.MapPost("/sales/{id}/receipt", async (SalesService sales, ReceiptService receipts, PermissionService permissions, ClaimsPrincipal user, IWebHostEnvironment env, int id) =>
{
    if (!await permissions.HasPermissionAsync(user, "Sales", "Create"))
        return Results.Forbid();

    var order = await sales.GetModelByIdAsync(id);
    if (order == null) return Results.NotFound();

    var receipt = receipts.BuildReceipt(order, "MyFirstApi");
    var pdf = receipts.GenerateReceiptPdf(receipt);

    var folder = Path.Combine(env.WebRootPath ?? "wwwroot", "receipts");
    Directory.CreateDirectory(folder);

    var filePath = Path.Combine(folder, $"{id}.pdf");
    await File.WriteAllBytesAsync(filePath, pdf);

    return Results.File(pdf, "application/pdf", $"receipt-{id}.pdf");
})
    .RequireAuthorization();

// Reports endpoints
app.MapGet("/reports/purchase", async (ReportService reports, PermissionService permissions, ClaimsPrincipal user, DateTime? from, DateTime? to, int? vendorId, int? itemId, string? status) =>
{
    if (!await permissions.HasPermissionAsync(user, "Reports", "View"))
        return Results.Forbid();

    var rows = await reports.GetPurchaseReportAsync(from, to, vendorId, itemId, status);
    return Results.Ok(rows);
})
    .RequireAuthorization();

app.MapPost("/reports/purchase/pdf", async (ReportService reports, PermissionService permissions, ClaimsPrincipal user, DateTime? from, DateTime? to, int? vendorId, int? itemId, string? status) =>
{
    if (!await permissions.HasPermissionAsync(user, "Reports", "View"))
        return Results.Forbid();

    var rows = await reports.GetPurchaseReportAsync(from, to, vendorId, itemId, status);
    var pdf = reports.BuildPurchaseReportPdf(rows);
    return Results.File(pdf, "application/pdf", "purchase-report.pdf");
})
    .RequireAuthorization();

app.MapPost("/reports/purchase/csv", async (ReportService reports, PermissionService permissions, ClaimsPrincipal user, DateTime? from, DateTime? to, int? vendorId, int? itemId, string? status) =>
{
    if (!await permissions.HasPermissionAsync(user, "Reports", "View"))
        return Results.Forbid();

    var rows = await reports.GetPurchaseReportAsync(from, to, vendorId, itemId, status);
    var csv = reports.BuildPurchaseReportCsv(rows);
    return Results.File(csv, "text/csv", "purchase-report.csv");
})
    .RequireAuthorization();

app.MapGet("/reports/sales", async (ReportService reports, PermissionService permissions, ClaimsPrincipal user, DateTime? from, DateTime? to, int? customerId, int? itemId, string? status) =>
{
    if (!await permissions.HasPermissionAsync(user, "Reports", "View"))
        return Results.Forbid();

    var rows = await reports.GetSalesReportAsync(from, to, customerId, itemId, status);
    return Results.Ok(rows);
})
    .RequireAuthorization();

app.MapPost("/reports/sales/pdf", async (ReportService reports, PermissionService permissions, ClaimsPrincipal user, DateTime? from, DateTime? to, int? customerId, int? itemId, string? status) =>
{
    if (!await permissions.HasPermissionAsync(user, "Reports", "View"))
        return Results.Forbid();

    var rows = await reports.GetSalesReportAsync(from, to, customerId, itemId, status);
    var pdf = reports.BuildSalesReportPdf(rows);
    return Results.File(pdf, "application/pdf", "sales-report.pdf");
})
    .RequireAuthorization();

app.MapPost("/reports/sales/csv", async (ReportService reports, PermissionService permissions, ClaimsPrincipal user, DateTime? from, DateTime? to, int? customerId, int? itemId, string? status) =>
{
    if (!await permissions.HasPermissionAsync(user, "Reports", "View"))
        return Results.Forbid();

    var rows = await reports.GetSalesReportAsync(from, to, customerId, itemId, status);
    var csv = reports.BuildSalesReportCsv(rows);
    return Results.File(csv, "text/csv", "sales-report.csv");
})
    .RequireAuthorization();

app.MapGet("/reports/inventory", async (ReportService reports, PermissionService permissions, ClaimsPrincipal user) =>
{
    if (!await permissions.HasPermissionAsync(user, "Reports", "View"))
        return Results.Forbid();

    var rows = await reports.GetInventoryReportAsync();
    return Results.Ok(rows);
})
    .RequireAuthorization();

app.MapPost("/reports/inventory/pdf", async (ReportService reports, PermissionService permissions, ClaimsPrincipal user) =>
{
    if (!await permissions.HasPermissionAsync(user, "Reports", "View"))
        return Results.Forbid();

    var rows = await reports.GetInventoryReportAsync();
    var pdf = reports.BuildInventoryReportPdf(rows);
    return Results.File(pdf, "application/pdf", "inventory-report.pdf");
})
    .RequireAuthorization();

app.MapPost("/reports/inventory/csv", async (ReportService reports, PermissionService permissions, ClaimsPrincipal user) =>
{
    if (!await permissions.HasPermissionAsync(user, "Reports", "View"))
        return Results.Forbid();

    var rows = await reports.GetInventoryReportAsync();
    var csv = reports.BuildInventoryReportCsv(rows);
    return Results.File(csv, "text/csv", "inventory-report.csv");
})
    .RequireAuthorization();


app.MapPost("/items/import", async (
    IFormFile file,
    IBackgroundJobClient jobClient,
    ItemImportService importService) =>
{
    if (file is null || file.Length == 0)
        return Results.BadRequest("No file provided");

    var ext = Path.GetExtension(file.FileName).ToLower();
    if (ext != ".csv" && ext != ".xlsx")
        return Results.BadRequest("Only .csv and .xlsx files are supported");

    // save file to temp location
    var tempPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}{ext}");
    await using (var stream = File.Create(tempPath))
    {
        await file.CopyToAsync(stream);
    }

    // queue background job
    var jobId = jobClient.Enqueue<ItemImportService>(
        x => x.ProcessFileAsync(tempPath, ext.TrimStart('.')));

    return Results.Accepted("/jobs", new { jobId, message = "Import queued" });
}).DisableAntiforgery();

app.MapPost("/auth/register", async (AuthService auth, RegisterDto dto) =>
{
    var result = await auth.RegisterAsync(dto);
    return result is null
        ? Results.Conflict("Email already registered")
        : Results.Ok(result);
});

app.MapPost("/auth/login", async (AuthService auth, LoginDto dto) =>
{
    var result = await auth.LoginAsync(dto);
    return result is null
        ? Results.Unauthorized()
        : Results.Ok(result);
});

app.Run();
