using ShilpoHubBD.Application.DTOs.Mentorship;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Mentorship;

namespace ShilpoHubBD.Application.Services.Mentorship;

public class MentorshipService : IMentorshipService
{
    private readonly IMentorshipRequestRepository _mentorshipRequestRepository;
    private readonly IMentorRepository _mentorRepository;
    private readonly IHeritageSkillRepository _heritageSkillRepository;

    public MentorshipService(
        IMentorshipRequestRepository mentorshipRequestRepository,
        IMentorRepository mentorRepository,
        IHeritageSkillRepository heritageSkillRepository)
    {
        _mentorshipRequestRepository = mentorshipRequestRepository;
        _mentorRepository = mentorRepository;
        _heritageSkillRepository = heritageSkillRepository;
    }

    public async Task<MentorshipRequestDto> CreateRequestAsync(
        Guid learnerUserId, CreateMentorshipRequestRequest request, CancellationToken cancellationToken)
    {
        var mentor = await _mentorRepository.GetByIdAsync(request.MentorProfileId, cancellationToken)
            ?? throw new NotFoundException("Mentor not found.");

        if (!mentor.IsActive)
        {
            throw new ConflictException("This mentor is not currently accepting mentorship requests.");
        }

        if (mentor.UserId == learnerUserId)
        {
            throw new ConflictException("You cannot request mentorship from yourself.");
        }

        if (request.HeritageSkillId.HasValue
            && await _heritageSkillRepository.GetByIdAsync(request.HeritageSkillId.Value, cancellationToken) is null)
        {
            throw new NotFoundException("Heritage skill not found.");
        }

        if (await _mentorshipRequestRepository.HasOpenRequestAsync(mentor.Id, learnerUserId, cancellationToken))
        {
            throw new ConflictException("You already have a pending or active mentorship with this mentor.");
        }

        var mentorshipRequest = new MentorshipRequest
        {
            Id = Guid.NewGuid(),
            MentorProfileId = mentor.Id,
            LearnerUserId = learnerUserId,
            HeritageSkillId = request.HeritageSkillId,
            Message = request.Message.Trim(),
            Status = MentorshipRequestStatus.Pending,
            RequestedAt = DateTime.UtcNow,
        };

        await _mentorshipRequestRepository.AddAsync(mentorshipRequest, cancellationToken);
        await _mentorshipRequestRepository.SaveChangesAsync(cancellationToken);

        var created = await _mentorshipRequestRepository.GetByIdAsync(mentorshipRequest.Id, cancellationToken);
        return ToDto(created!);
    }

    public async Task<MentorshipRequestDto> AcceptAsync(
        Guid mentorUserId, Guid requestId, RespondMentorshipRequestRequest request, CancellationToken cancellationToken)
    {
        var mentorshipRequest = await GetOwnedByMentorAsync(mentorUserId, requestId, cancellationToken);

        if (mentorshipRequest.Status != MentorshipRequestStatus.Pending)
        {
            throw new ConflictException("Only a pending mentorship request can be accepted.");
        }

        mentorshipRequest.Status = MentorshipRequestStatus.Accepted;
        mentorshipRequest.ResponseMessage = request.ResponseMessage?.Trim();
        mentorshipRequest.RespondedAt = DateTime.UtcNow;

        await _mentorshipRequestRepository.SaveChangesAsync(cancellationToken);
        return ToDto(mentorshipRequest);
    }

    public async Task<MentorshipRequestDto> RejectAsync(
        Guid mentorUserId, Guid requestId, RespondMentorshipRequestRequest request, CancellationToken cancellationToken)
    {
        var mentorshipRequest = await GetOwnedByMentorAsync(mentorUserId, requestId, cancellationToken);

        if (mentorshipRequest.Status != MentorshipRequestStatus.Pending)
        {
            throw new ConflictException("Only a pending mentorship request can be rejected.");
        }

        mentorshipRequest.Status = MentorshipRequestStatus.Rejected;
        mentorshipRequest.ResponseMessage = request.ResponseMessage?.Trim();
        mentorshipRequest.RespondedAt = DateTime.UtcNow;

        await _mentorshipRequestRepository.SaveChangesAsync(cancellationToken);
        return ToDto(mentorshipRequest);
    }

    public async Task<MentorshipRequestDto> CompleteAsync(Guid mentorUserId, Guid requestId, CancellationToken cancellationToken)
    {
        var mentorshipRequest = await GetOwnedByMentorAsync(mentorUserId, requestId, cancellationToken);

        if (mentorshipRequest.Status != MentorshipRequestStatus.Accepted)
        {
            throw new ConflictException("Only an active mentorship can be marked complete.");
        }

        mentorshipRequest.Status = MentorshipRequestStatus.Completed;
        mentorshipRequest.CompletedAt = DateTime.UtcNow;

        await _mentorshipRequestRepository.SaveChangesAsync(cancellationToken);
        return ToDto(mentorshipRequest);
    }

    public async Task<MentorshipRequestDto> GetByIdAsync(Guid userId, Guid requestId, CancellationToken cancellationToken)
    {
        var mentorshipRequest = await _mentorshipRequestRepository.GetByIdAsync(requestId, cancellationToken)
            ?? throw new NotFoundException("Mentorship request not found.");

        if (mentorshipRequest.LearnerUserId != userId && mentorshipRequest.MentorProfile.UserId != userId)
        {
            throw new UnauthorizedAccessException("You do not have permission to view this mentorship request.");
        }

        return ToDto(mentorshipRequest);
    }

    public async Task<List<MentorshipRequestListItemDto>> GetMyRequestsAsLearnerAsync(Guid learnerUserId, CancellationToken cancellationToken)
    {
        var requests = await _mentorshipRequestRepository.GetByLearnerAsync(learnerUserId, cancellationToken);
        return requests.Select(ToListItemDto).ToList();
    }

    public async Task<List<MentorshipRequestListItemDto>> GetMyRequestsAsMentorAsync(Guid mentorUserId, CancellationToken cancellationToken)
    {
        var mentor = await _mentorRepository.GetByUserIdAsync(mentorUserId, cancellationToken)
            ?? throw new NotFoundException("Mentor profile not found.");

        var requests = await _mentorshipRequestRepository.GetByMentorProfileAsync(mentor.Id, cancellationToken);
        return requests.Select(ToListItemDto).ToList();
    }

    private async Task<MentorshipRequest> GetOwnedByMentorAsync(Guid mentorUserId, Guid requestId, CancellationToken cancellationToken)
    {
        var mentorshipRequest = await _mentorshipRequestRepository.GetByIdAsync(requestId, cancellationToken)
            ?? throw new NotFoundException("Mentorship request not found.");

        if (mentorshipRequest.MentorProfile.UserId != mentorUserId)
        {
            throw new UnauthorizedAccessException("You do not have permission to manage this mentorship request.");
        }

        return mentorshipRequest;
    }

    private static MentorshipRequestListItemDto ToListItemDto(MentorshipRequest request) => new()
    {
        Id = request.Id,
        MentorProfileId = request.MentorProfileId,
        MentorName = request.MentorProfile.User.FullName,
        LearnerUserId = request.LearnerUserId,
        LearnerName = request.Learner.FullName,
        HeritageSkillName = request.HeritageSkill?.Name,
        Status = request.Status.ToString(),
        RequestedAt = request.RequestedAt,
    };

    private static MentorshipRequestDto ToDto(MentorshipRequest request) => new()
    {
        Id = request.Id,
        MentorProfileId = request.MentorProfileId,
        MentorName = request.MentorProfile.User.FullName,
        LearnerUserId = request.LearnerUserId,
        LearnerName = request.Learner.FullName,
        HeritageSkillId = request.HeritageSkillId,
        HeritageSkillName = request.HeritageSkill?.Name,
        Message = request.Message,
        Status = request.Status.ToString(),
        RequestedAt = request.RequestedAt,
        RespondedAt = request.RespondedAt,
        ResponseMessage = request.ResponseMessage,
        CompletedAt = request.CompletedAt,
    };
}
