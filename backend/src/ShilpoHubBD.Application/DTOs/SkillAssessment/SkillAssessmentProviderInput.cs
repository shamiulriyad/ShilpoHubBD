using ShilpoHubBD.Domain.Entities.Learning;

namespace ShilpoHubBD.Application.DTOs.SkillAssessment;

// Provider-agnostic snapshot of a learner's activity for one skill. Any IAISkillAssessmentProvider
// implementation (rule-based today, an LLM or ML model later) consumes this same shape.
public class SkillAssessmentProviderInput
{
    public Guid HeritageSkillId { get; set; }
    public string HeritageSkillName { get; set; } = string.Empty;
    public SkillLevel? CurrentLevel { get; set; }
    public List<PerformanceSignal> QuizPerformances { get; set; } = new();
    public List<PerformanceSignal> ExamPerformances { get; set; } = new();
    public List<PerformanceSignal> AssignmentPerformances { get; set; } = new();
    public int CompletedCourseCount { get; set; }
    public List<CandidateSkillInput> CandidateSkills { get; set; } = new();
}
