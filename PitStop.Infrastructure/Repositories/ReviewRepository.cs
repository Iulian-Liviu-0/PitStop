using Microsoft.EntityFrameworkCore;
using PitStop.Application.Interfaces;
using PitStop.Domain.Entities;
using PitStop.Infrastructure.Data;

namespace PitStop.Infrastructure.Repositories;

public class ReviewRepository(IDbContextFactory<AppDbContext> factory) : IReviewRepository
{
    public async Task<(List<Review> Items, int TotalCount)> GetByShopIdAsync(int shopId, int page, int pageSize)
    {
        await using var ctx = factory.CreateDbContext();
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
        await using var ctx = factory.CreateDbContext();
        return await ctx.Reviews
            .AsNoTracking()
            .Include(r => r.Shop)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<Review> CreateAsync(Review review)
    {
        await using var ctx = factory.CreateDbContext();
        ctx.Reviews.Add(review);
        await ctx.SaveChangesAsync();
        return review;
    }

    public async Task<Review> UpdateAsync(Review review)
    {
        await using var ctx = factory.CreateDbContext();
        ctx.Reviews.Update(review);
        await ctx.SaveChangesAsync();
        return review;
    }

    public async Task DeleteAsync(int id)
    {
        await using var ctx = factory.CreateDbContext();
        await ctx.Reviews.Where(r => r.Id == id).ExecuteDeleteAsync();
    }

    public async Task<double> GetAverageRatingAsync(int shopId)
    {
        await using var ctx = factory.CreateDbContext();
        return await ctx.Reviews
            .Where(r => r.ShopId == shopId)
            .AverageAsync(r => (double?)r.Rating) ?? 0.0;
    }

    public async Task<int> GetTotalCountAsync()
    {
        await using var ctx = factory.CreateDbContext();
        return await ctx.Reviews.CountAsync();
    }
}
