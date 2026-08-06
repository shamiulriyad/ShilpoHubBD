using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Constants;
using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Data.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public static readonly Guid CustomerId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    public static readonly Guid ProducerId = Guid.Parse("00000000-0000-0000-0000-000000000002");
    public static readonly Guid BusinessPartnerId = Guid.Parse("00000000-0000-0000-0000-000000000003");
    public static readonly Guid TouristId = Guid.Parse("00000000-0000-0000-0000-000000000004");
    public static readonly Guid HeritageAcademyMemberId = Guid.Parse("00000000-0000-0000-0000-000000000005");
    public static readonly Guid HeritageInnovationHubId = Guid.Parse("00000000-0000-0000-0000-000000000006");
    public static readonly Guid GovernmentNGOId = Guid.Parse("00000000-0000-0000-0000-000000000007");
    public static readonly Guid LogisticsPartnerId = Guid.Parse("00000000-0000-0000-0000-000000000008");
    public static readonly Guid SuperAdminId = Guid.Parse("00000000-0000-0000-0000-000000000009");

    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name).IsRequired().HasMaxLength(100);
        builder.HasIndex(r => r.Name).IsUnique();

        builder.Property(r => r.Description).HasMaxLength(500);

        builder.HasData(
            new { Id = CustomerId, Name = RoleNames.Customer, Description = (string?)"Buys products and services on the platform." },
            new { Id = ProducerId, Name = RoleNames.Producer, Description = (string?)"Produces and lists products on the platform." },
            new { Id = BusinessPartnerId, Name = RoleNames.BusinessPartner, Description = (string?)"Business partner collaborating with the platform." },
            new { Id = TouristId, Name = RoleNames.Tourist, Description = (string?)"Tourist exploring heritage and cultural offerings." },
            new { Id = HeritageAcademyMemberId, Name = RoleNames.HeritageAcademyMember, Description = (string?)"Member of the Heritage Academy." },
            new { Id = HeritageInnovationHubId, Name = RoleNames.HeritageInnovationHub, Description = (string?)"Heritage Innovation Hub participant." },
            new { Id = GovernmentNGOId, Name = RoleNames.GovernmentNGO, Description = (string?)"Government or NGO stakeholder." },
            new { Id = LogisticsPartnerId, Name = RoleNames.LogisticsPartner, Description = (string?)"Logistics and delivery partner." },
            new { Id = SuperAdminId, Name = RoleNames.SuperAdmin, Description = (string?)"Full administrative access to the platform." }
        );
    }
}
