using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Governance;

namespace ShilpoHubBD.Data.Configurations;

public class FundingProgramConfiguration : IEntityTypeConfiguration<FundingProgram>
{
    public void Configure(EntityTypeBuilder<FundingProgram> builder)
    {
        builder.ToTable("FundingPrograms");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name).IsRequired().HasMaxLength(160);
        builder.Property(p => p.Slug).IsRequired().HasMaxLength(160);
        builder.Property(p => p.Type).IsRequired().HasConversion<string>().HasMaxLength(30);
        builder.Property(p => p.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.Description).IsRequired().HasMaxLength(4000);
        builder.Property(p => p.EligibilityCriteria).HasMaxLength(4000);
        builder.Property(p => p.Currency).IsRequired().HasMaxLength(8);
        builder.Property(p => p.TotalBudget).HasColumnType("numeric(18,2)");
        builder.Property(p => p.AllocatedAmount).HasColumnType("numeric(18,2)");
        builder.Property(p => p.DisbursedAmount).HasColumnType("numeric(18,2)");
        builder.Property(p => p.MinAmountPerApplicant).HasColumnType("numeric(18,2)");
        builder.Property(p => p.MaxAmountPerApplicant).HasColumnType("numeric(18,2)");
        builder.Property(p => p.InterestRatePercent).HasColumnType("numeric(6,3)");
        builder.Property(p => p.CreatedAt).IsRequired();
        builder.Property(p => p.UpdatedAt).IsRequired();

        builder.HasIndex(p => p.Slug).IsUnique();
        builder.HasIndex(p => p.Type);
        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.ManagedByUserId);

        builder.HasOne(p => p.ManagedBy)
            .WithMany()
            .HasForeignKey(p => p.ManagedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Applications)
            .WithOne(a => a.Program)
            .HasForeignKey(a => a.FundingProgramId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class FundingApplicationConfiguration : IEntityTypeConfiguration<FundingApplication>
{
    public void Configure(EntityTypeBuilder<FundingApplication> builder)
    {
        builder.ToTable("FundingApplications");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.ReferenceCode).IsRequired().HasMaxLength(30);
        builder.Property(a => a.ApplicantType).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(a => a.ApplicantLabel).IsRequired().HasMaxLength(200);
        builder.Property(a => a.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(a => a.RequestedAmount).HasColumnType("numeric(18,2)");
        builder.Property(a => a.ApprovedAmount).HasColumnType("numeric(18,2)");
        builder.Property(a => a.OutstandingBalance).HasColumnType("numeric(18,2)");
        builder.Property(a => a.TotalRepaid).HasColumnType("numeric(18,2)");
        builder.Property(a => a.RepaymentStatus).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(a => a.Purpose).IsRequired().HasMaxLength(4000);
        builder.Property(a => a.Justification).HasMaxLength(4000);
        builder.Property(a => a.ContactName).HasMaxLength(160);
        builder.Property(a => a.ContactPhone).HasMaxLength(40);
        builder.Property(a => a.ContactEmail).HasMaxLength(200);
        builder.Property(a => a.DecisionNotes).HasMaxLength(4000);
        builder.Property(a => a.CreatedAt).IsRequired();
        builder.Property(a => a.UpdatedAt).IsRequired();

        builder.HasIndex(a => a.ReferenceCode).IsUnique();
        builder.HasIndex(a => new { a.FundingProgramId, a.Status });
        builder.HasIndex(a => a.Status);
        builder.HasIndex(a => a.ApplicantUserId);
        builder.HasIndex(a => a.ApplicantVillageId);
        builder.HasIndex(a => a.RepaymentStatus);
        builder.HasIndex(a => a.SubmittedAt);

        builder.HasOne(a => a.ApplicantUser)
            .WithMany()
            .HasForeignKey(a => a.ApplicantUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(a => a.ApplicantVillage)
            .WithMany()
            .HasForeignKey(a => a.ApplicantVillageId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(a => a.DecisionBy)
            .WithMany()
            .HasForeignKey(a => a.DecisionByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(a => a.Reviews)
            .WithOne(r => r.Application)
            .HasForeignKey(r => r.FundingApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.Disbursements)
            .WithOne(d => d.Application)
            .HasForeignKey(d => d.FundingApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.Events)
            .WithOne(e => e.Application)
            .HasForeignKey(e => e.FundingApplicationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class FundingApplicationReviewConfiguration : IEntityTypeConfiguration<FundingApplicationReview>
{
    public void Configure(EntityTypeBuilder<FundingApplicationReview> builder)
    {
        builder.ToTable("FundingApplicationReviews");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Decision).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(r => r.RecommendedAmount).HasColumnType("numeric(18,2)");
        builder.Property(r => r.Comments).HasMaxLength(4000);
        builder.Property(r => r.CreatedAt).IsRequired();

        builder.HasIndex(r => new { r.FundingApplicationId, r.CreatedAt });

        builder.HasOne(r => r.Reviewer)
            .WithMany()
            .HasForeignKey(r => r.ReviewerUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class FundingDisbursementConfiguration : IEntityTypeConfiguration<FundingDisbursement>
{
    public void Configure(EntityTypeBuilder<FundingDisbursement> builder)
    {
        builder.ToTable("FundingDisbursements");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Amount).HasColumnType("numeric(18,2)");
        builder.Property(d => d.Method).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(d => d.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(d => d.Reference).HasMaxLength(120);
        builder.Property(d => d.Notes).HasMaxLength(2000);
        builder.Property(d => d.CreatedAt).IsRequired();
        builder.Property(d => d.UpdatedAt).IsRequired();

        builder.HasIndex(d => new { d.FundingApplicationId, d.ScheduledFor });
        builder.HasIndex(d => d.Status);

        builder.HasOne(d => d.RecordedBy)
            .WithMany()
            .HasForeignKey(d => d.RecordedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class FundingApplicationEventConfiguration : IEntityTypeConfiguration<FundingApplicationEvent>
{
    public void Configure(EntityTypeBuilder<FundingApplicationEvent> builder)
    {
        builder.ToTable("FundingApplicationEvents");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Type).IsRequired().HasConversion<string>().HasMaxLength(30);
        builder.Property(e => e.Note).HasMaxLength(2000);
        builder.Property(e => e.FromStatus).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.ToStatus).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.CreatedAt).IsRequired();

        builder.HasIndex(e => new { e.FundingApplicationId, e.CreatedAt });

        builder.HasOne(e => e.Actor)
            .WithMany()
            .HasForeignKey(e => e.ActorUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
