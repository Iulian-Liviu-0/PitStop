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

        // PostgreSQL: column name is quoted to match EF Core's default PascalCase naming
        builder.ToTable(t =>
            t.HasCheckConstraint("CK_Review_Rating", "\"Rating\" BETWEEN 1 AND 5"));

        // One review per user per shop
        builder.HasIndex(r => new { r.ShopId, r.UserId })
            .IsUnique();
    }
}
