using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Research;

namespace ShilpoHubBD.Data.Configurations;

public class ResearchProjectMemberConfiguration : IEntityTypeConfiguration<ResearchProjectMember>
{
    public void Configure(EntityTypeBuilder<ResearchProjectMember> builder)
    {
        builder.ToTable("ResearchProjectMembers");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Role).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(m => m.JoinedAt).IsRequired();

        builder.HasIndex(m => new { m.ResearchProjectId, m.UserId }).IsUnique();
        builder.HasIndex(m => m.UserId);

        builder.HasOne(m => m.User)
            .WithMany()
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.InvitedBy)
            .WithMany()
            .HasForeignKey(m => m.InvitedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
