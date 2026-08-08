namespace ShilpoHubBD.Domain.Entities.HeritageIdentity;

public class HeritageAward
{
    public Guid Id { get; set; }

    public Guid ProducerHeritageIdentityId { get; set; }
    public ProducerHeritageIdentity ProducerHeritageIdentity { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string IssuingOrganization { get; set; } = string.Empty;
    public int Year { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; }
}
