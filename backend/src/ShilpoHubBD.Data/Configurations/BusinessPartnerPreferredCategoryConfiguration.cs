using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.BusinessPartner;

namespace ShilpoHubBD.Data.Configurations;

public class BusinessPartnerPreferredCategoryConfiguration : IEntityTypeConfiguration<BusinessPartnerPreferredCategory>
{
    public void Configure(EntityTypeBuilder<BusinessPartnerPreferredCategory> builder)
    {
        builder.ToTable("BusinessPartnerPreferredCategories");
        builder.HasKey(c => c.Id);

        builder.HasIndex(c => new { c.BusinessPartnerProfileId, c.CategoryId }).IsUnique();

        builder.HasOne(c => c.Category)
            .WithMany()
            .HasForeignKey(c => c.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
