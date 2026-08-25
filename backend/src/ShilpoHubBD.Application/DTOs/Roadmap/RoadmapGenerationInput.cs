namespace ShilpoHubBD.Application.DTOs.Roadmap;

// Provider-agnostic snapshot used to generate a roadmap. Any ILearningRoadmapProvider implementation
// (rule-based today, AI-backed later) consumes this same shape.
public class RoadmapGenerationInput
{
    public string Goal { get; set; } = string.Empty;
    public Guid? TargetHeritageSkillId { get; set; }
    public List<SkillProgressInput> CurrentSkills { get; set; } = new();
    public List<SkillPlanningInput> CandidateSkills { get; set; } = new();
}
