using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PitStop.Infrastructure.Data;
using PitStop.Infrastructure.Identity;

namespace PitStop.Web.Seeding;

internal static class DataCleaner
{
    /// <summary>
    ///     Deletes all shop data, reviews, requests, and non-admin users.
    ///     Roles and the admin account are preserved.
    ///     Trigger by setting "ResetForRelease": true in appsettings before one startup.
    /// </summary>
    internal static async Task ClearAllAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // Delete child tables before parent to avoid FK violations.
        await db.ShopBrands.ExecuteDeleteAsync();
        await db.ShopHours.ExecuteDeleteAsync();
        await db.ShopServices.ExecuteDeleteAsync();
        await db.ShopPhotos.ExecuteDeleteAsync();
        await db.FavoriteShops.ExecuteDeleteAsync();
        await db.Reviews.ExecuteDeleteAsync();
        await db.ShopRequests.ExecuteDeleteAsync();
        await db.Shops.ExecuteDeleteAsync();

        // Remove all non-admin users via UserManager so Identity cascade tables are cleaned up.
        var toDelete = new List<ApplicationUser>();
        foreach (var user in await userManager.Users.ToListAsync())
        {
            var roles = await userManager.GetRolesAsync(user);
            if (!roles.Contains("Admin"))
                toDelete.Add(user);
        }

        foreach (var user in toDelete)
            await userManager.DeleteAsync(user);
    }
}