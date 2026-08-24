using ShilpoHubBD.Application.DTOs.Mentorship;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IMentorshipService
{
    Task<MentorshipRequestDto> CreateRequestAsync(Guid learnerUserId, CreateMentorshipRequestRequest request, CancellationToken cancellationToken);

    Task<MentorshipRequestDto> AcceptAsync(Guid mentorUserId, Guid requestId, RespondMentorshipRequestRequest request, CancellationToken cancellationToken);

    Task<MentorshipRequestDto> RejectAsync(Guid mentorUserId, Guid requestId, RespondMentorshipRequestRequest request, CancellationToken cancellationToken);

    Task<MentorshipRequestDto> CompleteAsync(Guid mentorUserId, Guid requestId, CancellationToken cancellationToken);

    Task<MentorshipRequestDto> GetByIdAsync(Guid userId, Guid requestId, CancellationToken cancellationToken);

    Task<List<MentorshipRequestListItemDto>> GetMyRequestsAsLearnerAsync(Guid learnerUserId, CancellationToken cancellationToken);

    Task<List<MentorshipRequestListItemDto>> GetMyRequestsAsMentorAsync(Guid mentorUserId, CancellationToken cancellationToken);
}
