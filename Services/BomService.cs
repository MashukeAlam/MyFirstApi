using Microsoft.EntityFrameworkCore;
using MyFirstApi.Data;
using MyFirstApi.DTOs;
using MyFirstApi.Models;

namespace MyFirstApi.Services;

public class BomService
{
    private readonly AppDbContext _db;
    private readonly InventoryService _inventory;

    public BomService(AppDbContext db, InventoryService inventory)
    {
        _db = db;
        _inventory = inventory;
    }

    public async Task<List<BomResponseDto>> GetAllAsync()
    {
        return await _db.BomHeaders
            .Include(b => b.Lines)
            .Select(b => ToResponseDto(b))
            .ToListAsync();
    }

    public async Task<BomResponseDto?> GetByIdAsync(int id)
    {
        var bom = await _db.BomHeaders
            .Include(b => b.Lines)
            .FirstOrDefaultAsync(b => b.Id == id);

        return bom == null ? null : ToResponseDto(bom);
    }

    public async Task<BomResponseDto?> CreateAsync(BomCreateDto dto)
    {
        var itemExists = await _db.Items.AnyAsync(i => i.Id == dto.FinishedItemId && i.IsActive);
        if (!itemExists) return null;

        var bom = new BomHeader
        {
            FinishedItemId = dto.FinishedItemId,
            Version = dto.Version,
            IsActive = dto.IsActive,
            Notes = dto.Notes
        };

        _db.BomHeaders.Add(bom);
        await _db.SaveChangesAsync();

        return await GetByIdAsync(bom.Id);
    }

    public async Task<BomLineResponseDto?> AddLineAsync(int bomId, BomLineCreateDto dto)
    {
        var bom = await _db.BomHeaders.FirstOrDefaultAsync(b => b.Id == bomId && b.IsActive);
        if (bom == null) return null;

        var itemExists = await _db.Items.AnyAsync(i => i.Id == dto.RawMaterialItemId && i.IsActive);
        if (!itemExists) return null;

        var line = new BomLine
        {
            BomHeaderId = bomId,
            RawMaterialItemId = dto.RawMaterialItemId,
            QuantityRequired = dto.QuantityRequired
        };

        _db.BomLines.Add(line);
        await _db.SaveChangesAsync();

        return new BomLineResponseDto(line.Id, line.RawMaterialItemId, line.QuantityRequired);
    }

    public async Task<bool> RemoveLineAsync(int bomId, int lineId)
    {
        var line = await _db.BomLines
            .FirstOrDefaultAsync(l => l.Id == lineId && l.BomHeaderId == bomId);

        if (line == null) return false;

        _db.BomLines.Remove(line);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<ProductionOrderResponseDto?> CreateProductionAsync(ProductionOrderCreateDto dto)
    {
        var bom = await _db.BomHeaders
            .Include(b => b.Lines)
            .FirstOrDefaultAsync(b => b.Id == dto.BomHeaderId && b.IsActive);

        if (bom == null) return null;

        var order = new ProductionOrder
        {
            BomHeaderId = dto.BomHeaderId,
            QuantityToProduce = dto.QuantityToProduce,
            Status = "Draft",
            Notes = dto.Notes,
            CreatedByUserId = dto.CreatedByUserId
        };

        _db.ProductionOrders.Add(order);
        await _db.SaveChangesAsync();

        return ToResponseDto(order);
    }

    public async Task<ProductionOrderResponseDto?> GetProductionByIdAsync(int id)
    {
        var order = await _db.ProductionOrders.FirstOrDefaultAsync(p => p.Id == id);
        return order == null ? null : ToResponseDto(order);
    }

    public async Task<List<ProductionOrderResponseDto>> GetAllProductionAsync()
    {
        return await _db.ProductionOrders
            .Select(p => ToResponseDto(p))
            .ToListAsync();
    }

    public async Task<ProductionOrderResponseDto?> StartProductionAsync(int id)
    {
        var order = await _db.ProductionOrders
            .Include(p => p.BomHeader)
                .ThenInclude(b => b.Lines)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (order == null) return null;
        if (order.Status != "Draft") return null;

        var stockOk = await HasSufficientRawMaterialsAsync(order);
        if (!stockOk) return null;

        order.Status = "InProgress";
        await _db.SaveChangesAsync();

        return ToResponseDto(order);
    }

    public async Task<ProductionOrderResponseDto?> CompleteProductionAsync(int id)
    {
        var order = await _db.ProductionOrders
            .Include(p => p.BomHeader)
                .ThenInclude(b => b.Lines)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (order == null) return null;
        if (order.Status != "InProgress") return null;

        var stockOk = await HasSufficientRawMaterialsAsync(order);
        if (!stockOk) return null;

        order.Status = "Completed";
        await _db.SaveChangesAsync();

        foreach (var line in order.BomHeader.Lines)
        {
            var qty = line.QuantityRequired * order.QuantityToProduce;
            await _inventory.AddMovementAsync(
                line.RawMaterialItemId,
                "OUT",
                qty,
                "Production",
                order.Id,
                $"Production {order.Id} consumption");
        }

        await _inventory.AddMovementAsync(
            order.BomHeader.FinishedItemId,
            "IN",
            order.QuantityToProduce,
            "Production",
            order.Id,
            $"Production {order.Id} completed");

        return ToResponseDto(order);
    }

    private async Task<bool> HasSufficientRawMaterialsAsync(ProductionOrder order)
    {
        foreach (var line in order.BomHeader.Lines)
        {
            var required = line.QuantityRequired * order.QuantityToProduce;
            var stock = await _inventory.GetStockLevelValueAsync(line.RawMaterialItemId);
            if (stock < required)
            {
                return false;
            }
        }

        return true;
    }

    private static BomResponseDto ToResponseDto(BomHeader bom) => new(
        bom.Id,
        bom.FinishedItemId,
        bom.Version,
        bom.IsActive,
        bom.Notes,
        bom.Lines.Select(l => new BomLineResponseDto(
            l.Id,
            l.RawMaterialItemId,
            l.QuantityRequired)).ToList()
    );

    private static ProductionOrderResponseDto ToResponseDto(ProductionOrder order) => new(
        order.Id,
        order.BomHeaderId,
        order.QuantityToProduce,
        order.Status,
        order.Date,
        order.Notes,
        order.CreatedByUserId
    );
}
