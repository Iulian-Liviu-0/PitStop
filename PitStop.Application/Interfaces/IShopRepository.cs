using PitStop.Domain.Entities;
using PitStop.Domain.Enums;

namespace PitStop.Application.Interfaces;

public interface IShopRepository
{
    Task<Shop?> GetByIdAsync(int id);
    Task<List<Shop>> GetAllAsync();
    Task<(List<Shop> Items, int TotalCount)> SearchAsync(
        string query,
        string city,
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
}
