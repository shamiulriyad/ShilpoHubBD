using ShilpoHubBD.Application.DTOs.Portfolio;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Mentorship;
using ShilpoHubBD.Domain.Entities.Portfolio;

namespace ShilpoHubBD.Application.Services.Portfolio;

public class MentorFeedbackService : IMentorFeedbackService
{
    private readonly IMentorFeedbackRepository _mentorFeedbackRepository;
    private readonly IMentorRepository _mentorRepository;
    private readonly IMentorshipRequestRepository _mentorshipRequestRepository;
    private readonly IHeritageSkillRepository _heritageSkillRepository;

    public MentorFeedbackService(
        IMentorFeedbackRepository mentorFeedbackRepository,
        IMentorRepository mentorRepository,
        IMentorshipRequestRepository mentorshipRequestRepository,
        IHeritageSkillRepository heritageSkillRepository)
    {
        _mentorFeedbackRepository = mentorFeedbackRepository;
        _mentorRepository = mentorRepository;
        _mentorshipRequestRepository = mentorshipRequestRepository;
        _heritageSkillRepository = heritageSkillRepository;
    }

    public async Task<MentorFeedbackDto> SubmitAsync(Guid mentorUserId, SubmitMentorFeedbackRequest request, CancellationToken cancellationToken)
    {
        var mentor = await _mentorRepository.GetByUserIdAsync(mentorUserId, cancellationToken)
            ?? throw new ConflictException("You must have a mentor profile before leaving feedback.");

        var mentorshipRequests = await _mentorshipRequestRepository.GetByMentorProfileAsync(mentor.Id, cancellationToken);
        var hasRelationship = mentorshipRequests.Any(r =>
            r.LearnerUserId == request.LearnerUserId
            && (r.Status == MentorshipRequestStatus.Accepted || r.Status == MentorshipRequestStatus.Completed));

        if (!hasRelationship)
        {
            throw new ConflictException("You can only leave feedback for a learner you have an active or completed mentorship with.");
        }

        if (request.HeritageSkillId.HasValue
            && await _heritageSkillRepository.GetByIdAsync(request.HeritageSkillId.Value, cancellationToken) is null)
        {
            throw new NotFoundException("Heritage skill not found.");
        }

        var feedback = new MentorFeedback
        {
            Id = Guid.NewGuid(),
            MentorProfileId = mentor.Id,
            LearnerUserId = request.LearnerUserId,
            HeritageSkillId = request.HeritageSkillId,
            Message = request.Message.Trim(),
            Rating = request.Rating,
            CreatedAt = DateTime.UtcNow,
        };

        await _mentorFeedbackRepository.AddAsync(feedback, cancellationToken);
        await _mentorFeedbackRepository.SaveChangesAsync(cancellationToken);

        var created = (await _mentorFeedbackRepository.GetByLearnerAsync(request.LearnerUserId, cancellationToken))
            .First(f => f.Id == feedback.Id);
        return ToDto(created);
    }

    public async Task<List<MentorFeedbackDto>> GetForLearnerAsync(Guid learnerUserId, CancellationToken cancellationToken)
    {
        var feedback = await _mentorFeedbackRepository.GetByLearnerAsync(learnerUserId, cancellationToken);
        return feedback.Select(ToDto).ToList();
    }

    private static MentorFeedbackDto ToDto(MentorFeedback feedback) => new()
    {
        Id = feedback.Id,
        MentorProfileId = feedback.MentorProfileId,
        MentorName = feedback.MentorProfile.User.FullName,
        LearnerUserId = feedback.LearnerUserId,
        HeritageSkillId = feedback.HeritageSkillId,
        HeritageSkillName = feedback.HeritageSkill?.Name,
        Message = feedback.Message,
        Rating = feedback.Rating,
        CreatedAt = feedback.CreatedAt,
    };
}
