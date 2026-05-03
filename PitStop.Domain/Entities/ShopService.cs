namespace PitStop.Domain.Entities;

public class ShopService : BaseEntity
{
    public int ShopId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal PriceMin { get; set; }
    public decimal PriceMax { get; set; }

    public Shop Shop { get; set; } = null!;
}