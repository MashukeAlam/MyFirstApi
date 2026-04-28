using Microsoft.EntityFrameworkCore;
using MyFirstApi.Data;
using System.Security.Claims;

namespace MyFirstApi.Services;

public class PermissionService
{
    private readonly AppDbContext _db;

    public PermissionService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<bool> HasPermissionAsync(
        ClaimsPrincipal user,
        string module,
        string action) // View, Create, Edit, Delete
    {
        var roles = user.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();

        if (roles.Contains("Admin")) return true; // admin bypasses all checks

        return await _db.RolePermissions
            .Include(rp => rp.Role)
            .Where(rp => roles.Contains(rp.Role.Name) && rp.Module == module)
            .AnyAsync(rp =>
                action == "View" ? rp.CanView :
                action == "Create" ? rp.CanCreate :
                action == "Edit" ? rp.CanEdit :
                rp.CanDelete);
    }
}