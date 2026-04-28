using Microsoft.EntityFrameworkCore;
using MyFirstApi.Data;
using MyFirstApi.DTOs;
using MyFirstApi.Models;

namespace MyFirstApi.Services;

public class PurchaseService
{
    private readonly AppDbContext _db;
    private readonly InventoryService _inventory;

    public PurchaseService(AppDbContext db, InventoryService inventory)
    {
        _db = db;
        _inventory = inventory;
    }

    public async Task<List<PurchaseOrderResponseDto>> GetAllAsync()
    {
        return await _db.PurchaseOrders
            .Include(p => p.Lines)
            .Select(p => ToResponseDto(p))
            .ToListAsync();
    }

    public async Task<PurchaseOrderResponseDto?> GetByIdAsync(int id)
    {
        var order = await _db.PurchaseOrders
            .Include(p => p.Lines)
            .FirstOrDefaultAsync(p => p.Id == id);

        return order == null ? null : ToResponseDto(order);
    }

    public async Task<PurchaseOrderResponseDto?> CreateAsync(PurchaseOrderCreateDto dto)
    {
        var vendor = await _db.Vendors.FirstOrDefaultAsync(v => v.Id == dto.VendorId && v.IsActive);
        if (vendor == null) return null;

        var order = new PurchaseOrder
        {
            VendorId = dto.VendorId,
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

        _db.PurchaseOrders.Add(order);
        await _db.SaveChangesAsync();

        return await GetByIdAsync(order.Id);
    }

    public async Task<PurchaseOrderResponseDto?> UpdateAsync(int id, PurchaseOrderUpdateDto dto)
    {
        var order = await _db.PurchaseOrders
            .Include(p => p.Lines)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (order == null) return null;
        if (order.Status != "Draft") return null;

        var vendor = await _db.Vendors.FirstOrDefaultAsync(v => v.Id == dto.VendorId && v.IsActive);
        if (vendor == null) return null;

        order.VendorId = dto.VendorId;
        order.Date = dto.Date;
        order.Notes = dto.Notes;
        order.CompanyId = dto.CompanyId;

        _db.PurchaseOrderLines.RemoveRange(order.Lines);

        var lines = await BuildLinesAsync(dto.Lines);
        if (lines == null) return null;

        order.Lines = lines;

        await _db.SaveChangesAsync();
        return await GetByIdAsync(order.Id);
    }

    public async Task<PurchaseOrderResponseDto?> ConfirmAsync(int id)
    {
        var order = await _db.PurchaseOrders
            .Include(p => p.Lines)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (order == null) return null;
        if (order.Status != "Draft") return null;

        order.Status = "Confirmed";
        await _db.SaveChangesAsync();

        return await GetByIdAsync(order.Id);
    }

    public async Task<PurchaseOrderResponseDto?> ReceiveAsync(int id)
    {
        var order = await _db.PurchaseOrders
            .Include(p => p.Lines)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (order == null) return null;
        if (order.Status != "Confirmed") return null;

        order.Status = "Received";
        await _db.SaveChangesAsync();

        foreach (var line in order.Lines)
        {
            await _inventory.AddMovementAsync(
                line.ItemId,
                "IN",
                line.Quantity,
                "Purchase",
                order.Id,
                $"PO {order.Id} received");
        }

        return await GetByIdAsync(order.Id);
    }

    private async Task<List<PurchaseOrderLine>?> BuildLinesAsync(List<PurchaseOrderLineCreateDto> lines)
    {
        var itemIds = lines.Select(l => l.ItemId).ToList();
        var items = await _db.Items.Where(i => itemIds.Contains(i.Id) && i.IsActive).ToListAsync();

        if (items.Count != itemIds.Count) return null;

        var mapped = new List<PurchaseOrderLine>();
        foreach (var line in lines)
        {
            var item = items.First(i => i.Id == line.ItemId);
            var vatAmount = line.Quantity * line.UnitPrice * item.VatRate / 100m;
            var sdAmount = line.Quantity * line.UnitPrice * item.SdRate / 100m;

            mapped.Add(new PurchaseOrderLine
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

    private static PurchaseOrderResponseDto ToResponseDto(PurchaseOrder order) => new(
        order.Id,
        order.VendorId,
        order.Date,
        order.Status,
        order.Notes,
        order.CreatedByUserId,
        order.CompanyId,
        order.Lines.Select(l => new PurchaseOrderLineResponseDto(
            l.Id,
            l.ItemId,
            l.Quantity,
            l.UnitPrice,
            l.VatAmount,
            l.SdAmount)).ToList()
    );
}
