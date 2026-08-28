using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.LiveShopping;

namespace ShilpoHubBD.Data.Configurations;

public class LiveEventConfiguration : IEntityTypeConfiguration<LiveEvent>
{
    public void Configure(EntityTypeBuilder<LiveEvent> builder)
    {
        builder.ToTable("LiveEvents");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Title).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Description).IsRequired().HasMaxLength(2000);
        builder.Property(e => e.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.ScheduledStartAt).IsRequired();
        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.UpdatedAt).IsRequired();

        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.ProducerId);

        builder.HasOne(e => e.Producer)
            .WithMany()
            .HasForeignKey(e => e.ProducerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Product)
            .WithMany()
            .HasForeignKey(e => e.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Auction)
            .WithMany()
            .HasForeignKey(e => e.AuctionId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(e => e.Comments)
            .WithOne(c => c.LiveEvent)
            .HasForeignKey(c => c.LiveEventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Reactions)
            .WithOne(r => r.LiveEvent)
            .HasForeignKey(r => r.LiveEventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Purchases)
            .WithOne(p => p.LiveEvent)
            .HasForeignKey(p => p.LiveEventId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
