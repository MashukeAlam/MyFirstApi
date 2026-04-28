using Microsoft.EntityFrameworkCore;
using MyFirstApi.Data;
using MyFirstApi.DTOs;
using MyFirstApi.Models;

namespace MyFirstApi.Services;

public class SalesService
{
    private readonly AppDbContext _db;
    private readonly InventoryService _inventory;

    public SalesService(AppDbContext db, InventoryService inventory)
    {
        _db = db;
        _inventory = inventory;
    }

    public async Task<List<SalesOrderResponseDto>> GetAllAsync()
    {
        return await _db.SalesOrders
            .Include(s => s.Lines)
            .Select(s => ToResponseDto(s))
            .ToListAsync();
    }

    public async Task<SalesOrderResponseDto?> GetByIdAsync(int id)
    {
        var order = await _db.SalesOrders
            .Include(s => s.Customer)
            .Include(s => s.Lines)
                .ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(s => s.Id == id);

        return order == null ? null : ToResponseDto(order);
    }

    public async Task<SalesOrder?> GetModelByIdAsync(int id)
    {
        return await _db.SalesOrders
            .Include(s => s.Customer)
            .Include(s => s.Lines)
                .ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<SalesOrderResponseDto?> CreateAsync(SalesOrderCreateDto dto)
    {
        var customer = await _db.Customers.FirstOrDefaultAsync(c => c.Id == dto.CustomerId && c.IsActive);
        if (customer == null) return null;

        var order = new SalesOrder
        {
            CustomerId = dto.CustomerId,
            Date = dto.Date,
            Status = "Draft",
            Notes = dto.Notes,
            CreatedByUserId = dto.CreatedByUserId,
            CompanyId = dto.CompanyId
        };

        var lines = await BuildLinesAsync(dto.Lines);
        if (lines == null) return null;

        foreach (var line in lines)
        {
            order.Lines.Add(line);
        }

        _db.SalesOrders.Add(order);
        await _db.SaveChangesAsync();

        return await GetByIdAsync(order.Id);
    }

    public async Task<SalesOrderResponseDto?> UpdateAsync(int id, SalesOrderUpdateDto dto)
    {
        var order = await _db.SalesOrders
            .Include(s => s.Lines)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (order == null) return null;
        if (order.Status != "Draft") return null;

        var customer = await _db.Customers.FirstOrDefaultAsync(c => c.Id == dto.CustomerId && c.IsActive);
        if (customer == null) return null;

        order.CustomerId = dto.CustomerId;
        order.Date = dto.Date;
        order.Notes = dto.Notes;
        order.CompanyId = dto.CompanyId;

        _db.SalesOrderLines.RemoveRange(order.Lines);

        var lines = await BuildLinesAsync(dto.Lines);
        if (lines == null) return null;

        order.Lines = lines;

        await _db.SaveChangesAsync();
        return await GetByIdAsync(order.Id);
    }

    public async Task<SalesOrderResponseDto?> ConfirmAsync(int id)
    {
        var order = await _db.SalesOrders
            .Include(s => s.Lines)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (order == null) return null;
        if (order.Status != "Draft") return null;

        var stockOk = await HasSufficientStockAsync(order);
        if (!stockOk) return null;

        order.Status = "Confirmed";
        await _db.SaveChangesAsync();

        return await GetByIdAsync(order.Id);
    }

    public async Task<SalesOrderResponseDto?> DeliverAsync(int id)
    {
        var order = await _db.SalesOrders
            .Include(s => s.Lines)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (order == null) return null;
        if (order.Status != "Confirmed") return null;

        var stockOk = await HasSufficientStockAsync(order);
        if (!stockOk) return null;

        order.Status = "Delivered";
        await _db.SaveChangesAsync();

        foreach (var line in order.Lines)
        {
            await _inventory.AddMovementAsync(
                line.ItemId,
                "OUT",
                line.Quantity,
                "Sale",
                order.Id,
                $"SO {order.Id} delivered");
        }

        return await GetByIdAsync(order.Id);
    }

    private async Task<bool> HasSufficientStockAsync(SalesOrder order)
    {
        foreach (var line in order.Lines)
        {
            var stock = await _inventory.GetStockLevelValueAsync(line.ItemId);
            if (stock < line.Quantity)
            {
                return false;
            }
        }

        return true;
    }

    private async Task<List<SalesOrderLine>?> BuildLinesAsync(List<SalesOrderLineCreateDto> lines)
    {
        var itemIds = lines.Select(l => l.ItemId).ToList();
        var items = await _db.Items.Where(i => itemIds.Contains(i.Id) && i.IsActive).ToListAsync();

        if (items.Count != itemIds.Count) return null;

        var mapped = new List<SalesOrderLine>();
        foreach (var line in lines)
        {
            var item = items.First(i => i.Id == line.ItemId);
            var vatAmount = line.Quantity * line.UnitPrice * item.VatRate / 100m;
            var sdAmount = line.Quantity * line.UnitPrice * item.SdRate / 100m;

            mapped.Add(new SalesOrderLine
            {
                ItemId = line.ItemId,
                Quantity = line.Quantity,
                UnitPrice = line.UnitPrice,
                VatAmount = vatAmount,
                SdAmount = sdAmount
            });
        }

        return mapped;
    }

    private static SalesOrderResponseDto ToResponseDto(SalesOrder order) => new(
        order.Id,
        order.CustomerId,
        order.Date,
        order.Status,
        order.Notes,
        order.CreatedByUserId,
        order.CompanyId,
        order.Lines.Select(l => new SalesOrderLineResponseDto(
            l.Id,
            l.ItemId,
            l.Quantity,
            l.UnitPrice,
            l.VatAmount,
            l.SdAmount)).ToList()
    );
}
