using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PitStop.Domain.Entities;

namespace PitStop.Infrastructure.Data.Configurations;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.Property(r => r.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(r => r.UserName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.UserInitials)
            .IsRequired()
            .HasMaxLength(5);

        builder.Property(r => r.Text)
            .IsRequired()
            .HasMaxLength(2000);

        // One review per user per shop
        builder.HasIndex(r => new { r.ShopId, r.UserId })
            .IsUnique();
    }
}