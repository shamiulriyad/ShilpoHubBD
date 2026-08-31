namespace ShilpoHubBD.Domain.Entities.Learning;

public class MentorSkill
{
    public Guid Id { get; set; }

    public Guid MentorProfileId { get; set; }
    public MentorProfile MentorProfile { get; set; } = null!;

    public Guid HeritageSkillId { get; set; }
    public HeritageSkill HeritageSkill { get; set; } = null!;

    public SkillLevel Level { get; set; } = SkillLevel.Advanced;
    public DateTime AddedAt { get; set; }
}
