using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Innovation;

namespace ShilpoHubBD.Data.Configurations;

public class InnovationPrototypeConfiguration : IEntityTypeConfiguration<InnovationPrototype>
{
    public void Configure(EntityTypeBuilder<InnovationPrototype> builder)
    {
        builder.ToTable("InnovationPrototypes");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Description).IsRequired().HasMaxLength(6000);
        builder.Property(p => p.Category).HasMaxLength(100);
        builder.Property(p => p.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.CreatedAt).IsRequired();
        builder.Property(p => p.UpdatedAt).IsRequired();

        builder.HasIndex(p => p.OwnerUserId);
        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.ResearchProjectId);

        builder.HasOne(p => p.Owner).WithMany().HasForeignKey(p => p.OwnerUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(p => p.ResearchProject).WithMany().HasForeignKey(p => p.ResearchProjectId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(p => p.PreservationStrategy).WithMany().HasForeignKey(p => p.PreservationStrategyId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(p => p.InnovationExperiment).WithMany().HasForeignKey(p => p.InnovationExperimentId).OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(p => p.Iterations).WithOne(i => i.Prototype)
            .HasForeignKey(i => i.InnovationPrototypeId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(p => p.TestCases).WithOne(c => c.Prototype)
            .HasForeignKey(c => c.InnovationPrototypeId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(p => p.TestRuns).WithOne(r => r.Prototype)
            .HasForeignKey(r => r.InnovationPrototypeId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(p => p.Issues).WithOne(x => x.Prototype)
            .HasForeignKey(x => x.InnovationPrototypeId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(p => p.CurrentIteration).WithMany()
            .HasForeignKey(p => p.CurrentIterationId).OnDelete(DeleteBehavior.NoAction);
    }
}

public class PrototypeIterationConfiguration : IEntityTypeConfiguration<PrototypeIteration>
{
    public void Configure(EntityTypeBuilder<PrototypeIteration> builder)
    {
        builder.ToTable("PrototypeIterations");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Label).HasMaxLength(50);
        builder.Property(i => i.ChangeSummary).IsRequired().HasMaxLength(4000);
        builder.Property(i => i.ArtifactUrl).HasMaxLength(2048);
        builder.Property(i => i.CreatedAt).IsRequired();

        builder.HasIndex(i => new { i.InnovationPrototypeId, i.VersionNumber }).IsUnique();

        builder.HasOne(i => i.CreatedBy).WithMany().HasForeignKey(i => i.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class PrototypeTestCaseConfiguration : IEntityTypeConfiguration<PrototypeTestCase>
{
    public void Configure(EntityTypeBuilder<PrototypeTestCase> builder)
    {
        builder.ToTable("PrototypeTestCases");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Title).IsRequired().HasMaxLength(300);
        builder.Property(c => c.Description).HasMaxLength(4000);
        builder.Property(c => c.Steps).HasMaxLength(6000);
        builder.Property(c => c.ExpectedResult).IsRequired().HasMaxLength(4000);
        builder.Property(c => c.Priority).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(c => c.CreatedAt).IsRequired();
        builder.Property(c => c.UpdatedAt).IsRequired();

        builder.HasIndex(c => new { c.InnovationPrototypeId, c.OrderIndex });

        builder.HasMany(c => c.Results).WithOne(r => r.TestCase)
            .HasForeignKey(r => r.PrototypeTestCaseId).OnDelete(DeleteBehavior.SetNull);
    }
}

public class PrototypeTestRunConfiguration : IEntityTypeConfiguration<PrototypeTestRun>
{
    public void Configure(EntityTypeBuilder<PrototypeTestRun> builder)
    {
        builder.ToTable("PrototypeTestRuns");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Title).IsRequired().HasMaxLength(200);
        builder.Property(r => r.Summary).HasMaxLength(6000);
        builder.Property(r => r.Environment).HasMaxLength(300);
        builder.Property(r => r.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(r => r.CreatedAt).IsRequired();
        builder.Property(r => r.UpdatedAt).IsRequired();

        builder.HasIndex(r => new { r.InnovationPrototypeId, r.RunNumber }).IsUnique();
        builder.HasIndex(r => r.Status);

        builder.HasOne(r => r.Iteration).WithMany()
            .HasForeignKey(r => r.PrototypeIterationId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(r => r.ExecutedBy).WithMany()
            .HasForeignKey(r => r.ExecutedByUserId).OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(r => r.Results).WithOne(x => x.TestRun)
            .HasForeignKey(x => x.PrototypeTestRunId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class PrototypeTestResultConfiguration : IEntityTypeConfiguration<PrototypeTestResult>
{
    public void Configure(EntityTypeBuilder<PrototypeTestResult> builder)
    {
        builder.ToTable("PrototypeTestResults");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.CaseTitle).IsRequired().HasMaxLength(300);
        builder.Property(x => x.Outcome).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.ActualResult).HasMaxLength(4000);
        builder.Property(x => x.Notes).HasMaxLength(2000);

        builder.HasIndex(x => x.PrototypeTestRunId);
        builder.HasIndex(x => x.PrototypeTestCaseId);
    }
}

public class PrototypeIssueConfiguration : IEntityTypeConfiguration<PrototypeIssue>
{
    public void Configure(EntityTypeBuilder<PrototypeIssue> builder)
    {
        builder.ToTable("PrototypeIssues");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title).IsRequired().HasMaxLength(300);
        builder.Property(x => x.Description).IsRequired().HasMaxLength(6000);
        builder.Property(x => x.Severity).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.Resolution).HasMaxLength(4000);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();

        builder.HasIndex(x => new { x.InnovationPrototypeId, x.Status });
        builder.HasIndex(x => x.Severity);

        builder.HasOne(x => x.TestRun).WithMany()
            .HasForeignKey(x => x.PrototypeTestRunId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(x => x.ReportedBy).WithMany()
            .HasForeignKey(x => x.ReportedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.ResolvedBy).WithMany()
            .HasForeignKey(x => x.ResolvedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}
