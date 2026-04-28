namespace MyFirstApi.Models;

public class InventoryMovement
{
    public int Id { get; set; }
    public int ItemId { get; set; }
    public Item Item { get; set; } = null!;
    public string MovementType { get; set; } = string.Empty; // IN/OUT
    public decimal Quantity { get; set; }
    public string ReferenceType { get; set; } = string.Empty; // Purchase/Sale/Production
    public int ReferenceId { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public string Notes { get; set; } = string.Empty;
}
