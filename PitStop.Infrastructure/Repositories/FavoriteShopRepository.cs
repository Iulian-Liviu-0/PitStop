using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PitStop.Application.Interfaces;
using PitStop.Domain.Entities;
using PitStop.Infrastructure.Data;

namespace PitStop.Infrastructure.Repositories;

public class FavoriteShopRepository(IDbContextFactory<AppDbContext> factory, ILogger<FavoriteShopRepository> logger) : IFavoriteShopRepository
{
    public async Task<List<FavoriteShop>> GetByUserIdAsync(string userId)
    {
        await using var ctx = await factory.CreateDbContextAsync();
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
        await using var ctx = await factory.CreateDbContextAsync();
        var alreadyExists = await ctx.FavoriteShops
            .AnyAsync(f => f.UserId == userId && f.ShopId == shopId);

        if (alreadyExists) return;

        ctx.FavoriteShops.Add(new FavoriteShop { UserId = userId, ShopId = shopId });
        await ctx.SaveChangesAsync();
        logger.LogDebug("Favorite added: user={UserId} shop={ShopId}", userId, shopId);
    }

    public async Task RemoveAsync(string userId, int shopId)
    {
        await using var ctx = await factory.CreateDbContextAsync();
        await ctx.FavoriteShops
            .Where(f => f.UserId == userId && f.ShopId == shopId)
            .ExecuteDeleteAsync();
        logger.LogDebug("Favorite removed: user={UserId} shop={ShopId}", userId, shopId);
    }

    public async Task<bool> IsFavoriteAsync(string userId, int shopId)
    {
        await using var ctx = await factory.CreateDbContextAsync();
        return await ctx.FavoriteShops
            .AnyAsync(f => f.UserId == userId && f.ShopId == shopId);
    }
}
