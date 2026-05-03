namespace PitStop.Web.Components.Shared.ShopProfile;

public record ServiceItem(string Name, string Description, string PriceRange);

public record ReviewItem(
    int Id,
    string Author,
    string Initials,
    int Rating,
    string Date,
    string Text,
    int UsefulCount,
    string? OwnerResponse,
    string? OwnerResponseDate);

public record HourItem(string DayName, string Open, string Close, bool IsClosed, bool IsToday);

public record RatingBar(int Stars, int Percentage);

public record SimilarShop(
    int Id,
    string Name,
    string Sector,
    double Rating,
    int ReviewCount,
    string ImageUrl,
    List<string> Tags);

public record ShopProfileData(
    string Name,
    string Category,
    string City,
    string County,
    string Address,
    string Phone,
    string Email,
    string Website,
    double Rating,
    int ReviewCount,
    bool IsOpen,
    string ClosingTime,
    string CoverImage,
    string Description,
    List<string> Brands,
    List<ServiceItem> Services,
    List<string> Photos,
    List<ReviewItem> Reviews,
    List<HourItem> Hours
);