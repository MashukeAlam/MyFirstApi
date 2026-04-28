namespace MyFirstApi.Models;

public class ProductionOrder
{
    public int Id { get; set; }
    public int BomHeaderId { get; set; }
    public BomHeader BomHeader { get; set; } = null!;
    public decimal QuantityToProduce { get; set; }
    public string Status { get; set; } = "Draft"; // Draft/InProgress/Completed
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public string Notes { get; set; } = string.Empty;
    public int CreatedByUserId { get; set; }
}
