using ShilpoHubBD.Application.DTOs.SkillAssessment;

namespace ShilpoHubBD.Application.Interfaces.Services;

// Produces a skill assessment from a learner's activity snapshot. The Application layer only ever
// depends on this abstraction, so the rule-based DummySkillAssessmentProvider can later be swapped
// for a Gemini/OpenAI/custom-ML-backed implementation without touching SkillAssessmentService.
public interface IAISkillAssessmentProvider
{
    Task<SkillAssessmentProviderResult> AssessAsync(SkillAssessmentProviderInput input, CancellationToken cancellationToken);
}
