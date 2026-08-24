using ShilpoHubBD.Application.DTOs.MentorMatching;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IMentorMatchingService
{
    Task<List<MentorMatchResultDto>> MatchAsync(MentorMatchRequest request, CancellationToken cancellationToken);
}
