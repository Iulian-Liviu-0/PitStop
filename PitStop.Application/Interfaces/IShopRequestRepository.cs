using PitStop.Domain.Entities;

namespace PitStop.Application.Interfaces;

public interface IShopRequestRepository
{
    Task<List<ShopRequest>> GetAllAsync();
    Task<List<ShopRequest>> GetPendingAsync();
    Task<ShopRequest?> GetByIdAsync(int id);
    Task<ShopRequest> CreateAsync(ShopRequest request);
    Task<ShopRequest> UpdateAsync(ShopRequest request);
}
