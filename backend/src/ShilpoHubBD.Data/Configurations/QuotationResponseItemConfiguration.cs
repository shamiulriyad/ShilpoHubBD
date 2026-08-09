using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Quotations;

namespace ShilpoHubBD.Data.Configurations;

public class QuotationResponseItemConfiguration : IEntityTypeConfiguration<QuotationResponseItem>
{
    public void Configure(EntityTypeBuilder<QuotationResponseItem> builder)
    {
        builder.ToTable("QuotationResponseItems");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.QuotedUnitPrice).HasColumnType("numeric(18,2)").IsRequired();
        builder.Property(i => i.Notes).HasMaxLength(1000);

        builder.HasOne(i => i.QuotationRequestItem)
            .WithMany(ri => ri.ResponseItems)
            .HasForeignKey(i => i.QuotationRequestItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
