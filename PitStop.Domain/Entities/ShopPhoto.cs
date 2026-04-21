namespace PitStop.Domain.Entities;

public class ShopPhoto : BaseEntity
{
    public int ShopId { get; set; }
    public string Url { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }

    public Shop Shop { get; set; } = null!;
}
