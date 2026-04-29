using PitStop.Domain.Entities;
using PitStop.Domain.Enums;

namespace PitStop.Application.Interfaces;

public interface IShopRepository
{
    Task<Shop?> GetByIdAsync(int id);
    Task<List<Shop>> GetAllAsync();
    Task<(List<Shop> Items, int TotalCount)> SearchAsync(
        string query,
        string county,
        ShopCategory? category,
        double? minRating,
        bool? openNow,
        int page,
        int pageSize);
    Task<List<Shop>> GetFeaturedAsync(int count);
    Task<Shop?> GetByOwnerIdAsync(string ownerId);
    Task<Shop> CreateAsync(Shop shop);
    Task<Shop> UpdateAsync(Shop shop);
    Task UpdateProfileAsync(int id, string name, string description, string address,
        string city, string county, string phone, string email, string? website, string? sector);
    Task DeleteAsync(int id);
    Task<ShopService> AddServiceAsync(ShopService service);
    Task UpdateServiceAsync(ShopService service);
    Task DeleteServiceAsync(int id);
    Task UpsertHoursAsync(int shopId, List<ShopHour> hours);
    Task SetStatusAsync(int shopId, ShopStatus status);
    Task<(int ShopCount, int CountyCount, double AvgRating, int ReviewCount)> GetSiteStatsAsync();
    Task RecalcRatingAsync(int shopId);
    Task<ShopPhoto> AddPhotoAsync(ShopPhoto photo);
    Task DeletePhotoAsync(int photoId);
    Task SetCoverImageAsync(int shopId, string? url);
    Task<ShopBrand> AddBrandAsync(ShopBrand brand);
    Task DeleteBrandAsync(int brandId);
    Task UpdatePhotoOrderAsync(List<(int PhotoId, int DisplayOrder)> updates);
    Task IncrementViewCountAsync(int shopId);
}
