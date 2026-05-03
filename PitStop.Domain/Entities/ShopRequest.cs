using PitStop.Domain.Enums;

namespace PitStop.Domain.Entities;

/// <summary>
///     A public onboarding request submitted before a shop is created.
///     Does not inherit BaseEntity — has no UpdatedAt and uses its own Id.
/// </summary>
public class ShopRequest
{
    public int Id { get; set; }
    public string ShopName { get; set; } = string.Empty;
    public ShopCategory Category { get; set; }
    public string City { get; set; } = string.Empty;
    public string County { get; set; } = string.Empty;
    public string ContactPerson { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ShopRequestStatus Status { get; set; } = ShopRequestStatus.Pending;
    public string? AdminNote { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}