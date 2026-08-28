namespace ShilpoHubBD.Domain.Entities.Learning;

public class AcademyMemberSkill
{
    public Guid Id { get; set; }

    public Guid AcademyMemberProfileId { get; set; }
    public AcademyMemberProfile AcademyMemberProfile { get; set; } = null!;

    public Guid HeritageSkillId { get; set; }
    public HeritageSkill HeritageSkill { get; set; } = null!;

    public SkillLevel Level { get; set; } = SkillLevel.Beginner;
    public DateTime AddedAt { get; set; }
}
