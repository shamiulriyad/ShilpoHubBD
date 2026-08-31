using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Innovation;

namespace ShilpoHubBD.Data.Configurations;

public class HeritageInnovationSubmissionConfiguration : IEntityTypeConfiguration<HeritageInnovationSubmission>
{
    public void Configure(EntityTypeBuilder<HeritageInnovationSubmission> builder)
    {
        builder.ToTable("HeritageInnovationSubmissions");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Title).IsRequired().HasMaxLength(200);
        builder.Property(s => s.Problem).IsRequired().HasMaxLength(6000);
        builder.Property(s => s.Solution).IsRequired().HasMaxLength(6000);
        builder.Property(s => s.ResearchEvidence).HasMaxLength(6000);
        builder.Property(s => s.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(s => s.DecisionNote).HasMaxLength(4000);
        builder.Property(s => s.CreatedAt).IsRequired();
        builder.Property(s => s.UpdatedAt).IsRequired();

        builder.HasIndex(s => s.SubmitterUserId);
        builder.HasIndex(s => s.Status);
        builder.HasIndex(s => s.ResearchProjectId);

        builder.HasOne(s => s.Submitter).WithMany().HasForeignKey(s => s.SubmitterUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(s => s.DecisionBy).WithMany().HasForeignKey(s => s.DecisionByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(s => s.ResearchProject).WithMany().HasForeignKey(s => s.ResearchProjectId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(s => s.Prototype).WithMany().HasForeignKey(s => s.InnovationPrototypeId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(s => s.PreservationStrategy).WithMany().HasForeignKey(s => s.PreservationStrategyId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(s => s.HeritageDataset).WithMany().HasForeignKey(s => s.HeritageDatasetId).OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(s => s.TeamMembers).WithOne(m => m.Submission)
            .HasForeignKey(m => m.HeritageInnovationSubmissionId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(s => s.Reviews).WithOne(r => r.Submission)
            .HasForeignKey(r => r.HeritageInnovationSubmissionId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(s => s.History).WithOne(h => h.Submission)
            .HasForeignKey(h => h.HeritageInnovationSubmissionId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class SubmissionTeamMemberConfiguration : IEntityTypeConfiguration<SubmissionTeamMember>
{
    public void Configure(EntityTypeBuilder<SubmissionTeamMember> builder)
    {
        builder.ToTable("SubmissionTeamMembers");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.RoleOnTeam).HasMaxLength(120);
        builder.Property(m => m.AddedAt).IsRequired();

        builder.HasIndex(m => new { m.HeritageInnovationSubmissionId, m.UserId }).IsUnique();
        builder.HasIndex(m => m.UserId);

        builder.HasOne(m => m.User).WithMany().HasForeignKey(m => m.UserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(m => m.AddedBy).WithMany().HasForeignKey(m => m.AddedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class SubmissionReviewConfiguration : IEntityTypeConfiguration<SubmissionReview>
{
    public void Configure(EntityTypeBuilder<SubmissionReview> builder)
    {
        builder.ToTable("SubmissionReviews");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Decision).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(r => r.Comments).IsRequired().HasMaxLength(6000);
        builder.Property(r => r.CreatedAt).IsRequired();

        builder.HasIndex(r => r.HeritageInnovationSubmissionId);

        builder.HasOne(r => r.Reviewer).WithMany().HasForeignKey(r => r.ReviewerUserId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class SubmissionEventConfiguration : IEntityTypeConfiguration<SubmissionEvent>
{
    public void Configure(EntityTypeBuilder<SubmissionEvent> builder)
    {
        builder.ToTable("SubmissionEvents");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.EventType).IsRequired().HasConversion<string>().HasMaxLength(30);
        builder.Property(e => e.Summary).IsRequired().HasMaxLength(500);
        builder.Property(e => e.CreatedAt).IsRequired();

        builder.HasIndex(e => new { e.HeritageInnovationSubmissionId, e.CreatedAt });

        builder.HasOne(e => e.Actor).WithMany().HasForeignKey(e => e.ActorUserId).OnDelete(DeleteBehavior.Restrict);
    }
}
