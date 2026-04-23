using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PitStop.Application.Interfaces;
using PitStop.Domain.Entities;
using PitStop.Domain.Enums;
using PitStop.Infrastructure.Data;

namespace PitStop.Infrastructure.Repositories;

public class ShopRequestRepository(IDbContextFactory<AppDbContext> factory, ILogger<ShopRequestRepository> logger) : IShopRequestRepository
{
    public async Task<List<ShopRequest>> GetAllAsync()
    {
        await using var ctx = await factory.CreateDbContextAsync();
        return await ctx.ShopRequests
            .AsNoTracking()
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<ShopRequest>> GetPendingAsync()
    {
        await using var ctx = await factory.CreateDbContextAsync();
        return await ctx.ShopRequests
            .AsNoTracking()
            .Where(r => r.Status == ShopRequestStatus.Pending)
            // Oldest first so admins process in FIFO order
            .OrderBy(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<ShopRequest?> GetByIdAsync(int id)
    {
        await using var ctx = await factory.CreateDbContextAsync();
        return await ctx.ShopRequests
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<ShopRequest> CreateAsync(ShopRequest request)
    {
        await using var ctx = await factory.CreateDbContextAsync();
        ctx.ShopRequests.Add(request);
        await ctx.SaveChangesAsync();
        logger.LogInformation("ShopRequest created: id={Id} shop={ShopName} email={Email}", request.Id, request.ShopName, request.Email);
        return request;
    }

    public async Task<ShopRequest> UpdateAsync(ShopRequest request)
    {
        await using var ctx = await factory.CreateDbContextAsync();
        ctx.ShopRequests.Update(request);
        await ctx.SaveChangesAsync();
        logger.LogInformation("ShopRequest updated: id={Id} status={Status}", request.Id, request.Status);
        return request;
    }
}
