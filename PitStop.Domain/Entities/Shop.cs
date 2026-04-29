using PitStop.Domain.Enums;

namespace PitStop.Domain.Entities;

public class Shop : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string County { get; set; } = string.Empty;
    public string? Sector { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Website { get; set; }
    public string? CoverImage { get; set; }
    public ShopCategory Category { get; set; }
    public ShopStatus Status { get; set; } = ShopStatus.Pending;

    /// <summary>Identity user ID of the shop owner. Null until owner completes setup.</summary>
    public string? OwnerId { get; set; }

    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public int ViewCount { get; set; }

    // Navigation properties
    public ICollection<Review> Reviews { get; set; } = [];
    public ICollection<ShopPhoto> Photos { get; set; } = [];
    public ICollection<ShopService> Services { get; set; } = [];
    public ICollection<ShopHour> Hours { get; set; } = [];
    public ICollection<FavoriteShop> FavoriteShops { get; set; } = [];
    public ICollection<ShopBrand> Brands { get; set; } = [];
}
