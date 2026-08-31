using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Research;

namespace ShilpoHubBD.Data.Configurations;

public class ResearchAIAnalysisConfiguration : IEntityTypeConfiguration<ResearchAIAnalysis>
{
    public void Configure(EntityTypeBuilder<ResearchAIAnalysis> builder)
    {
        builder.ToTable("ResearchAIAnalyses");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.AnalysisType).IsRequired().HasConversion<string>().HasMaxLength(30);
        builder.Property(a => a.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(a => a.ProviderName).IsRequired().HasMaxLength(80);
        builder.Property(a => a.Title).IsRequired().HasMaxLength(200);
        builder.Property(a => a.ResearchQuestions).IsRequired().HasMaxLength(4000);
        builder.Property(a => a.InputSummary).IsRequired().HasMaxLength(4000);
        builder.Property(a => a.ContextJson).HasMaxLength(32000);
        builder.Property(a => a.ResultSummary).IsRequired().HasMaxLength(8000);
        builder.Property(a => a.ResultJson).HasMaxLength(64000);
        builder.Property(a => a.ErrorMessage).HasMaxLength(2000);
        builder.Property(a => a.CreatedAt).IsRequired();

        builder.HasIndex(a => new { a.ResearchProjectId, a.CreatedAt });
        builder.HasIndex(a => a.AnalysisType);
        builder.HasIndex(a => a.RequestedByUserId);

        builder.HasOne(a => a.Project)
            .WithMany()
            .HasForeignKey(a => a.ResearchProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.RequestedBy)
            .WithMany()
            .HasForeignKey(a => a.RequestedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Dataset)
            .WithMany()
            .HasForeignKey(a => a.DatasetId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(a => a.Paper)
            .WithMany()
            .HasForeignKey(a => a.ResearchPaperId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(a => a.Findings)
            .WithOne(f => f.Analysis)
            .HasForeignKey(f => f.ResearchAIAnalysisId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.Citations)
            .WithOne(c => c.Analysis)
            .HasForeignKey(c => c.ResearchAIAnalysisId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
