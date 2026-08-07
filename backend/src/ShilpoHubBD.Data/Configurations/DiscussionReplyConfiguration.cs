using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Community;

namespace ShilpoHubBD.Data.Configurations;

public class DiscussionReplyConfiguration : IEntityTypeConfiguration<DiscussionReply>
{
    public void Configure(EntityTypeBuilder<DiscussionReply> builder)
    {
        builder.ToTable("DiscussionReplies");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Body).IsRequired().HasMaxLength(2000);
        builder.Property(r => r.CreatedAt).IsRequired();

        builder.HasIndex(r => r.ThreadId);

        builder.HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
