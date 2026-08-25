using ShilpoHubBD.Application.DTOs.MentorMatching;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;

namespace ShilpoHubBD.Application.Services.MentorMatching;

// Rule-based (not AI) matching engine: scores each mentor against the criteria the caller actually
// specified, so an unspecified filter neither helps nor hurts a candidate's score. Years of
// experience is always factored in as a small baseline quality signal.
public class MentorMatchingService : IMentorMatchingService
{
    private const decimal BaselineExperienceWeight = 5m;
    private const decimal SkillWeight = 25m;
    private const decimal SkillLevelWeight = 15m;
    private const decimal LocationWeight = 15m;
    private const decimal ExperienceWeight = 15m;
    private const decimal AvailabilityWeight = 10m;
    private const decimal CategoryWeight = 10m;
    private const decimal GoalKeywordWeight = 10m;

    private readonly IMentorMatchingRepository _mentorMatchingRepository;

    public MentorMatchingService(IMentorMatchingRepository mentorMatchingRepository)
    {
        _mentorMatchingRepository = mentorMatchingRepository;
    }

    public async Task<List<MentorMatchResultDto>> MatchAsync(MentorMatchRequest request, CancellationToken cancellationToken)
    {
        var candidates = await _mentorMatchingRepository.GetCandidatesAsync(request, cancellationToken);

        return candidates
            .Select(c => Score(c, request))
            .OrderByDescending(r => r.MatchScore)
            .ThenByDescending(r => r.YearsOfExperience)
            .Take(request.MaxResults)
            .ToList();
    }

    private static MentorMatchResultDto Score(MentorMatchCandidateDto candidate, MentorMatchRequest request)
    {
        var totalWeight = BaselineExperienceWeight;
        var achievedWeight = Math.Min(1m, candidate.YearsOfExperience / 10m) * BaselineExperienceWeight;
        var reasons = new List<string>();

        if (candidate.YearsOfExperience > 0)
        {
            reasons.Add($"{candidate.YearsOfExperience} year(s) of experience.");
        }

        if (request.HeritageSkillId.HasValue)
        {
            totalWeight += SkillWeight;
            if (candidate.HasMatchingSkill)
            {
                achievedWeight += SkillWeight;
                reasons.Add("Teaches the requested heritage skill.");

                if (request.MinSkillLevel.HasValue)
                {
                    totalWeight += SkillLevelWeight;
                    if (candidate.MatchingSkillLevel.HasValue && candidate.MatchingSkillLevel.Value >= request.MinSkillLevel.Value)
                    {
                        achievedWeight += SkillLevelWeight;
                        reasons.Add($"Skill level ({candidate.MatchingSkillLevel}) meets the requested {request.MinSkillLevel} level.");
                    }
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(request.Location))
        {
            totalWeight += LocationWeight;
            if (candidate.HasMatchingLocation)
            {
                achievedWeight += LocationWeight;
                reasons.Add($"Located in \"{request.Location.Trim()}\".");
            }
        }

        if (request.MinYearsOfExperience.HasValue && request.MinYearsOfExperience.Value > 0)
        {
            totalWeight += ExperienceWeight;
            var fraction = Math.Min(1m, candidate.YearsOfExperience / (decimal)request.MinYearsOfExperience.Value);
            achievedWeight += ExperienceWeight * fraction;
            if (candidate.YearsOfExperience >= request.MinYearsOfExperience.Value)
            {
                reasons.Add($"Meets the requested minimum of {request.MinYearsOfExperience.Value} year(s) of experience.");
            }
        }

        if (!string.IsNullOrWhiteSpace(request.AvailabilityKeyword))
        {
            totalWeight += AvailabilityWeight;
            if (candidate.HasMatchingAvailability)
            {
                achievedWeight += AvailabilityWeight;
                reasons.Add($"Availability matches \"{request.AvailabilityKeyword.Trim()}\".");
            }
        }

        if (!string.IsNullOrWhiteSpace(request.PreferredCategory))
        {
            totalWeight += CategoryWeight;
            if (candidate.HasMatchingCategory)
            {
                achievedWeight += CategoryWeight;
                reasons.Add($"Prefers teaching in the \"{request.PreferredCategory.Trim()}\" category.");
            }
        }

        if (!string.IsNullOrWhiteSpace(request.LearningGoalKeyword))
        {
            totalWeight += GoalKeywordWeight;
            if (candidate.HasMatchingGoalKeyword)
            {
                achievedWeight += GoalKeywordWeight;
                reasons.Add($"Profile matches your learning goal \"{request.LearningGoalKeyword.Trim()}\".");
            }
        }

        var score = totalWeight == 0m ? 0m : Math.Round(achievedWeight / totalWeight * 100m, 1);

        return new MentorMatchResultDto
        {
            MentorProfileId = candidate.MentorProfileId,
            UserId = candidate.UserId,
            FullName = candidate.FullName,
            Bio = candidate.Bio,
            Expertise = candidate.Expertise,
            YearsOfExperience = candidate.YearsOfExperience,
            Location = candidate.Location,
            AvailabilityNote = candidate.AvailabilityNote,
            PreferredCategory = candidate.PreferredCategory,
            MatchScore = score,
            MatchReasons = reasons,
        };
    }
}
