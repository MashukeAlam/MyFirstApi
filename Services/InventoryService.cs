using Microsoft.EntityFrameworkCore;
using MyFirstApi.Data;
using MyFirstApi.DTOs;
using MyFirstApi.Models;

namespace MyFirstApi.Services;

public class InventoryService
{
    private readonly AppDbContext _db;

    public InventoryService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<InventoryStockResponseDto?> GetStockLevelAsync(int itemId)
    {
        var itemExists = await _db.Items.AnyAsync(i => i.Id == itemId && i.IsActive);
        if (!itemExists) return null;

        var stock = await _db.InventoryMovements
            .Where(m => m.ItemId == itemId)
            .Select(m => new { m.MovementType, m.Quantity })
            .ToListAsync();

        var level = stock.Sum(m => m.MovementType == "IN" ? m.Quantity : -m.Quantity);
        return new InventoryStockResponseDto(itemId, level);
    }

    public async Task<List<InventoryMovementResponseDto>?> GetLedgerAsync(int itemId)
    {
        var itemExists = await _db.Items.AnyAsync(i => i.Id == itemId && i.IsActive);
        if (!itemExists) return null;

        return await _db.InventoryMovements
            .Where(m => m.ItemId == itemId)
            .OrderBy(m => m.Date)
            .Select(m => ToResponseDto(m))
            .ToListAsync();
    }

    public async Task AddMovementAsync(
        int itemId,
        string movementType,
        decimal quantity,
        string referenceType,
        int referenceId,
        string notes)
    {
        var itemExists = await _db.Items.AnyAsync(i => i.Id == itemId && i.IsActive);
        if (!itemExists) return;

        var movement = new InventoryMovement
        {
            ItemId = itemId,
            MovementType = movementType,
            Quantity = quantity,
            ReferenceType = referenceType,
            ReferenceId = referenceId,
            Notes = notes,
            Date = DateTime.UtcNow
        };

        _db.InventoryMovements.Add(movement);
        await _db.SaveChangesAsync();
    }

    public async Task<decimal> GetStockLevelValueAsync(int itemId)
    {
        var movements = await _db.InventoryMovements
            .Where(m => m.ItemId == itemId)
            .Select(m => new { m.MovementType, m.Quantity })
            .ToListAsync();

        return movements.Sum(m => m.MovementType == "IN" ? m.Quantity : -m.Quantity);
    }

    private static InventoryMovementResponseDto ToResponseDto(InventoryMovement movement) => new(
        movement.Id,
        movement.ItemId,
        movement.MovementType,
        movement.Quantity,
        movement.ReferenceType,
        movement.ReferenceId,
        movement.Date,
        movement.Notes
    );
}
