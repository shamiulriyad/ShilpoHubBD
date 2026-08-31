using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.FieldResearch;

namespace ShilpoHubBD.Data.Configurations;

public class SurveyConfiguration : IEntityTypeConfiguration<Survey>
{
    public void Configure(EntityTypeBuilder<Survey> builder)
    {
        builder.ToTable("Surveys");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Slug).IsRequired().HasMaxLength(160);
        builder.Property(s => s.Title).IsRequired().HasMaxLength(200);
        builder.Property(s => s.Description).IsRequired().HasMaxLength(4000);
        builder.Property(s => s.Objective).HasMaxLength(2000);
        builder.Property(s => s.TargetRegion).HasMaxLength(200);
        builder.Property(s => s.Language).IsRequired().HasMaxLength(10);
        builder.Property(s => s.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(s => s.CreatedAt).IsRequired();
        builder.Property(s => s.UpdatedAt).IsRequired();

        builder.HasIndex(s => s.Slug).IsUnique();
        builder.HasIndex(s => s.OwnerUserId);
        builder.HasIndex(s => s.Status);
        builder.HasIndex(s => s.ResearchProjectId);

        builder.HasOne(s => s.Owner)
            .WithMany()
            .HasForeignKey(s => s.OwnerUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.ResearchProject)
            .WithMany()
            .HasForeignKey(s => s.ResearchProjectId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(s => s.Questions)
            .WithOne(q => q.Survey)
            .HasForeignKey(q => q.SurveyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.FieldAssignments)
            .WithOne(a => a.Survey)
            .HasForeignKey(a => a.SurveyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.Responses)
            .WithOne(r => r.Survey)
            .HasForeignKey(r => r.SurveyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.Evidence)
            .WithOne(e => e.Survey)
            .HasForeignKey(e => e.SurveyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.CollectionEvents)
            .WithOne(e => e.Survey)
            .HasForeignKey(e => e.SurveyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
