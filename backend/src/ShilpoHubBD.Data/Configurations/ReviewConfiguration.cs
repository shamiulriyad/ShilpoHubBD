using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Reviews;

namespace ShilpoHubBD.Data.Configurations;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable("Reviews");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Rating).IsRequired();
        builder.Property(r => r.Comment).IsRequired().HasMaxLength(2000);
        builder.Property(r => r.CreatedAt).IsRequired();
        builder.Property(r => r.UpdatedAt).IsRequired();

        builder.HasIndex(r => new { r.ProductId, r.UserId }).IsUnique().HasFilter("\"ProductId\" IS NOT NULL");
        builder.HasIndex(r => new { r.HeritagePlaceId, r.UserId }).IsUnique().HasFilter("\"HeritagePlaceId\" IS NOT NULL");
        builder.HasIndex(r => new { r.BookingId, r.UserId }).IsUnique().HasFilter("\"BookingId\" IS NOT NULL");

        builder.HasOne(r => r.Product)
            .WithMany()
            .HasForeignKey(r => r.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.HeritagePlace)
            .WithMany()
            .HasForeignKey(r => r.HeritagePlaceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Booking)
            .WithMany()
            .HasForeignKey(r => r.BookingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(r => r.Images)
            .WithOne(i => i.Review)
            .HasForeignKey(i => i.ReviewId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
