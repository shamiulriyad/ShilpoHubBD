using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.CSRSponsorship;

namespace ShilpoHubBD.Data.Configurations;

public class SponsorshipProgressUpdateConfiguration : IEntityTypeConfiguration<SponsorshipProgressUpdate>
{
    public void Configure(EntityTypeBuilder<SponsorshipProgressUpdate> builder)
    {
        builder.ToTable("SponsorshipProgressUpdates");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Content).IsRequired().HasMaxLength(2000);
        builder.Property(u => u.CreatedAt).IsRequired();

        builder.HasOne(u => u.Author)
            .WithMany()
            .HasForeignKey(u => u.AuthorUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
