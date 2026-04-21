namespace PitStop.Domain.Entities;

public class Review : BaseEntity
{
    public int ShopId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string UserInitials { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Text { get; set; } = string.Empty;
    public int UsefulCount { get; set; }

    public Shop Shop { get; set; } = null!;
}
