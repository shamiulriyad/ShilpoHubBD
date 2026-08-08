namespace ShilpoHubBD.Domain.Entities.HeritageIdentity;

public class FamilyHeritageMember
{
    public Guid Id { get; set; }

    public Guid ProducerHeritageIdentityId { get; set; }
    public ProducerHeritageIdentity ProducerHeritageIdentity { get; set; } = null!;

    public string FullName { get; set; } = string.Empty;
    public string Relation { get; set; } = string.Empty;
    public int Generation { get; set; }
    public string? Role { get; set; }
    public string? ActiveYearsRange { get; set; }
    public string? Story { get; set; }
    public int DisplayOrder { get; set; }
}
