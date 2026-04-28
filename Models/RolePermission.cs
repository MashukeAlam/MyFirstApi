using MyFirstApi.Models;

namespace MyFirstApi.Models;

public class RolePermission
{
    public int Id { get; set; }
    public int RoleId { get; set; }
    public Role Role { get; set; } = null!;

    public string Module { get; set; } = string.Empty; // Items, Purchase, Sales, Production, Reports
    public bool CanView { get; set; }
    public bool CanCreate { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
}