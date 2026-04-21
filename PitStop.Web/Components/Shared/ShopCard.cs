namespace PitStop.Web.Components.Shared;

public enum ShopCardVariant { Grid, Row }

public record ShopCardModel(
    int Id,
    string Name,
    string Category,
    string Location,
    double Rating,
    string Description,
    string ImageUrl,
    string? Icon = null,
    int? ReviewCount = null,
    bool? IsOpen = null
);
