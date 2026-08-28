using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.FieldResearch;

namespace ShilpoHubBD.Data.Configurations;

public class SurveyFieldAssignmentConfiguration : IEntityTypeConfiguration<SurveyFieldAssignment>
{
    public void Configure(EntityTypeBuilder<SurveyFieldAssignment> builder)
    {
        builder.ToTable("SurveyFieldAssignments");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Role).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(a => a.AreaNote).HasMaxLength(500);
        builder.Property(a => a.AssignedAt).IsRequired();

        builder.HasIndex(a => new { a.SurveyId, a.FieldResearcherUserId }).IsUnique();
        builder.HasIndex(a => a.FieldResearcherUserId);

        builder.HasOne(a => a.FieldResearcher)
            .WithMany()
            .HasForeignKey(a => a.FieldResearcherUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.AssignedBy)
            .WithMany()
            .HasForeignKey(a => a.AssignedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
