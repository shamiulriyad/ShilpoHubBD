using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.DesignCollaboration;

namespace ShilpoHubBD.Data.Configurations;

public class DesignRevisionConfiguration : IEntityTypeConfiguration<DesignRevision>
{
    public void Configure(EntityTypeBuilder<DesignRevision> builder)
    {
        builder.ToTable("DesignRevisions");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Description).IsRequired().HasMaxLength(2000);
        builder.Property(r => r.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(r => r.DecisionNotes).HasMaxLength(1000);
        builder.Property(r => r.SubmittedAt).IsRequired();

        builder.HasIndex(r => new { r.ProjectId, r.RevisionNumber }).IsUnique();

        builder.HasOne(r => r.SubmittedBy)
            .WithMany()
            .HasForeignKey(r => r.SubmittedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
