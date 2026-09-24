using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Domain.Entities.Tourism;

public class TourismLocation
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public TourismLocationType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Address { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public decimal? Price { get; set; }
    public decimal? EntryFee { get; set; }
    public string? OpeningHours { get; set; }
    public string? ContactInfo { get; set; }
    public string? Facilities { get; set; }
    public string? ImageUrl { get; set; }
    // For an imported photo: the credit line the licence requires (author, licence, source) and the
    // page it came from. Null for an admin-uploaded image or when there is no photo.
    public string? ImageCredit { get; set; }
    public string? ImageSourceUrl { get; set; }
    public bool IsVerified { get; set; }

    // Provenance for imported listings (see Seed/TourismData). A null field means "not available"
    // -- never a guess. VerificationStatus: Verified (official operator/government source),
    // SecondarySource (reputable but non-official) or Unverified (admin-entered / sample).
    public string? Area { get; set; }
    public string? SourceUrl { get; set; }
    public string VerificationStatus { get; set; } = "Unverified";
    public string? UnverifiedFields { get; set; }
    public string? CoordinatesSource { get; set; }
    // official_listing | community_listing | geocoded_approximate | admin_entered
    public string? CoordinatesPrecision { get; set; }
    public DateTime? DataRetrievedOn { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Guid DistrictId { get; set; }
    public District District { get; set; } = null!;
}
