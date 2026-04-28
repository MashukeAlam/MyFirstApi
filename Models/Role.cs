namespace MyFirstApi.Models;

public class Role
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty; // Admin, PurchaseManager, SalesEntry etc.

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}