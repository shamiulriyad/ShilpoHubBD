using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Community;

namespace ShilpoHubBD.Data.Configurations;

public class ProducerFollowConfiguration : IEntityTypeConfiguration<ProducerFollow>
{
    public void Configure(EntityTypeBuilder<ProducerFollow> builder)
    {
        builder.ToTable("ProducerFollows");
        builder.HasKey(f => f.Id);

        builder.Property(f => f.CreatedAt).IsRequired();

        builder.HasIndex(f => new { f.FollowerId, f.ProducerId }).IsUnique();

        builder.HasOne(f => f.Follower)
            .WithMany()
            .HasForeignKey(f => f.FollowerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(f => f.Producer)
            .WithMany()
            .HasForeignKey(f => f.ProducerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
