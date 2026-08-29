using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Logistics;

namespace ShilpoHubBD.Data.Configurations;

public class LogisticsPartnerProfileConfiguration : IEntityTypeConfiguration<LogisticsPartnerProfile>
{
    public void Configure(EntityTypeBuilder<LogisticsPartnerProfile> builder)
    {
        builder.ToTable("LogisticsPartnerProfiles");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.CompanyName).IsRequired().HasMaxLength(200);
        builder.Property(p => p.LegalName).HasMaxLength(200);
        builder.Property(p => p.RegistrationNumber).HasMaxLength(100);
        builder.Property(p => p.ContactPersonName).IsRequired().HasMaxLength(160);
        builder.Property(p => p.ContactPhone).IsRequired().HasMaxLength(40);
        builder.Property(p => p.ContactEmail).IsRequired().HasMaxLength(200);
        builder.Property(p => p.BaseAddressLine).IsRequired().HasMaxLength(400);
        builder.Property(p => p.BaseCity).IsRequired().HasMaxLength(120);
        builder.Property(p => p.BasePostalCode).HasMaxLength(20);
        builder.Property(p => p.Country).IsRequired().HasMaxLength(80);
        builder.Property(p => p.MaxVehicleCapacityKg).HasColumnType("numeric(12,2)");
        builder.Property(p => p.VerificationStatus).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.VerificationNotes).HasMaxLength(2000);
        builder.Property(p => p.Notes).HasMaxLength(2000);
        builder.Property(p => p.CreatedAt).IsRequired();
        builder.Property(p => p.UpdatedAt).IsRequired();

        builder.HasIndex(p => p.UserId).IsUnique();
        builder.HasIndex(p => p.VerificationStatus);

        builder.HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.BaseDistrict)
            .WithMany()
            .HasForeignKey(p => p.BaseDistrictId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(p => p.VerifiedBy)
            .WithMany()
            .HasForeignKey(p => p.VerifiedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(p => p.ServiceAreas)
            .WithOne(a => a.Profile)
            .HasForeignKey(a => a.LogisticsPartnerProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class LogisticsServiceAreaConfiguration : IEntityTypeConfiguration<LogisticsServiceArea>
{
    public void Configure(EntityTypeBuilder<LogisticsServiceArea> builder)
    {
        builder.ToTable("LogisticsServiceAreas");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.DistrictName).IsRequired().HasMaxLength(120);
        builder.Property(a => a.Division).IsRequired().HasMaxLength(120);
        builder.Property(a => a.SurchargeAmount).HasColumnType("numeric(12,2)");

        builder.HasIndex(a => new { a.LogisticsPartnerProfileId, a.DistrictId }).IsUnique();
        builder.HasIndex(a => a.DistrictId);

        builder.HasOne(a => a.District)
            .WithMany()
            .HasForeignKey(a => a.DistrictId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class PickupRequestConfiguration : IEntityTypeConfiguration<PickupRequest>
{
    public void Configure(EntityTypeBuilder<PickupRequest> builder)
    {
        builder.ToTable("PickupRequests");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.ReferenceCode).IsRequired().HasMaxLength(40);
        builder.Property(r => r.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(r => r.Priority).IsRequired().HasConversion<string>().HasMaxLength(20);

        builder.Property(r => r.OriginContactName).IsRequired().HasMaxLength(160);
        builder.Property(r => r.OriginPhone).IsRequired().HasMaxLength(40);
        builder.Property(r => r.OriginAddressLine).IsRequired().HasMaxLength(400);
        builder.Property(r => r.OriginCity).IsRequired().HasMaxLength(120);
        builder.Property(r => r.OriginPostalCode).HasMaxLength(20);

        builder.Property(r => r.DestinationContactName).HasMaxLength(160);
        builder.Property(r => r.DestinationPhone).HasMaxLength(40);
        builder.Property(r => r.DestinationAddressLine).HasMaxLength(400);
        builder.Property(r => r.DestinationCity).HasMaxLength(120);

        builder.Property(r => r.TotalWeightKg).HasColumnType("numeric(12,2)");
        builder.Property(r => r.DeclaredValue).HasColumnType("numeric(14,2)");
        builder.Property(r => r.CodAmount).HasColumnType("numeric(14,2)");

        builder.Property(r => r.AssignedDriverName).HasMaxLength(160);
        builder.Property(r => r.AssignedDriverPhone).HasMaxLength(40);
        builder.Property(r => r.AssignedVehicleLabel).HasMaxLength(80);

        builder.Property(r => r.SpecialInstructions).HasMaxLength(2000);
        builder.Property(r => r.CancellationReason).HasMaxLength(1000);
        builder.Property(r => r.FailureReason).HasMaxLength(1000);
        builder.Property(r => r.CreatedAt).IsRequired();
        builder.Property(r => r.UpdatedAt).IsRequired();

        builder.HasIndex(r => r.ReferenceCode).IsUnique();
        builder.HasIndex(r => new { r.LogisticsPartnerProfileId, r.Status });
        builder.HasIndex(r => r.ScheduledPickupAt);
        builder.HasIndex(r => r.OrderId);
        builder.HasIndex(r => r.OriginProducerUserId);

        builder.HasOne(r => r.Profile)
            .WithMany()
            .HasForeignKey(r => r.LogisticsPartnerProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.RequestedBy)
            .WithMany()
            .HasForeignKey(r => r.RequestedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Order)
            .WithMany()
            .HasForeignKey(r => r.OrderId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(r => r.OriginDistrict)
            .WithMany()
            .HasForeignKey(r => r.OriginDistrictId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(r => r.OriginProducer)
            .WithMany()
            .HasForeignKey(r => r.OriginProducerUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(r => r.DestinationDistrict)
            .WithMany()
            .HasForeignKey(r => r.DestinationDistrictId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(r => r.Items)
            .WithOne(i => i.PickupRequest)
            .HasForeignKey(i => i.PickupRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(r => r.Events)
            .WithOne(e => e.PickupRequest)
            .HasForeignKey(e => e.PickupRequestId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class PickupItemConfiguration : IEntityTypeConfiguration<PickupItem>
{
    public void Configure(EntityTypeBuilder<PickupItem> builder)
    {
        builder.ToTable("PickupItems");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Description).IsRequired().HasMaxLength(400);
        builder.Property(i => i.Reference).HasMaxLength(120);
        builder.Property(i => i.WeightKg).HasColumnType("numeric(12,2)");
        builder.Property(i => i.LengthCm).HasColumnType("numeric(10,2)");
        builder.Property(i => i.WidthCm).HasColumnType("numeric(10,2)");
        builder.Property(i => i.HeightCm).HasColumnType("numeric(10,2)");

        builder.HasIndex(i => i.PickupRequestId);
    }
}

public class PickupEventConfiguration : IEntityTypeConfiguration<PickupEvent>
{
    public void Configure(EntityTypeBuilder<PickupEvent> builder)
    {
        builder.ToTable("PickupEvents");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Type).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.FromStatus).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.ToStatus).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.Note).HasMaxLength(2000);
        builder.Property(e => e.CreatedAt).IsRequired();

        builder.HasIndex(e => new { e.PickupRequestId, e.CreatedAt });

        builder.HasOne(e => e.Actor)
            .WithMany()
            .HasForeignKey(e => e.ActorUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
