using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.BusinessPartner;

namespace ShilpoHubBD.Data.Configurations;

public class BusinessPartnerProfileConfiguration : IEntityTypeConfiguration<BusinessPartnerProfile>
{
    public void Configure(EntityTypeBuilder<BusinessPartnerProfile> builder)
    {
        builder.ToTable("BusinessPartnerProfiles");
        builder.HasKey(b => b.Id);

        builder.Property(b => b.BusinessType).HasConversion<string>().HasMaxLength(30);
        builder.Property(b => b.CompanyName).IsRequired().HasMaxLength(200);
        builder.Property(b => b.RegistrationNumber).IsRequired().HasMaxLength(100);
        builder.Property(b => b.TaxIdentificationNumber).HasMaxLength(100);
        builder.Property(b => b.Industry).IsRequired().HasMaxLength(150);
        builder.Property(b => b.BusinessSize).HasConversion<string>().HasMaxLength(20);
        builder.Property(b => b.Website).HasMaxLength(300);
        builder.Property(b => b.CompanyDescription).IsRequired().HasMaxLength(2000);

        builder.Property(b => b.AddressLine).IsRequired().HasMaxLength(300);
        builder.Property(b => b.City).IsRequired().HasMaxLength(100);
        builder.Property(b => b.PostalCode).HasMaxLength(20);
        builder.Property(b => b.Country).IsRequired().HasMaxLength(100);

        builder.Property(b => b.ContactPersonName).IsRequired().HasMaxLength(150);
        builder.Property(b => b.ContactPersonDesignation).HasMaxLength(150);
        builder.Property(b => b.ContactPhone).IsRequired().HasMaxLength(30);
        builder.Property(b => b.ContactEmail).IsRequired().HasMaxLength(200);

        builder.Property(b => b.MaxBudgetPerOrder).HasColumnType("numeric(18,2)");
        builder.Property(b => b.PreferredOrderFrequency).HasConversion<string>().HasMaxLength(20);
        builder.Property(b => b.PreferredPaymentTerms).HasMaxLength(200);

        builder.Property(b => b.VerificationStatus).HasConversion<string>().HasMaxLength(20);
        builder.Property(b => b.VerificationNotes).HasMaxLength(1000);

        builder.Property(b => b.CreatedAt).IsRequired();
        builder.Property(b => b.UpdatedAt).IsRequired();

        builder.HasIndex(b => b.UserId).IsUnique();

        builder.HasOne(b => b.User)
            .WithMany()
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.VerifiedBy)
            .WithMany()
            .HasForeignKey(b => b.VerifiedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.District)
            .WithMany()
            .HasForeignKey(b => b.DistrictId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(b => b.Documents)
            .WithOne(d => d.BusinessPartnerProfile)
            .HasForeignKey(d => d.BusinessPartnerProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(b => b.PreferredCategories)
            .WithOne(c => c.BusinessPartnerProfile)
            .HasForeignKey(c => c.BusinessPartnerProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
