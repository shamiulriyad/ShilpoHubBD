using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.FieldResearch;

namespace ShilpoHubBD.Data.Configurations;

public class FieldEvidenceConfiguration : IEntityTypeConfiguration<FieldEvidence>
{
    public void Configure(EntityTypeBuilder<FieldEvidence> builder)
    {
        builder.ToTable("FieldEvidence");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.EvidenceType).IsRequired().HasConversion<string>().HasMaxLength(30);
        builder.Property(e => e.Title).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Description).HasMaxLength(2000);
        builder.Property(e => e.FileUrl).HasMaxLength(2048);
        builder.Property(e => e.FileName).HasMaxLength(400);
        builder.Property(e => e.MimeType).HasMaxLength(150);
        builder.Property(e => e.TranscriptText).HasMaxLength(32000);
        builder.Property(e => e.Language).HasMaxLength(20);
        builder.Property(e => e.CapturedAt).IsRequired();
        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.UpdatedAt).IsRequired();

        builder.HasIndex(e => new { e.SurveyId, e.EvidenceType });
        builder.HasIndex(e => e.SurveyResponseId);
        builder.HasIndex(e => e.CapturedByUserId);
        builder.HasIndex(e => e.CapturedAt);

        builder.HasOne(e => e.CapturedBy)
            .WithMany()
            .HasForeignKey(e => e.CapturedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
