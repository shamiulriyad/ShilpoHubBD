using ShilpoHubBD.Application.DTOs.SkillAssessment;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Learning;

namespace ShilpoHubBD.Application.Services.SkillAssessment;

// Rule-based stand-in for a future AI-backed provider. No external model calls — scores are derived
// purely from the learner's existing quiz/exam/assignment/course activity supplied in the input.
public class DummySkillAssessmentProvider : IAISkillAssessmentProvider
{
    private const decimal ExpertThreshold = 85m;
    private const decimal AdvancedThreshold = 70m;
    private const decimal IntermediateThreshold = 50m;
    private const int MaxRecommendedSkills = 3;

    public Task<SkillAssessmentProviderResult> AssessAsync(SkillAssessmentProviderInput input, CancellationToken cancellationToken)
    {
        var allSignals = input.QuizPerformances
            .Concat(input.ExamPerformances)
            .Concat(input.AssignmentPerformances)
            .ToList();

        var hasSignals = allSignals.Count > 0;
        var score = hasSignals
            ? Math.Round(allSignals.Average(s => s.PercentageScore), 2)
            : FallbackScore(input.CurrentLevel);

        var level = LevelFromScore(score);

        var strengths = new List<string>();
        var weaknesses = new List<string>();

        AddPerformanceInsight(input.QuizPerformances, "quiz", input.HeritageSkillName, strengths, weaknesses);
        AddPerformanceInsight(input.ExamPerformances, "exam", input.HeritageSkillName, strengths, weaknesses);
        AddPerformanceInsight(input.AssignmentPerformances, "assignment", input.HeritageSkillName, strengths, weaknesses);

        if (input.CompletedCourseCount > 0)
        {
            strengths.Add($"Completed {input.CompletedCourseCount} course(s) related to {input.HeritageSkillName}.");
        }

        if (!hasSignals)
        {
            weaknesses.Add("No quiz, exam, or assignment history yet — complete some coursework for a more accurate assessment.");
        }

        if (strengths.Count == 0)
        {
            strengths.Add($"Actively engaged in learning {input.HeritageSkillName}.");
        }

        var recommendedSkills = input.CandidateSkills
            .Where(c => c.HeritageSkillId != input.HeritageSkillId)
            .Take(MaxRecommendedSkills)
            .Select(c => new RecommendedSkillResult
            {
                HeritageSkillId = c.HeritageSkillId,
                Reason = $"Complements your progress in {input.HeritageSkillName} and broadens your heritage craft expertise.",
            })
            .ToList();

        var result = new SkillAssessmentProviderResult
        {
            Level = level,
            Score = score,
            Summary = $"Based on available learning activity, current proficiency in {input.HeritageSkillName} is assessed as {level}.",
            Strengths = strengths,
            Weaknesses = weaknesses,
            RecommendedSkills = recommendedSkills,
        };

        return Task.FromResult(result);
    }

    private static void AddPerformanceInsight(
        List<PerformanceSignal> signals, string label, string skillName, List<string> strengths, List<string> weaknesses)
    {
        if (signals.Count == 0)
        {
            return;
        }

        var average = Math.Round(signals.Average(s => s.PercentageScore), 1);
        if (average >= AdvancedThreshold)
        {
            strengths.Add($"Strong {label} performance for {skillName} (average {average}%).");
        }
        else if (average < IntermediateThreshold)
        {
            weaknesses.Add($"{char.ToUpperInvariant(label[0])}{label[1..]} scores for {skillName} are below average (average {average}%) — more practice is recommended.");
        }
    }

    private static decimal FallbackScore(SkillLevel? currentLevel) => currentLevel switch
    {
        SkillLevel.Expert => 90m,
        SkillLevel.Advanced => 75m,
        SkillLevel.Intermediate => 55m,
        _ => 30m,
    };

    private static SkillLevel LevelFromScore(decimal score) => score switch
    {
        >= ExpertThreshold => SkillLevel.Expert,
        >= AdvancedThreshold => SkillLevel.Advanced,
        >= IntermediateThreshold => SkillLevel.Intermediate,
        _ => SkillLevel.Beginner,
    };
}
