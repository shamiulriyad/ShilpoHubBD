using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.LiveClass;

namespace ShilpoHubBD.Data.Configurations;

public class LiveClassParticipantConfiguration : IEntityTypeConfiguration<LiveClassParticipant>
{
    public void Configure(EntityTypeBuilder<LiveClassParticipant> builder)
    {
        builder.ToTable("LiveClassParticipants");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.RegisteredAt).IsRequired();

        builder.HasIndex(p => new { p.LiveClassId, p.UserId }).IsUnique();

        builder.HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
