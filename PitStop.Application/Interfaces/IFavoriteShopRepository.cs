using PitStop.Domain.Entities;

namespace PitStop.Application.Interfaces;

public interface IFavoriteShopRepository
{
    Task<List<FavoriteShop>> GetByUserIdAsync(string userId);
    Task AddAsync(string userId, int shopId);
    Task RemoveAsync(string userId, int shopId);
    Task<bool> IsFavoriteAsync(string userId, int shopId);
}