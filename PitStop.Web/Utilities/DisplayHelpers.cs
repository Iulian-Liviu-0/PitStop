using PitStop.Domain.Enums;

namespace PitStop.Web.Utilities;

public static class DisplayHelpers
{
    public static string CategoryDisplayName(ShopCategory cat)
    {
        return cat switch
        {
            ShopCategory.ServiceAuto => "Service Auto",
            ShopCategory.PieseAuto => "Piese Auto",
            ShopCategory.Spalatorie => "Spălătorie",
            ShopCategory.Vopsitorie => "Vopsitorie",
            ShopCategory.Vulcanizare => "Vulcanizare",
            ShopCategory.Tuning => "Tuning",
            ShopCategory.Tractari => "Tractări",
            ShopCategory.Detailing => "Detailing",
            ShopCategory.ITP => "ITP",
            ShopCategory.ServiceMoto => "Service Moto",
            ShopCategory.PieseMoto => "Piese Moto",
            ShopCategory.Altele => "Altele",
            _ => cat.ToString()
        };
    }

    public static string Initials(string name)
    {
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length >= 2
            ? $"{parts[0][0]}{parts[1][0]}".ToUpper()
            : name.Length > 0
                ? name[0].ToString().ToUpper()
                : "?";
    }
}