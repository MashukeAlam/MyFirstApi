namespace MyFirstApi.Models
{
    public class Item
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public string ItemType { get; set; } = string.Empty; // raw/finished/both
        public decimal VatRate { get; set; }
        public decimal SdRate { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
