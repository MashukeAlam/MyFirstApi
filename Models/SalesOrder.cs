namespace MyFirstApi.Models;

public class SalesOrder
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Draft"; // Draft/Confirmed/Delivered
    public string Notes { get; set; } = string.Empty;
    public int CreatedByUserId { get; set; }
    public int CompanyId { get; set; }

    public ICollection<SalesOrderLine> Lines { get; set; } = new List<SalesOrderLine>();
}
