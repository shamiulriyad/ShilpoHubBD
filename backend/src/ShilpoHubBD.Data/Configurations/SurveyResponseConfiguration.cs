using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.FieldResearch;

namespace ShilpoHubBD.Data.Configurations;

public class SurveyResponseConfiguration : IEntityTypeConfiguration<SurveyResponse>
{
    public void Configure(EntityTypeBuilder<SurveyResponse> builder)
    {
        builder.ToTable("SurveyResponses");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.RespondentName).HasMaxLength(200);
        builder.Property(r => r.RespondentContact).HasMaxLength(200);
        builder.Property(r => r.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(r => r.Source).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(r => r.VillageName).HasMaxLength(200);
        builder.Property(r => r.DistrictName).HasMaxLength(200);
        builder.Property(r => r.ReviewNote).HasMaxLength(2000);
        builder.Property(r => r.CollectedAt).IsRequired();
        builder.Property(r => r.CreatedAt).IsRequired();
        builder.Property(r => r.UpdatedAt).IsRequired();

        builder.HasIndex(r => new { r.SurveyId, r.Status });
        builder.HasIndex(r => r.SubmittedByUserId);
        builder.HasIndex(r => r.CollectedAt);

        builder.HasOne(r => r.SubmittedBy)
            .WithMany()
            .HasForeignKey(r => r.SubmittedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.ReviewedBy)
            .WithMany()
            .HasForeignKey(r => r.ReviewedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(r => r.Answers)
            .WithOne(a => a.Response)
            .HasForeignKey(a => a.SurveyResponseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(r => r.Evidence)
            .WithOne(e => e.Response)
            .HasForeignKey(e => e.SurveyResponseId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
