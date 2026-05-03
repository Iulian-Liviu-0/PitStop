using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PitStop.Domain.Entities;

namespace PitStop.Infrastructure.Data.Configurations;

public class ShopRequestConfiguration : IEntityTypeConfiguration<ShopRequest>
{
    public void Configure(EntityTypeBuilder<ShopRequest> builder)
    {
        builder.Property(r => r.ShopName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.City)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(r => r.County)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(r => r.ContactPerson)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.Phone)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(r => r.Email)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.Description)
            .HasMaxLength(1000);

        builder.Property(r => r.AdminNote)
            .HasMaxLength(1000);
    }
}