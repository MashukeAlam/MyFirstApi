using Microsoft.EntityFrameworkCore;
using MyFirstApi.Data;
using MyFirstApi.DTOs;
using MyFirstApi.Models;

namespace MyFirstApi.Services;

public class ItemService
{
    private readonly AppDbContext _db;

    public ItemService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<ItemResponseDto>> GetAllAsync()
    {
        return await _db.Items
            .Where(i => i.IsActive)
            .Select(i => ToResponseDto(i))
            .ToListAsync();
    }

    public async Task<ItemResponseDto?> GetByIdAsync(int id)
    {
        var item = await _db.Items.FindAsync(id);
        return item == null ? null : ToResponseDto(item);
    }

    public async Task<ItemResponseDto> CreateAsync(ItemCreateDto dto)
    {
        var item = new Item
        {
            Name = dto.Name,
            Code = dto.Code,
            Unit = dto.Unit,
            ItemType = dto.ItemType,
            VatRate = dto.VatRate,
            SdRate = dto.SdRate
        };

        _db.Items.Add(item);
        await _db.SaveChangesAsync();
        return ToResponseDto(item);
    }

    public async Task<ItemResponseDto?> UpdateAsync(int id, ItemUpdateDto dto)
    {
        var item = await _db.Items.FindAsync(id);
        if (item == null) return null;

        item.Name = dto.Name;
        item.Unit = dto.Unit;
        item.ItemType = dto.ItemType;
        item.VatRate = dto.VatRate;
        item.SdRate = dto.SdRate;

        await _db.SaveChangesAsync();
        return ToResponseDto(item);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var item = await _db.Items.FindAsync(id);
        if (item == null) return false;

        item.IsActive = false; // soft delete
        await _db.SaveChangesAsync();
        return true;
    }

    private static ItemResponseDto ToResponseDto(Item item) => new(
        item.Id,
        item.Name,
        item.Code,
        item.Unit,
        item.ItemType,
        item.VatRate,
        item.SdRate,
        item.IsActive,
        item.CreatedAt
    );
}