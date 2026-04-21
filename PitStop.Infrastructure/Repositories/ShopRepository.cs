using Microsoft.EntityFrameworkCore;
using PitStop.Application.Interfaces;
using PitStop.Domain.Entities;
using PitStop.Domain.Enums;
using PitStop.Infrastructure.Data;

namespace PitStop.Infrastructure.Repositories;

public class ShopRepository(IDbContextFactory<AppDbContext> factory) : IShopRepository
{
    public async Task<Shop?> GetByIdAsync(int id)
    {
        await using var ctx = factory.CreateDbContext();
        return await ctx.Shops
            .AsNoTracking()
            .Include(s => s.Photos.OrderBy(p => p.DisplayOrder))
            .Include(s => s.Hours.OrderBy(h => h.DayOfWeek))
            .Include(s => s.Services)
            .Include(s => s.Brands)
            .Include(s => s.Reviews.OrderByDescending(r => r.CreatedAt))
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<List<Shop>> GetAllAsync()
    {
        await using var ctx = factory.CreateDbContext();
        return await ctx.Shops
            .AsNoTracking()
            .Include(s => s.Photos.OrderBy(p => p.DisplayOrder))
            .OrderByDescending(s => s.AverageRating)
            .ToListAsync();
    }

    public async Task<(List<Shop> Items, int TotalCount)> SearchAsync(
        string query,
        string city,
        ShopCategory? category,
        double? minRating,
        bool? openNow,
        int page,
        int pageSize)
    {
        await using var ctx = factory.CreateDbContext();
        var q = ctx.Shops
            .AsNoTracking()
            .Where(s => s.Status == ShopStatus.Active)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
            q = q.Where(s => s.Name.Contains(query) ||
                              s.Description.Contains(query) ||
                              s.City.Contains(query));

        if (!string.IsNullOrWhiteSpace(city))
            q = q.Where(s => s.City == city);

        if (category.HasValue)
            q = q.Where(s => s.Category == category.Value);

        if (minRating.HasValue)
            q = q.Where(s => s.AverageRating >= minRating.Value);

        if (openNow == true)
        {
            var now = DateTime.UtcNow;
            var dayOfWeek = ((int)now.DayOfWeek + 6) % 7;
            var currentTime = TimeOnly.FromDateTime(now);

            q = q.Where(s => s.Hours.Any(h =>
                h.DayOfWeek == dayOfWeek &&
                !h.IsClosed &&
                h.OpenTime != null &&
                h.CloseTime != null &&
                h.OpenTime <= currentTime &&
                currentTime <= h.CloseTime));
        }

        var totalCount = await q.CountAsync();

        var items = await q
            .Include(s => s.Photos.OrderBy(p => p.DisplayOrder))
            .Include(s => s.Hours)
            .OrderByDescending(s => s.AverageRating)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<List<Shop>> GetFeaturedAsync(int count)
    {
        await using var ctx = factory.CreateDbContext();
        return await ctx.Shops
            .AsNoTracking()
            .Where(s => s.Status == ShopStatus.Active)
            .Include(s => s.Photos.OrderBy(p => p.DisplayOrder))
            .OrderByDescending(s => s.AverageRating)
            .ThenByDescending(s => s.ReviewCount)
            .Take(count)
            .ToListAsync();
    }

    public async Task<Shop?> GetByOwnerIdAsync(string ownerId)
    {
        await using var ctx = factory.CreateDbContext();
        return await ctx.Shops
            .AsNoTracking()
            .Include(s => s.Photos.OrderBy(p => p.DisplayOrder))
            .Include(s => s.Hours.OrderBy(h => h.DayOfWeek))
            .Include(s => s.Services)
            .Include(s => s.Brands)
            .FirstOrDefaultAsync(s => s.OwnerId == ownerId);
    }

    public async Task<Shop> CreateAsync(Shop shop)
    {
        await using var ctx = factory.CreateDbContext();
        ctx.Shops.Add(shop);
        await ctx.SaveChangesAsync();
        return shop;
    }

    public async Task<Shop> UpdateAsync(Shop shop)
    {
        await using var ctx = factory.CreateDbContext();
        ctx.Shops.Update(shop);
        await ctx.SaveChangesAsync();
        return shop;
    }

    public async Task UpdateProfileAsync(int id, string name, string description, string address,
        string city, string county, string phone, string email, string? website, string? sector)
    {
        await using var ctx = factory.CreateDbContext();
        var now = DateTime.UtcNow;
        await ctx.Shops.Where(s => s.Id == id).ExecuteUpdateAsync(s => s
            .SetProperty(p => p.Name, name)
            .SetProperty(p => p.Description, description)
            .SetProperty(p => p.Address, address)
            .SetProperty(p => p.City, city)
            .SetProperty(p => p.County, county)
            .SetProperty(p => p.Phone, phone)
            .SetProperty(p => p.Email, email)
            .SetProperty(p => p.Website, website)
            .SetProperty(p => p.Sector, sector)
            .SetProperty(p => p.UpdatedAt, now));
    }

    public async Task DeleteAsync(int id)
    {
        await using var ctx = factory.CreateDbContext();
        await ctx.Shops.Where(s => s.Id == id).ExecuteDeleteAsync();
    }

    public async Task<ShopService> AddServiceAsync(ShopService service)
    {
        await using var ctx = factory.CreateDbContext();
        ctx.ShopServices.Add(service);
        await ctx.SaveChangesAsync();
        return service;
    }

    public async Task UpdateServiceAsync(ShopService service)
    {
        await using var ctx = factory.CreateDbContext();
        ctx.ShopServices.Update(service);
        await ctx.SaveChangesAsync();
    }

    public async Task DeleteServiceAsync(int id)
    {
        await using var ctx = factory.CreateDbContext();
        await ctx.ShopServices.Where(s => s.Id == id).ExecuteDeleteAsync();
    }

    public async Task UpsertHoursAsync(int shopId, List<ShopHour> hours)
    {
        await using var ctx = factory.CreateDbContext();
        await ctx.ShopHours.Where(h => h.ShopId == shopId).ExecuteDeleteAsync();
        foreach (var hour in hours)
        {
            hour.Id = 0;
            hour.ShopId = shopId;
        }
        ctx.ShopHours.AddRange(hours);
        await ctx.SaveChangesAsync();
    }

    public async Task SetStatusAsync(int shopId, ShopStatus status)
    {
        await using var ctx = factory.CreateDbContext();
        var now = DateTime.UtcNow;
        await ctx.Shops.Where(s => s.Id == shopId).ExecuteUpdateAsync(s => s
            .SetProperty(p => p.Status, status)
            .SetProperty(p => p.UpdatedAt, now));
    }
}
