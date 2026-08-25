using ShilpoHubBD.Domain.Entities.Learning;

namespace ShilpoHubBD.Application.DTOs.Roadmap;

// One candidate skill the roadmap could target, together with the courses/lessons available to
// recommend for it. Includes skills the learner already holds and ones they don't yet.
public class SkillPlanningInput
{
    public Guid HeritageSkillId { get; set; }
    public string Name { get; set; } = string.Empty;
    public SkillLevel? CurrentLevel { get; set; }
    public List<CandidateCourseInput> Courses { get; set; } = new();
}
