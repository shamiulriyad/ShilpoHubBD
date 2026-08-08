namespace ShilpoHubBD.Application.DTOs.HeritageIdentity;

public class UpsertHeritageIdentityRequest
{
    public string PrimaryCraft { get; set; } = string.Empty;
    public int YearsOfExperience { get; set; }

    public string WorkshopName { get; set; } = string.Empty;
    public string WorkshopDescription { get; set; } = string.Empty;
    public string? WorkshopAddress { get; set; }
    public int? EstablishedYear { get; set; }

    public Guid? DistrictId { get; set; }

    public List<FamilyHeritageMemberInput> FamilyMembers { get; set; } = new();
    public List<SkillTimelineEntryInput> SkillTimeline { get; set; } = new();
    public List<HeritageAwardInput> Awards { get; set; } = new();
    public List<HeritageCertificationInput> Certifications { get; set; } = new();
    public List<StoryArchiveEntryInput> StoryArchive { get; set; } = new();
}
