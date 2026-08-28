using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Governance;

namespace ShilpoHubBD.Data.Configurations;

public class MonitoringFlagConfiguration : IEntityTypeConfiguration<MonitoringFlag>
{
    public void Configure(EntityTypeBuilder<MonitoringFlag> builder)
    {
        builder.ToTable("MonitoringFlags");
        builder.HasKey(f => f.Id);

        builder.Property(f => f.FlagType).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(f => f.Severity).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(f => f.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(f => f.Source).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(f => f.SubjectType).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(f => f.SubjectLabel).IsRequired().HasMaxLength(200);
        builder.Property(f => f.Title).IsRequired().HasMaxLength(200);
        builder.Property(f => f.Description).IsRequired().HasMaxLength(2000);
        builder.Property(f => f.EvidenceJson).HasColumnType("text");
        builder.Property(f => f.RiskScore).HasColumnType("numeric(6,2)");
        builder.Property(f => f.DedupeKey).IsRequired().HasMaxLength(200);
        builder.Property(f => f.ResolutionNotes).HasMaxLength(2000);
        builder.Property(f => f.CreatedAt).IsRequired();
        builder.Property(f => f.UpdatedAt).IsRequired();

        builder.HasIndex(f => f.FlagType);
        builder.HasIndex(f => f.Status);
        builder.HasIndex(f => f.Severity);
        builder.HasIndex(f => new { f.SubjectType, f.SubjectId });
        builder.HasIndex(f => f.AssignedToUserId);
        builder.HasIndex(f => f.DedupeKey);
        builder.HasIndex(f => f.DetectedAt);

        builder.HasOne(f => f.CreatedBy)
            .WithMany()
            .HasForeignKey(f => f.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(f => f.AssignedTo)
            .WithMany()
            .HasForeignKey(f => f.AssignedToUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(f => f.ResolvedBy)
            .WithMany()
            .HasForeignKey(f => f.ResolvedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(f => f.Events)
            .WithOne(e => e.Flag)
            .HasForeignKey(e => e.MonitoringFlagId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class MonitoringFlagEventConfiguration : IEntityTypeConfiguration<MonitoringFlagEvent>
{
    public void Configure(EntityTypeBuilder<MonitoringFlagEvent> builder)
    {
        builder.ToTable("MonitoringFlagEvents");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Type).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.Note).HasMaxLength(2000);
        builder.Property(e => e.FromStatus).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.ToStatus).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.CreatedAt).IsRequired();

        builder.HasIndex(e => new { e.MonitoringFlagId, e.CreatedAt });

        builder.HasOne(e => e.Actor)
            .WithMany()
            .HasForeignKey(e => e.ActorUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ComplaintConfiguration : IEntityTypeConfiguration<Complaint>
{
    public void Configure(EntityTypeBuilder<Complaint> builder)
    {
        builder.ToTable("Complaints");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.ReferenceCode).IsRequired().HasMaxLength(30);
        builder.Property(c => c.Category).IsRequired().HasConversion<string>().HasMaxLength(30);
        builder.Property(c => c.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(c => c.Priority).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(c => c.AgainstType).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(c => c.Title).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Description).IsRequired().HasMaxLength(4000);
        builder.Property(c => c.ComplainantName).HasMaxLength(160);
        builder.Property(c => c.ComplainantContact).HasMaxLength(200);
        builder.Property(c => c.AgainstLabel).HasMaxLength(200);
        builder.Property(c => c.Resolution).HasMaxLength(4000);
        builder.Property(c => c.CreatedAt).IsRequired();
        builder.Property(c => c.UpdatedAt).IsRequired();

        builder.HasIndex(c => c.ReferenceCode).IsUnique();
        builder.HasIndex(c => c.Status);
        builder.HasIndex(c => c.Category);
        builder.HasIndex(c => c.Priority);
        builder.HasIndex(c => c.AssignedToUserId);
        builder.HasIndex(c => c.ComplainantUserId);
        builder.HasIndex(c => c.CreatedAt);

        builder.HasOne(c => c.ComplainantUser)
            .WithMany()
            .HasForeignKey(c => c.ComplainantUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(c => c.AssignedTo)
            .WithMany()
            .HasForeignKey(c => c.AssignedToUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(c => c.ResolvedBy)
            .WithMany()
            .HasForeignKey(c => c.ResolvedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(c => c.RelatedOrder)
            .WithMany()
            .HasForeignKey(c => c.RelatedOrderId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(c => c.MonitoringFlag)
            .WithMany()
            .HasForeignKey(c => c.MonitoringFlagId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(c => c.Updates)
            .WithOne(u => u.Complaint)
            .HasForeignKey(u => u.ComplaintId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ComplaintUpdateConfiguration : IEntityTypeConfiguration<ComplaintUpdate>
{
    public void Configure(EntityTypeBuilder<ComplaintUpdate> builder)
    {
        builder.ToTable("ComplaintUpdates");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Message).IsRequired().HasMaxLength(4000);
        builder.Property(u => u.FromStatus).HasConversion<string>().HasMaxLength(20);
        builder.Property(u => u.ToStatus).HasConversion<string>().HasMaxLength(20);
        builder.Property(u => u.CreatedAt).IsRequired();

        builder.HasIndex(u => new { u.ComplaintId, u.CreatedAt });

        builder.HasOne(u => u.Actor)
            .WithMany()
            .HasForeignKey(u => u.ActorUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ComplianceRecordConfiguration : IEntityTypeConfiguration<ComplianceRecord>
{
    public void Configure(EntityTypeBuilder<ComplianceRecord> builder)
    {
        builder.ToTable("ComplianceRecords");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.EntityType).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(r => r.EntityLabel).IsRequired().HasMaxLength(200);
        builder.Property(r => r.Framework).IsRequired().HasMaxLength(200);
        builder.Property(r => r.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(r => r.OverallScorePercent).HasColumnType("numeric(6,2)");
        builder.Property(r => r.Notes).HasMaxLength(4000);
        builder.Property(r => r.PeriodStart).IsRequired();
        builder.Property(r => r.CreatedAt).IsRequired();
        builder.Property(r => r.UpdatedAt).IsRequired();

        builder.HasIndex(r => new { r.EntityType, r.EntityId });
        builder.HasIndex(r => r.Status);
        builder.HasIndex(r => r.NextReviewDue);
        builder.HasIndex(r => r.CreatedByUserId);

        builder.HasOne(r => r.CreatedBy)
            .WithMany()
            .HasForeignKey(r => r.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Reviewer)
            .WithMany()
            .HasForeignKey(r => r.ReviewerUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(r => r.Requirements)
            .WithOne(x => x.Record)
            .HasForeignKey(x => x.ComplianceRecordId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ComplianceRequirementConfiguration : IEntityTypeConfiguration<ComplianceRequirement>
{
    public void Configure(EntityTypeBuilder<ComplianceRequirement> builder)
    {
        builder.ToTable("ComplianceRequirements");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code).IsRequired().HasMaxLength(60);
        builder.Property(x => x.Title).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.Evidence).HasMaxLength(1000);

        builder.HasIndex(x => new { x.ComplianceRecordId, x.DisplayOrder });
    }
}
