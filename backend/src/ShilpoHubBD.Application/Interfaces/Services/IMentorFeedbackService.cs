using ShilpoHubBD.Application.DTOs.Portfolio;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IMentorFeedbackService
{
    Task<MentorFeedbackDto> SubmitAsync(Guid mentorUserId, SubmitMentorFeedbackRequest request, CancellationToken cancellationToken);

    Task<List<MentorFeedbackDto>> GetForLearnerAsync(Guid learnerUserId, CancellationToken cancellationToken);
}
