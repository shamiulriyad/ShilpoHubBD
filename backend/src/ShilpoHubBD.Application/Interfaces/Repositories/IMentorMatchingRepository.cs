using ShilpoHubBD.Application.DTOs.MentorMatching;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IMentorMatchingRepository
{
    Task<List<MentorMatchCandidateDto>> GetCandidatesAsync(MentorMatchRequest request, CancellationToken cancellationToken);
}
