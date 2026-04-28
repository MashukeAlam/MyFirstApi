using Microsoft.EntityFrameworkCore;
using MyFirstApi.Data;
using MyFirstApi.DTOs;
using MyFirstApi.Models;

namespace MyFirstApi.Services;

public class VendorService
{
    private readonly AppDbContext _db;

    public VendorService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<VendorResponseDto>> GetAllAsync()
    {
        return await _db.Vendors
            .Where(v => v.IsActive)
            .Select(v => ToResponseDto(v))
            .ToListAsync();
    }

    public async Task<VendorResponseDto?> GetByIdAsync(int id)
    {
        var vendor = await _db.Vendors.FindAsync(id);
        return vendor == null ? null : ToResponseDto(vendor);
    }

    public async Task<VendorResponseDto> CreateAsync(VendorCreateDto dto)
    {
        var vendor = new Vendor
        {
            Name = dto.Name,
            Address = dto.Address,
            Phone = dto.Phone,
            Email = dto.Email,
            VatRegNo = dto.VatRegNo,
            CompanyId = dto.CompanyId
        };

        _db.Vendors.Add(vendor);
        await _db.SaveChangesAsync();
        return ToResponseDto(vendor);
    }

    public async Task<VendorResponseDto?> UpdateAsync(int id, VendorUpdateDto dto)
    {
        var vendor = await _db.Vendors.FindAsync(id);
        if (vendor == null) return null;

        vendor.Name = dto.Name;
        vendor.Address = dto.Address;
        vendor.Phone = dto.Phone;
        vendor.Email = dto.Email;
        vendor.VatRegNo = dto.VatRegNo;
        vendor.IsActive = dto.IsActive;
        vendor.CompanyId = dto.CompanyId;

        await _db.SaveChangesAsync();
        return ToResponseDto(vendor);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var vendor = await _db.Vendors.FindAsync(id);
        if (vendor == null) return false;

        vendor.IsActive = false;
        await _db.SaveChangesAsync();
        return true;
    }

    private static VendorResponseDto ToResponseDto(Vendor vendor) => new(
        vendor.Id,
        vendor.Name,
        vendor.Address,
        vendor.Phone,
        vendor.Email,
        vendor.VatRegNo,
        vendor.IsActive,
        vendor.CompanyId,
        vendor.CreatedAt
    );
}
