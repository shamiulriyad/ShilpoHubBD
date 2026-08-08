using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.LiveShopping;

namespace ShilpoHubBD.Data.Configurations;

public class LiveEventCommentConfiguration : IEntityTypeConfiguration<LiveEventComment>
{
    public void Configure(EntityTypeBuilder<LiveEventComment> builder)
    {
        builder.ToTable("LiveEventComments");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Body).IsRequired().HasMaxLength(1000);
        builder.Property(c => c.CreatedAt).IsRequired();

        builder.HasIndex(c => c.LiveEventId);

        builder.HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
