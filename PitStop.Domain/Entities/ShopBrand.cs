namespace PitStop.Domain.Entities;

public class ShopBrand : BaseEntity
{
    public int ShopId { get; set; }
    public string Name { get; set; } = string.Empty;

    public Shop Shop { get; set; } = null!;
}
