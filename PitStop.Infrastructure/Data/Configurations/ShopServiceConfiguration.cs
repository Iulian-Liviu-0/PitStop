using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PitStop.Domain.Entities;

namespace PitStop.Infrastructure.Data.Configurations;

public class ShopServiceConfiguration : IEntityTypeConfiguration<ShopService>
{
    public void Configure(EntityTypeBuilder<ShopService> builder)
    {
        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(s => s.PriceMin)
            .HasPrecision(10, 2);

        builder.Property(s => s.PriceMax)
            .HasPrecision(10, 2);
    }
}