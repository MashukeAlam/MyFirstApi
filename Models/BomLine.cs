namespace MyFirstApi.Models;

public class BomLine
{
    public int Id { get; set; }
    public int BomHeaderId { get; set; }
    public BomHeader BomHeader { get; set; } = null!;
    public int RawMaterialItemId { get; set; }
    public Item RawMaterialItem { get; set; } = null!;
    public decimal QuantityRequired { get; set; }
}
