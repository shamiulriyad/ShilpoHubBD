using ShilpoHubBD.Application.DTOs.Employment;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Employment;
using ShilpoHubBD.Domain.Entities.Learning;

namespace ShilpoHubBD.Application.Services.Employment;

// Rule-based (not AI) matching engine: scores each published job against the academy member's own
// heritage skills, plus any optional criteria (location, years of experience) the caller supplies.
// An unspecified or inapplicable criterion neither helps nor hurts a job's score.
public class JobMatchingService : IJobMatchingService
{
    private const decimal RequiredSkillWeight = 45m;
    private const decimal PreferredSkillWeight = 20m;
    private const decimal LocationWeight = 20m;
    private const decimal ExperienceWeight = 15m;

    private readonly IJobListingRepository _jobListingRepository;
    private readonly IAcademyMemberProfileRepository _academyMemberProfileRepository;

    public JobMatchingService(
        IJobListingRepository jobListingRepository,
        IAcademyMemberProfileRepository academyMemberProfileRepository)
    {
        _jobListingRepository = jobListingRepository;
        _academyMemberProfileRepository = academyMemberProfileRepository;
    }

    public async Task<List<JobMatchResultDto>> GetRecommendedJobsAsync(Guid userId, JobMatchRequest request, CancellationToken cancellationToken)
    {
        var profile = await _academyMemberProfileRepository.GetByUserIdAsync(userId, cancellationToken);
        var memberSkills = profile?.Skills.ToDictionary(s => s.HeritageSkillId, s => s.Level) ?? new Dictionary<Guid, SkillLevel>();

        var jobs = await _jobListingRepository.GetPublishedForMatchingAsync(cancellationToken);

        return jobs
            .Select(j => Score(j, memberSkills, request))
            .OrderByDescending(r => r.MatchScore)
            .Take(request.MaxResults)
            .ToList();
    }

    private static JobMatchResultDto Score(JobListing job, IReadOnlyDictionary<Guid, SkillLevel> memberSkills, JobMatchRequest request)
    {
        var totalWeight = 0m;
        var achievedWeight = 0m;
        var reasons = new List<string>();

        var requiredSkills = job.SkillRequirements.Where(r => r.IsRequired).ToList();
        if (requiredSkills.Count > 0)
        {
            totalWeight += RequiredSkillWeight;
            var met = requiredSkills.Count(r => memberSkills.TryGetValue(r.HeritageSkillId, out var level) && level >= r.MinLevel);
            achievedWeight += RequiredSkillWeight * (met / (decimal)requiredSkills.Count);
            if (met > 0)
            {
                reasons.Add($"Meets {met} of {requiredSkills.Count} required skill(s).");
            }
        }

        var preferredSkills = job.SkillRequirements.Where(r => !r.IsRequired).ToList();
        if (preferredSkills.Count > 0)
        {
            totalWeight += PreferredSkillWeight;
            var met = preferredSkills.Count(r => memberSkills.TryGetValue(r.HeritageSkillId, out var level) && level >= r.MinLevel);
            achievedWeight += PreferredSkillWeight * (met / (decimal)preferredSkills.Count);
            if (met > 0)
            {
                reasons.Add($"Meets {met} of {preferredSkills.Count} preferred skill(s).");
            }
        }

        if (!string.IsNullOrWhiteSpace(request.Location) && !string.IsNullOrWhiteSpace(job.Location))
        {
            totalWeight += LocationWeight;
            if (job.Location.Contains(request.Location, StringComparison.OrdinalIgnoreCase))
            {
                achievedWeight += LocationWeight;
                reasons.Add($"Located in \"{request.Location.Trim()}\".");
            }
        }

        if (job.MinExperienceYears.HasValue && job.MinExperienceYears.Value > 0 && request.YearsOfExperience.HasValue)
        {
            totalWeight += ExperienceWeight;
            var fraction = Math.Min(1m, request.YearsOfExperience.Value / (decimal)job.MinExperienceYears.Value);
            achievedWeight += ExperienceWeight * fraction;
            if (request.YearsOfExperience.Value >= job.MinExperienceYears.Value)
            {
                reasons.Add($"Meets the required minimum of {job.MinExperienceYears.Value} year(s) of experience.");
            }
        }

        var score = totalWeight == 0m ? 0m : Math.Round(achievedWeight / totalWeight * 100m, 1);

        return new JobMatchResultDto
        {
            JobListingId = job.Id,
            Title = job.Title,
            EmployerName = job.BusinessPartnerProfile.CompanyName,
            Location = job.Location,
            EmploymentType = job.EmploymentType.ToString(),
            MinExperienceYears = job.MinExperienceYears,
            SalaryMin = job.SalaryMin,
            SalaryMax = job.SalaryMax,
            MatchScore = score,
            MatchReasons = reasons,
        };
    }
}
