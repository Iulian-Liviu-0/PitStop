using System.Reflection;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PitStop.Domain.Entities;
using PitStop.Infrastructure.Identity;

namespace PitStop.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Shop> Shops => Set<Shop>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<ShopPhoto> ShopPhotos => Set<ShopPhoto>();
    public DbSet<ShopService> ShopServices => Set<ShopService>();
    public DbSet<ShopHour> ShopHours => Set<ShopHour>();
    public DbSet<ShopRequest> ShopRequests => Set<ShopRequest>();
    public DbSet<FavoriteShop> FavoriteShops => Set<FavoriteShop>();
    public DbSet<ShopBrand> ShopBrands => Set<ShopBrand>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        if (Database.IsNpgsql())
            modelBuilder.Entity<Review>().ToTable(t =>
                t.HasCheckConstraint("CK_Review_Rating", "\"Rating\" BETWEEN 1 AND 5"));
        else if (Database.IsSqlite())
            modelBuilder.Entity<Review>().ToTable(t =>
                t.HasCheckConstraint("CK_Review_Rating", "Rating BETWEEN 1 AND 5"));
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.UpdatedAt = now;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    break;
            }

        return await base.SaveChangesAsync(cancellationToken);
    }
}