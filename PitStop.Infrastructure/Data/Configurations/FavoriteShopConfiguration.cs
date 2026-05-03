using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PitStop.Domain.Entities;

namespace PitStop.Infrastructure.Data.Configurations;

public class FavoriteShopConfiguration : IEntityTypeConfiguration<FavoriteShop>
{
    public void Configure(EntityTypeBuilder<FavoriteShop> builder)
    {
        builder.Property(f => f.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.HasIndex(f => new { f.UserId, f.ShopId })
            .IsUnique();
    }
}