using CsvHelper;
using Microsoft.EntityFrameworkCore;
using MyFirstApi.Data;
using MyFirstApi.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;

namespace MyFirstApi.Services;

public class ReportService
{
    private readonly AppDbContext _db;

    public ReportService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<PurchaseReportItemDto>> GetPurchaseReportAsync(
        DateTime? from,
        DateTime? to,
        int? vendorId,
        int? itemId,
        string? status)
    {
        var query = _db.PurchaseOrders
            .Include(p => p.Vendor)
            .Include(p => p.Lines)
            .AsQueryable();

        if (from.HasValue) query = query.Where(p => p.Date >= from.Value);
        if (to.HasValue) query = query.Where(p => p.Date <= to.Value);
        if (vendorId.HasValue) query = query.Where(p => p.VendorId == vendorId.Value);
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(p => p.Status == status);

        var orders = await query.ToListAsync();

        var rows = new List<PurchaseReportItemDto>();
        foreach (var order in orders)
        {
            foreach (var line in order.Lines)
            {
                if (itemId.HasValue && line.ItemId != itemId.Value) continue;
                rows.Add(new PurchaseReportItemDto(
                    PurchaseOrderId: order.Id,
                    Date: order.Date,
                    Status: order.Status,
                    VendorId: order.VendorId,
                    VendorName: order.Vendor.Name,
                    ItemId: line.ItemId,
                    Quantity: line.Quantity,
                    UnitPrice: line.UnitPrice,
                    VatAmount: line.VatAmount,
                    SdAmount: line.SdAmount));
            }
        }

        return rows;
    }

    public async Task<List<SalesReportItemDto>> GetSalesReportAsync(
        DateTime? from,
        DateTime? to,
        int? customerId,
        int? itemId,
        string? status)
    {
        var query = _db.SalesOrders
            .Include(s => s.Customer)
            .Include(s => s.Lines)
            .AsQueryable();

        if (from.HasValue) query = query.Where(s => s.Date >= from.Value);
        if (to.HasValue) query = query.Where(s => s.Date <= to.Value);
        if (customerId.HasValue) query = query.Where(s => s.CustomerId == customerId.Value);
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(s => s.Status == status);

        var orders = await query.ToListAsync();

        var rows = new List<SalesReportItemDto>();
        foreach (var order in orders)
        {
            foreach (var line in order.Lines)
            {
                if (itemId.HasValue && line.ItemId != itemId.Value) continue;
                rows.Add(new SalesReportItemDto(
                    SalesOrderId: order.Id,
                    Date: order.Date,
                    Status: order.Status,
                    CustomerId: order.CustomerId,
                    CustomerName: order.Customer.Name,
                    ItemId: line.ItemId,
                    Quantity: line.Quantity,
                    UnitPrice: line.UnitPrice,
                    VatAmount: line.VatAmount,
                    SdAmount: line.SdAmount));
            }
        }

        return rows;
    }

    public async Task<List<InventoryReportItemDto>> GetInventoryReportAsync()
    {
        var items = await _db.Items.Where(i => i.IsActive).ToListAsync();
        var movements = await _db.InventoryMovements.ToListAsync();

        var report = new List<InventoryReportItemDto>();
        foreach (var item in items)
        {
            var level = movements
                .Where(m => m.ItemId == item.Id)
                .Sum(m => m.MovementType == "IN" ? m.Quantity : -m.Quantity);

            report.Add(new InventoryReportItemDto(item.Id, item.Name, level));
        }

        return report;
    }

    public byte[] BuildPurchaseReportPdf(List<PurchaseReportItemDto> rows)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Size(PageSizes.A4);

                page.Header().Text("Purchase Report").FontSize(18).SemiBold();

                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(CellStyle).Text("PO");
                        header.Cell().Element(CellStyle).Text("Date");
                        header.Cell().Element(CellStyle).Text("Vendor");
                        header.Cell().Element(CellStyle).Text("Item");
                        header.Cell().Element(CellStyle).Text("Qty");
                        header.Cell().Element(CellStyle).Text("Amount");
                    });

                    foreach (var row in rows)
                    {
                        var amount = row.Quantity * row.UnitPrice + row.VatAmount + row.SdAmount;
                        table.Cell().Element(CellStyle).Text(row.PurchaseOrderId.ToString());
                        table.Cell().Element(CellStyle).Text(row.Date.ToString("yyyy-MM-dd"));
                        table.Cell().Element(CellStyle).Text(row.VendorName);
                        table.Cell().Element(CellStyle).Text(row.ItemId.ToString());
                        table.Cell().Element(CellStyle).Text(row.Quantity.ToString("0.##"));
                        table.Cell().Element(CellStyle).Text(amount.ToString("0.00"));
                    }
                });
            });
        });

        return document.GeneratePdf();
    }

    public byte[] BuildSalesReportPdf(List<SalesReportItemDto> rows)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Size(PageSizes.A4);

                page.Header().Text("Sales Report").FontSize(18).SemiBold();

                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(CellStyle).Text("SO");
                        header.Cell().Element(CellStyle).Text("Date");
                        header.Cell().Element(CellStyle).Text("Customer");
                        header.Cell().Element(CellStyle).Text("Item");
                        header.Cell().Element(CellStyle).Text("Qty");
                        header.Cell().Element(CellStyle).Text("Amount");
                    });

                    foreach (var row in rows)
                    {
                        var amount = row.Quantity * row.UnitPrice + row.VatAmount + row.SdAmount;
                        table.Cell().Element(CellStyle).Text(row.SalesOrderId.ToString());
                        table.Cell().Element(CellStyle).Text(row.Date.ToString("yyyy-MM-dd"));
                        table.Cell().Element(CellStyle).Text(row.CustomerName);
                        table.Cell().Element(CellStyle).Text(row.ItemId.ToString());
                        table.Cell().Element(CellStyle).Text(row.Quantity.ToString("0.##"));
                        table.Cell().Element(CellStyle).Text(amount.ToString("0.00"));
                    }
                });
            });
        });

        return document.GeneratePdf();
    }

    public byte[] BuildInventoryReportPdf(List<InventoryReportItemDto> rows)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Size(PageSizes.A4);

                page.Header().Text("Inventory Report").FontSize(18).SemiBold();

                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(4);
                        columns.RelativeColumn(2);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(CellStyle).Text("Item Id");
                        header.Cell().Element(CellStyle).Text("Item Name");
                        header.Cell().Element(CellStyle).Text("Stock");
                    });

                    foreach (var row in rows)
                    {
                        table.Cell().Element(CellStyle).Text(row.ItemId.ToString());
                        table.Cell().Element(CellStyle).Text(row.ItemName);
                        table.Cell().Element(CellStyle).Text(row.StockLevel.ToString("0.##"));
                    }
                });
            });
        });

        return document.GeneratePdf();
    }

    public byte[] BuildPurchaseReportCsv(List<PurchaseReportItemDto> rows)
    {
        using var writer = new StringWriter();
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        csv.WriteRecords(rows);
        return System.Text.Encoding.UTF8.GetBytes(writer.ToString());
    }

    public byte[] BuildSalesReportCsv(List<SalesReportItemDto> rows)
    {
        using var writer = new StringWriter();
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        csv.WriteRecords(rows);
        return System.Text.Encoding.UTF8.GetBytes(writer.ToString());
    }

    public byte[] BuildInventoryReportCsv(List<InventoryReportItemDto> rows)
    {
        using var writer = new StringWriter();
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        csv.WriteRecords(rows);
        return System.Text.Encoding.UTF8.GetBytes(writer.ToString());
    }

    private static IContainer CellStyle(IContainer container)
    {
        return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
    }
}
