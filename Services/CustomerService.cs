using Microsoft.EntityFrameworkCore;
using MyFirstApi.Data;
using MyFirstApi.DTOs;
using MyFirstApi.Models;

namespace MyFirstApi.Services;

public class CustomerService
{
    private readonly AppDbContext _db;

    public CustomerService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<CustomerResponseDto>> GetAllAsync()
    {
        return await _db.Customers
            .Where(c => c.IsActive)
            .Select(c => ToResponseDto(c))
            .ToListAsync();
    }

    public async Task<CustomerResponseDto?> GetByIdAsync(int id)
    {
        var customer = await _db.Customers.FindAsync(id);
        return customer == null ? null : ToResponseDto(customer);
    }

    public async Task<CustomerResponseDto> CreateAsync(CustomerCreateDto dto)
    {
        var customer = new Customer
        {
            Name = dto.Name,
            Address = dto.Address,
            Phone = dto.Phone,
            Email = dto.Email,
            VatRegNo = dto.VatRegNo,
            CompanyId = dto.CompanyId
        };

        _db.Customers.Add(customer);
        await _db.SaveChangesAsync();
        return ToResponseDto(customer);
    }

    public async Task<CustomerResponseDto?> UpdateAsync(int id, CustomerUpdateDto dto)
    {
        var customer = await _db.Customers.FindAsync(id);
        if (customer == null) return null;

        customer.Name = dto.Name;
        customer.Address = dto.Address;
        customer.Phone = dto.Phone;
        customer.Email = dto.Email;
        customer.VatRegNo = dto.VatRegNo;
        customer.IsActive = dto.IsActive;
        customer.CompanyId = dto.CompanyId;

        await _db.SaveChangesAsync();
        return ToResponseDto(customer);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var customer = await _db.Customers.FindAsync(id);
        if (customer == null) return false;

        customer.IsActive = false;
        await _db.SaveChangesAsync();
        return true;
    }

    private static CustomerResponseDto ToResponseDto(Customer customer) => new(
        customer.Id,
        customer.Name,
        customer.Address,
        customer.Phone,
        customer.Email,
        customer.VatRegNo,
        customer.IsActive,
        customer.CompanyId,
        customer.CreatedAt
    );
}
