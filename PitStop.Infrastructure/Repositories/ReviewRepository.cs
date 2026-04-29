using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PitStop.Application.Interfaces;
using PitStop.Domain.Entities;
using PitStop.Infrastructure.Data;

namespace PitStop.Infrastructure.Repositories;

public class ReviewRepository(IDbContextFactory<AppDbContext> factory, ILogger<ReviewRepository> logger) : IReviewRepository
{
    public async Task<(List<Review> Items, int TotalCount)> GetByShopIdAsync(int shopId, int page, int pageSize)
    {
        await using var ctx = await factory.CreateDbContextAsync();
        var q = ctx.Reviews
            .AsNoTracking()
            .Where(r => r.ShopId == shopId)
            .OrderByDescending(r => r.CreatedAt);

        var totalCount = await q.CountAsync();
        var items = await q
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<List<Review>> GetByUserIdAsync(string userId)
    {
        await using var ctx = await factory.CreateDbContextAsync();
        return await ctx.Reviews
            .AsNoTracking()
            .Include(r => r.Shop)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<Review> CreateAsync(Review review)
    {
        await using var ctx = await factory.CreateDbContextAsync();
        ctx.Reviews.Add(review);
        await ctx.SaveChangesAsync();
        logger.LogInformation("Review created: shop={ShopId} user={UserId} rating={Rating}", review.ShopId, review.UserId, review.Rating);
        return review;
    }

    public async Task<Review> UpdateAsync(Review review)
    {
        await using var ctx = await factory.CreateDbContextAsync();
        ctx.Reviews.Update(review);
        await ctx.SaveChangesAsync();
        return review;
    }

    public async Task DeleteAsync(int id)
    {
        await using var ctx = await factory.CreateDbContextAsync();
        await ctx.Reviews.Where(r => r.Id == id).ExecuteDeleteAsync();
        logger.LogInformation("Review deleted: id={ReviewId}", id);
    }

    public async Task<double> GetAverageRatingAsync(int shopId)
    {
        await using var ctx = await factory.CreateDbContextAsync();
        return await ctx.Reviews
            .Where(r => r.ShopId == shopId)
            .AverageAsync(r => (double?)r.Rating) ?? 0.0;
    }

    public async Task<int> GetTotalCountAsync()
    {
        await using var ctx = await factory.CreateDbContextAsync();
        return await ctx.Reviews.CountAsync();
    }

    public async Task IncrementUsefulAsync(int reviewId)
    {
        await using var ctx = await factory.CreateDbContextAsync();
        await ctx.Reviews.Where(r => r.Id == reviewId)
            .ExecuteUpdateAsync(s => s.SetProperty(r => r.UsefulCount, r => r.UsefulCount + 1));
    }

    public async Task SetOwnerResponseAsync(int reviewId, string? response)
    {
        await using var ctx = await factory.CreateDbContextAsync();
        var now = string.IsNullOrWhiteSpace(response) ? (DateTime?)null : DateTime.UtcNow;
        await ctx.Reviews.Where(r => r.Id == reviewId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(r => r.OwnerResponse, response)
                .SetProperty(r => r.OwnerResponseAt, now));
    }
}
