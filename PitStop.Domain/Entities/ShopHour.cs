namespace PitStop.Domain.Entities;

public class ShopHour : BaseEntity
{
    public int ShopId { get; set; }

    /// <summary>Day of week: 0 = Monday, 6 = Sunday.</summary>
    public int DayOfWeek { get; set; }

    public TimeOnly? OpenTime { get; set; }
    public TimeOnly? CloseTime { get; set; }
    public bool IsClosed { get; set; }

    public Shop Shop { get; set; } = null!;
}