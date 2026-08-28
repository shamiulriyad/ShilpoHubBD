using ShilpoHubBD.Domain.Entities.Identity;
using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Domain.Entities.HeritageIdentity;

public class ProducerHeritageIdentity
{
    public Guid Id { get; set; }

    public Guid ProducerId { get; set; }
    public User Producer { get; set; } = null!;

    // Unique public identifier for this producer's heritage profile, distinct from any
    // internal database key so it can be printed on certificates/QR codes.
    public string HeritageIdNumber { get; set; } = string.Empty;

    public string PrimaryCraft { get; set; } = string.Empty;
    public int YearsOfExperience { get; set; }

    public string WorkshopName { get; set; } = string.Empty;
    public string WorkshopDescription { get; set; } = string.Empty;
    public string? WorkshopAddress { get; set; }
    public int? EstablishedYear { get; set; }

    public Guid? DistrictId { get; set; }
    public District? District { get; set; }

    public HeritageVerificationStatus VerificationStatus { get; set; } = HeritageVerificationStatus.Pending;
    public Guid? VerifiedByUserId { get; set; }
    public User? VerifiedBy { get; set; }
    public string? VerificationNotes { get; set; }
    public DateTime? VerifiedAt { get; set; }

    // Cached, recomputed after every mutation via configurable rule-based weights (see HeritageScoreOptions).
    public int LegacyScore { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<FamilyHeritageMember> FamilyMembers { get; set; } = new List<FamilyHeritageMember>();
    public ICollection<SkillTimelineEntry> SkillTimeline { get; set; } = new List<SkillTimelineEntry>();
    public ICollection<HeritageAward> Awards { get; set; } = new List<HeritageAward>();
    public ICollection<HeritageCertification> Certifications { get; set; } = new List<HeritageCertification>();
    public ICollection<StoryArchiveEntry> StoryArchive { get; set; } = new List<StoryArchiveEntry>();
    public ICollection<ScoreHistoryEntry> ScoreHistory { get; set; } = new List<ScoreHistoryEntry>();
}
