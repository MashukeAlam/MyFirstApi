namespace MyFirstApi.Models;

public class PurchaseOrder
{
    public int Id { get; set; }
    public int VendorId { get; set; }
    public Vendor Vendor { get; set; } = null!;
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Draft"; // Draft/Confirmed/Received
    public string Notes { get; set; } = string.Empty;
    public int CreatedByUserId { get; set; }
    public int CompanyId { get; set; }

    public ICollection<PurchaseOrderLine> Lines { get; set; } = new List<PurchaseOrderLine>();
}
