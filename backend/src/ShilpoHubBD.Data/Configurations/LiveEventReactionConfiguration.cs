using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.LiveShopping;

namespace ShilpoHubBD.Data.Configurations;

public class LiveEventReactionConfiguration : IEntityTypeConfiguration<LiveEventReaction>
{
    public void Configure(EntityTypeBuilder<LiveEventReaction> builder)
    {
        builder.ToTable("LiveEventReactions");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Type).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(r => r.CreatedAt).IsRequired();

        builder.HasIndex(r => r.LiveEventId);

        builder.HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
