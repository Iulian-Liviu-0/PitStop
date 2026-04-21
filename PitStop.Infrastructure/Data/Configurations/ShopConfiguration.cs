using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PitStop.Domain.Entities;

namespace PitStop.Infrastructure.Data.Configurations;

public class ShopConfiguration : IEntityTypeConfiguration<Shop>
{
    public void Configure(EntityTypeBuilder<Shop> builder)
    {
        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.Description)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(s => s.Address)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(s => s.City)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.County)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.Sector)
            .HasMaxLength(50);

        builder.Property(s => s.Phone)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(s => s.Email)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.Website)
            .HasMaxLength(300);

        builder.Property(s => s.CoverImage)
            .HasMaxLength(500);

        // OwnerId is a string FK to ApplicationUser (defined in Infrastructure)
        builder.Property(s => s.OwnerId)
            .HasMaxLength(450);

        builder.HasIndex(s => s.City);
        builder.HasIndex(s => s.Category);
    }
}
