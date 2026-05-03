namespace PitStop.Domain.Entities;

public class FavoriteShop : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public int ShopId { get; set; }

    public Shop Shop { get; set; } = null!;
}