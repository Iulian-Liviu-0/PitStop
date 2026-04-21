using Microsoft.EntityFrameworkCore;
using PitStop.Application.Interfaces;
using PitStop.Domain.Entities;
using PitStop.Domain.Enums;
using PitStop.Infrastructure.Data;

namespace PitStop.Infrastructure.Repositories;

public class ShopRequestRepository(IDbContextFactory<AppDbContext> factory) : IShopRequestRepository
{
    public async Task<List<ShopRequest>> GetAllAsync()
    {
        await using var ctx = factory.CreateDbContext();
        return await ctx.ShopRequests
            .AsNoTracking()
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<ShopRequest>> GetPendingAsync()
    {
        await using var ctx = factory.CreateDbContext();
        return await ctx.ShopRequests
            .AsNoTracking()
            .Where(r => r.Status == ShopRequestStatus.Pending)
            .OrderBy(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<ShopRequest?> GetByIdAsync(int id)
    {
        await using var ctx = factory.CreateDbContext();
        return await ctx.ShopRequests
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<ShopRequest> CreateAsync(ShopRequest request)
    {
        await using var ctx = factory.CreateDbContext();
        ctx.ShopRequests.Add(request);
        await ctx.SaveChangesAsync();
        return request;
    }

    public async Task<ShopRequest> UpdateAsync(ShopRequest request)
    {
        await using var ctx = factory.CreateDbContext();
        ctx.ShopRequests.Update(request);
        await ctx.SaveChangesAsync();
        return request;
    }
}
