namespace MyFirstApi.Models;

public class BomHeader
{
    public int Id { get; set; }
    public int FinishedItemId { get; set; }
    public Item FinishedItem { get; set; } = null!;
    public string Version { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string Notes { get; set; } = string.Empty;

    public ICollection<BomLine> Lines { get; set; } = new List<BomLine>();
}
