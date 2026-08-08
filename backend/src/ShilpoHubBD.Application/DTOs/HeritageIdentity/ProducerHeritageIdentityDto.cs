using ShilpoHubBD.Domain.Entities.HeritageIdentity;

namespace ShilpoHubBD.Application.DTOs.HeritageIdentity;

public class ProducerHeritageIdentityDto
{
    public Guid Id { get; set; }
    public Guid ProducerId { get; set; }
    public string ProducerName { get; set; } = string.Empty;

    public string HeritageIdNumber { get; set; } = string.Empty;
    public string PrimaryCraft { get; set; } = string.Empty;
    public int YearsOfExperience { get; set; }

    public string WorkshopName { get; set; } = string.Empty;
    public string WorkshopDescription { get; set; } = string.Empty;
    public string? WorkshopAddress { get; set; }
    public int? EstablishedYear { get; set; }

    public Guid? DistrictId { get; set; }
    public string? DistrictName { get; set; }

    public HeritageVerificationStatus VerificationStatus { get; set; }
    public string? VerifiedByName { get; set; }
    public string? VerificationNotes { get; set; }
    public DateTime? VerifiedAt { get; set; }

    public int LegacyScore { get; set; }

    public List<FamilyHeritageMemberDto> FamilyMembers { get; set; } = new();
    public List<SkillTimelineEntryDto> SkillTimeline { get; set; } = new();
    public List<HeritageAwardDto> Awards { get; set; } = new();
    public List<HeritageCertificationDto> Certifications { get; set; } = new();
    public List<StoryArchiveEntryDto> StoryArchive { get; set; } = new();

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
