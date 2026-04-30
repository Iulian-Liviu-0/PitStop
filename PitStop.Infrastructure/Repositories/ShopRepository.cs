using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PitStop.Application.Interfaces;
using PitStop.Domain.Entities;
using PitStop.Domain.Enums;
using PitStop.Infrastructure.Data;

namespace PitStop.Infrastructure.Repositories;

public class ShopRepository(IDbContextFactory<AppDbContext> factory, ILogger<ShopRepository> logger) : IShopRepository
{
    public async Task<Shop?> GetByIdAsync(int id)
    {
        await using var ctx = await factory.CreateDbContextAsync();
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
        await using var ctx = await factory.CreateDbContextAsync();
        return await ctx.Shops
            .AsNoTracking()
            .Include(s => s.Photos.OrderBy(p => p.DisplayOrder))
            .OrderByDescending(s => s.AverageRating)
            .ToListAsync();
    }

    public async Task<(List<Shop> Items, int TotalCount)> SearchAsync(
        string query,
        string county,
        ShopCategory? category,
        double? minRating,
        bool? openNow,
        int page,
        int pageSize)
    {
        await using var ctx = await factory.CreateDbContextAsync();
        var q = ctx.Shops
            .AsNoTracking()
            .Where(s => s.Status == ShopStatus.Active)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            foreach (var term in query.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                var t = term;
                q = q.Where(s => s.Name.ToLower().Contains(t) ||
                                  s.Description.ToLower().Contains(t) ||
                                  s.City.ToLower().Contains(t) ||
                                  s.Services.Any(sv => sv.Name.ToLower().Contains(t) ||
                                                       sv.Description.ToLower().Contains(t)) ||
                                  s.Brands.Any(b => b.Name.ToLower().Contains(t)));
            }
        }

        if (!string.IsNullOrWhiteSpace(county))
            q = q.Where(s => s.County == county);

        if (category.HasValue)
            q = q.Where(s => s.Category == category.Value);

        if (minRating.HasValue)
            q = q.Where(s => s.AverageRating >= minRating.Value);

        if (openNow == true)
        {
            var now = DateTime.UtcNow;
            // .NET DayOfWeek is Sunday=0; project convention is Monday=0, so shift by 6
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
        await using var ctx = await factory.CreateDbContextAsync();
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
        await using var ctx = await factory.CreateDbContextAsync();
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
        await using var ctx = await factory.CreateDbContextAsync();
        ctx.Shops.Add(shop);
        await ctx.SaveChangesAsync();
        logger.LogInformation("Shop created: id={ShopId} name={ShopName}", shop.Id, shop.Name);
        return shop;
    }

    public async Task<Shop> UpdateAsync(Shop shop)
    {
        await using var ctx = await factory.CreateDbContextAsync();
        ctx.Shops.Update(shop);
        await ctx.SaveChangesAsync();
        return shop;
    }

    public async Task UpdateProfileAsync(int id, string name, string description, string address,
        string city, string county, string phone, string email, string? website, string? sector)
    {
        await using var ctx = await factory.CreateDbContextAsync();
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
        await using var ctx = await factory.CreateDbContextAsync();
        await ctx.Shops.Where(s => s.Id == id).ExecuteDeleteAsync();
    }

    public async Task<ShopService> AddServiceAsync(ShopService service)
    {
        await using var ctx = await factory.CreateDbContextAsync();
        ctx.ShopServices.Add(service);
        await ctx.SaveChangesAsync();
        return service;
    }

    public async Task UpdateServiceAsync(ShopService service)
    {
        await using var ctx = await factory.CreateDbContextAsync();
        ctx.ShopServices.Update(service);
        await ctx.SaveChangesAsync();
    }

    public async Task DeleteServiceAsync(int id)
    {
        await using var ctx = await factory.CreateDbContextAsync();
        await ctx.ShopServices.Where(s => s.Id == id).ExecuteDeleteAsync();
    }

    public async Task UpsertHoursAsync(int shopId, List<ShopHour> hours)
    {
        await using var ctx = await factory.CreateDbContextAsync();
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
        await using var ctx = await factory.CreateDbContextAsync();
        var now = DateTime.UtcNow;
        await ctx.Shops.Where(s => s.Id == shopId).ExecuteUpdateAsync(s => s
            .SetProperty(p => p.Status, status)
            .SetProperty(p => p.UpdatedAt, now));
        logger.LogInformation("Shop status changed: id={ShopId} status={Status}", shopId, status);
    }

    public async Task<(int ShopCount, int CountyCount, double AvgRating, int ReviewCount)> GetSiteStatsAsync()
    {
        await using var ctx = await factory.CreateDbContextAsync();
        var active = ctx.Shops.Where(s => s.Status == ShopStatus.Active);
        var shopCount = await active.CountAsync();
        var countyCount = await active.Select(s => s.County).Distinct().CountAsync();
        var avgRating = shopCount > 0 ? await active.AverageAsync(s => s.AverageRating) : 0;
        var reviewCount = await ctx.Reviews.CountAsync();
        return (shopCount, countyCount, Math.Round(avgRating, 1), reviewCount);
    }

    public async Task<ShopPhoto> AddPhotoAsync(ShopPhoto photo)
    {
        await using var ctx = await factory.CreateDbContextAsync();
        var maxOrder = await ctx.ShopPhotos
            .Where(p => p.ShopId == photo.ShopId)
            .MaxAsync(p => (int?)p.DisplayOrder) ?? -1;
        photo.DisplayOrder = maxOrder + 1;
        ctx.ShopPhotos.Add(photo);
        await ctx.SaveChangesAsync();
        return photo;
    }

    public async Task DeletePhotoAsync(int photoId)
    {
        await using var ctx = await factory.CreateDbContextAsync();
        await ctx.ShopPhotos.Where(p => p.Id == photoId).ExecuteDeleteAsync();
    }

    public async Task SetCoverImageAsync(int shopId, string? url)
    {
        var now = DateTime.UtcNow;
        await using var ctx = await factory.CreateDbContextAsync();
        await ctx.Shops.Where(s => s.Id == shopId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(p => p.CoverImage, url)
                .SetProperty(p => p.UpdatedAt, now));
    }

    public async Task<ShopBrand> AddBrandAsync(ShopBrand brand)
    {
        await using var ctx = await factory.CreateDbContextAsync();
        ctx.ShopBrands.Add(brand);
        await ctx.SaveChangesAsync();
        return brand;
    }

    public async Task DeleteBrandAsync(int brandId)
    {
        await using var ctx = await factory.CreateDbContextAsync();
        await ctx.ShopBrands.Where(b => b.Id == brandId).ExecuteDeleteAsync();
    }

    public async Task UpdatePhotoOrderAsync(List<(int PhotoId, int DisplayOrder)> updates)
    {
        await using var ctx = await factory.CreateDbContextAsync();
        foreach (var (photoId, displayOrder) in updates)
            await ctx.ShopPhotos.Where(p => p.Id == photoId)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.DisplayOrder, displayOrder));
    }

    public async Task IncrementViewCountAsync(int shopId)
    {
        await using var ctx = await factory.CreateDbContextAsync();
        await ctx.Shops.Where(s => s.Id == shopId)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.ViewCount, p => p.ViewCount + 1));
    }

    public async Task RecalcRatingAsync(int shopId)
    {
        await using var ctx = await factory.CreateDbContextAsync();
        var stats = await ctx.Reviews
            .Where(r => r.ShopId == shopId)
            .GroupBy(r => 1)
            .Select(g => new { Count = g.Count(), Avg = g.Average(r => (double)r.Rating) })
            .FirstOrDefaultAsync();
        var count = stats?.Count ?? 0;
        var avg = count > 0 ? Math.Round(stats!.Avg, 2) : 0.0;
        var now = DateTime.UtcNow;
        await ctx.Shops.Where(s => s.Id == shopId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(p => p.ReviewCount, count)
                .SetProperty(p => p.AverageRating, avg)
                .SetProperty(p => p.UpdatedAt, now));
        logger.LogDebug("Rating recalculated: shop={ShopId} count={Count} avg={Avg}", shopId, count, avg);
    }
}
