using Microsoft.EntityFrameworkCore;
using PitStop.Application.Interfaces;
using PitStop.Domain.Entities;
using PitStop.Infrastructure.Data;

namespace PitStop.Infrastructure.Repositories;

public class FavoriteShopRepository(IDbContextFactory<AppDbContext> factory) : IFavoriteShopRepository
{
    public async Task<List<FavoriteShop>> GetByUserIdAsync(string userId)
    {
        await using var ctx = factory.CreateDbContext();
        return await ctx.FavoriteShops
            .AsNoTracking()
            .Where(f => f.UserId == userId)
            .Include(f => f.Shop)
                .ThenInclude(s => s.Photos.OrderBy(p => p.DisplayOrder))
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();
    }

    public async Task AddAsync(string userId, int shopId)
    {
        await using var ctx = factory.CreateDbContext();
        var alreadyExists = await ctx.FavoriteShops
            .AnyAsync(f => f.UserId == userId && f.ShopId == shopId);

        if (alreadyExists) return;

        ctx.FavoriteShops.Add(new FavoriteShop { UserId = userId, ShopId = shopId });
        await ctx.SaveChangesAsync();
    }

    public async Task RemoveAsync(string userId, int shopId)
    {
        await using var ctx = factory.CreateDbContext();
        await ctx.FavoriteShops
            .Where(f => f.UserId == userId && f.ShopId == shopId)
            .ExecuteDeleteAsync();
    }

    public async Task<bool> IsFavoriteAsync(string userId, int shopId)
    {
        await using var ctx = factory.CreateDbContext();
        return await ctx.FavoriteShops
            .AnyAsync(f => f.UserId == userId && f.ShopId == shopId);
    }
}
