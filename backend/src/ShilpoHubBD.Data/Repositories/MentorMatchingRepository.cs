using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.DTOs.MentorMatching;
using ShilpoHubBD.Application.Interfaces.Repositories;

namespace ShilpoHubBD.Data.Repositories;

public class MentorMatchingRepository : IMentorMatchingRepository
{
    private readonly ShilpoHubDbContext _context;

    public MentorMatchingRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    public async Task<List<MentorMatchCandidateDto>> GetCandidatesAsync(MentorMatchRequest request, CancellationToken cancellationToken)
    {
        var mentors = await _context.MentorProfiles
            .Where(m => m.IsActive)
            .Include(m => m.User)
            .Include(m => m.Skills)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);

        return mentors.Select(m =>
        {
            var matchingSkill = request.HeritageSkillId.HasValue
                ? m.Skills.FirstOrDefault(s => s.HeritageSkillId == request.HeritageSkillId.Value)
                : null;

            return new MentorMatchCandidateDto
            {
                MentorProfileId = m.Id,
                UserId = m.UserId,
                FullName = m.User.FullName,
                Bio = m.Bio,
                Expertise = m.Expertise,
                YearsOfExperience = m.YearsOfExperience,
                Location = m.Location,
                AvailabilityNote = m.AvailabilityNote,
                PreferredCategory = m.PreferredCategory,
                HasMatchingSkill = matchingSkill is not null,
                MatchingSkillLevel = matchingSkill?.Level,
                HasMatchingLocation = ContainsIgnoreCase(m.Location, request.Location),
                HasMatchingGoalKeyword = ContainsIgnoreCase(m.Bio, request.LearningGoalKeyword)
                    || ContainsIgnoreCase(m.Expertise, request.LearningGoalKeyword),
                HasMatchingAvailability = ContainsIgnoreCase(m.AvailabilityNote, request.AvailabilityKeyword),
                HasMatchingCategory = ContainsIgnoreCase(m.PreferredCategory, request.PreferredCategory),
            };
        }).ToList();
    }

    private static bool ContainsIgnoreCase(string? haystack, string? needle)
        => !string.IsNullOrWhiteSpace(needle) && !string.IsNullOrWhiteSpace(haystack)
            && haystack.Contains(needle, StringComparison.OrdinalIgnoreCase);
}
