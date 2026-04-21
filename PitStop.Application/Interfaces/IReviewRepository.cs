using PitStop.Domain.Entities;

namespace PitStop.Application.Interfaces;

public interface IReviewRepository
{
    Task<(List<Review> Items, int TotalCount)> GetByShopIdAsync(int shopId, int page, int pageSize);
    Task<List<Review>> GetByUserIdAsync(string userId);
    Task<Review> CreateAsync(Review review);
    Task<Review> UpdateAsync(Review review);
    Task DeleteAsync(int id);
    Task<double> GetAverageRatingAsync(int shopId);
    Task<int> GetTotalCountAsync();
}
