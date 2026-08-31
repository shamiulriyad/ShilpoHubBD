using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Innovation;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Innovation;

namespace ShilpoHubBD.Application.Services.Innovation;

public class HeritageInnovationSubmissionService : IHeritageInnovationSubmissionService
{
    private static readonly InnovationSubmissionStatus[] ReviewerVisibleStatuses =
    {
        InnovationSubmissionStatus.Submitted, InnovationSubmissionStatus.UnderReview,
        InnovationSubmissionStatus.RevisionRequested, InnovationSubmissionStatus.Approved,
        InnovationSubmissionStatus.Rejected,
    };

    private readonly IHeritageInnovationSubmissionRepository _repository;
    private readonly IInnovationLinkResolver _links;

    public HeritageInnovationSubmissionService(
        IHeritageInnovationSubmissionRepository repository, IInnovationLinkResolver links)
    {
        _repository = repository;
        _links = links;
    }

    public async Task<PagedResult<HeritageInnovationSubmissionListItemDto>> GetAccessibleAsync(
        Guid userId, bool isReviewer, HeritageInnovationSubmissionQueryParameters query, CancellationToken cancellationToken)
    {
        query.Page = query.Page < 1 ? 1 : query.Page;
        query.PageSize = query.PageSize is < 1 or > 100 ? 12 : query.PageSize;

        var (items, totalCount) = await _repository.GetPagedAsync(userId, isReviewer, query, cancellationToken);
        return new PagedResult<HeritageInnovationSubmissionListItemDto>
        {
            Items = items.Select(s => s.ToListItemDto()).ToList(),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }

    public async Task<HeritageInnovationSubmissionDetailDto> GetByIdAsync(
        Guid userId, bool isReviewer, Guid submissionId, CancellationToken cancellationToken)
    {
        var submission = await _repository.GetDetailAsync(submissionId, cancellationToken)
            ?? throw new NotFoundException("Submission not found.");
        RequireView(submission, userId, isReviewer);
        return ToDetailDto(submission, userId, isReviewer);
    }

    public async Task<HeritageInnovationSubmissionDetailDto> CreateAsync(
        Guid userId, bool isResearcher, CreateHeritageInnovationSubmissionRequest request, CancellationToken cancellationToken)
    {
        await ValidateLinksAsync(userId, isResearcher, request.ResearchProjectId, request.InnovationPrototypeId,
            request.PreservationStrategyId, request.HeritageDatasetId, cancellationToken);

        var now = DateTime.UtcNow;
        var submission = new HeritageInnovationSubmission
        {
            Id = Guid.NewGuid(),
            SubmitterUserId = userId,
            ResearchProjectId = request.ResearchProjectId,
            InnovationPrototypeId = request.InnovationPrototypeId,
            PreservationStrategyId = request.PreservationStrategyId,
            HeritageDatasetId = request.HeritageDatasetId,
            Title = request.Title.Trim(),
            Problem = request.Problem.Trim(),
            Solution = request.Solution.Trim(),
            ResearchEvidence = request.ResearchEvidence?.Trim(),
            Status = InnovationSubmissionStatus.Draft,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await _repository.AddAsync(submission, cancellationToken);
        await AddEventAsync(submission.Id, userId, SubmissionEventType.Created, "Submission created.", cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return ToDetailDto((await _repository.GetDetailAsync(submission.Id, cancellationToken))!, userId, false);
    }

    public async Task<HeritageInnovationSubmissionDetailDto> UpdateAsync(
        Guid userId, bool isResearcher, Guid submissionId, UpdateHeritageInnovationSubmissionRequest request, CancellationToken cancellationToken)
    {
        var submission = await _repository.GetDetailAsync(submissionId, cancellationToken)
            ?? throw new NotFoundException("Submission not found.");
        RequireSubmitter(submission, userId);

        if (submission.Status is not (InnovationSubmissionStatus.Draft or InnovationSubmissionStatus.RevisionRequested))
        {
            throw new ConflictException("Only draft or revision-requested submissions can be edited.");
        }

        await ValidateLinksAsync(userId, isResearcher, request.ResearchProjectId, request.InnovationPrototypeId,
            request.PreservationStrategyId, request.HeritageDatasetId, cancellationToken);

        submission.Title = request.Title.Trim();
        submission.Problem = request.Problem.Trim();
        submission.Solution = request.Solution.Trim();
        submission.ResearchEvidence = request.ResearchEvidence?.Trim();
        submission.ResearchProjectId = request.ResearchProjectId;
        submission.InnovationPrototypeId = request.InnovationPrototypeId;
        submission.PreservationStrategyId = request.PreservationStrategyId;
        submission.HeritageDatasetId = request.HeritageDatasetId;
        submission.UpdatedAt = DateTime.UtcNow;

        await AddEventAsync(submissionId, userId, SubmissionEventType.Updated, "Submission details updated.", cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return ToDetailDto(submission, userId, false);
    }

    public async Task DeleteAsync(Guid userId, Guid submissionId, CancellationToken cancellationToken)
    {
        var submission = await _repository.GetByIdAsync(submissionId, cancellationToken)
            ?? throw new NotFoundException("Submission not found.");
        RequireSubmitter(submission, userId);

        if (submission.Status != InnovationSubmissionStatus.Draft)
        {
            throw new ConflictException("Only draft submissions can be deleted. Withdraw it instead.");
        }

        _repository.Remove(submission);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task<HeritageInnovationSubmissionDetailDto> SubmitAsync(
        Guid userId, Guid submissionId, CancellationToken cancellationToken)
    {
        var submission = await _repository.GetDetailAsync(submissionId, cancellationToken)
            ?? throw new NotFoundException("Submission not found.");
        RequireSubmitter(submission, userId);

        if (submission.Status is not (InnovationSubmissionStatus.Draft or InnovationSubmissionStatus.RevisionRequested))
        {
            throw new ConflictException("Only draft or revision-requested submissions can be submitted.");
        }

        var now = DateTime.UtcNow;
        submission.Status = InnovationSubmissionStatus.Submitted;
        submission.SubmittedAt = now;
        submission.UpdatedAt = now;

        await AddEventAsync(submissionId, userId, SubmissionEventType.Submitted, "Submission sent for review.", cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return ToDetailDto(submission, userId, false);
    }

    public async Task<HeritageInnovationSubmissionDetailDto> WithdrawAsync(
        Guid userId, Guid submissionId, CancellationToken cancellationToken)
    {
        var submission = await _repository.GetDetailAsync(submissionId, cancellationToken)
            ?? throw new NotFoundException("Submission not found.");
        RequireSubmitter(submission, userId);

        if (submission.Status is InnovationSubmissionStatus.Approved or InnovationSubmissionStatus.Rejected
            or InnovationSubmissionStatus.Withdrawn)
        {
            throw new ConflictException("This submission can no longer be withdrawn.");
        }

        submission.Status = InnovationSubmissionStatus.Withdrawn;
        submission.UpdatedAt = DateTime.UtcNow;

        await AddEventAsync(submissionId, userId, SubmissionEventType.Withdrawn, "Submission withdrawn.", cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return ToDetailDto(submission, userId, false);
    }

    // ---- team ----

    public async Task<SubmissionTeamMemberDto> AddTeamMemberAsync(
        Guid userId, Guid submissionId, AddSubmissionTeamMemberRequest request, CancellationToken cancellationToken)
    {
        var submission = await _repository.GetByIdAsync(submissionId, cancellationToken)
            ?? throw new NotFoundException("Submission not found.");
        RequireSubmitter(submission, userId);

        if (request.UserId == submission.SubmitterUserId)
        {
            throw new ConflictException("The submitter is already on the team.");
        }

        if (submission.TeamMembers.Any(m => m.UserId == request.UserId))
        {
            throw new ConflictException("This user is already a team member.");
        }

        if (!await _links.UserExistsAsync(request.UserId, cancellationToken))
        {
            throw new NotFoundException("User not found.");
        }

        var member = new SubmissionTeamMember
        {
            Id = Guid.NewGuid(),
            HeritageInnovationSubmissionId = submissionId,
            UserId = request.UserId,
            RoleOnTeam = request.RoleOnTeam?.Trim(),
            AddedByUserId = userId,
            AddedAt = DateTime.UtcNow,
        };

        await _repository.AddTeamMemberAsync(member, cancellationToken);
        await AddEventAsync(submissionId, userId, SubmissionEventType.TeamMemberAdded, "Team member added.", cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return (await _repository.GetTeamMemberByIdAsync(member.Id, cancellationToken))!.ToDto();
    }

    public async Task RemoveTeamMemberAsync(Guid userId, Guid submissionId, Guid memberId, CancellationToken cancellationToken)
    {
        var submission = await _repository.GetByIdAsync(submissionId, cancellationToken)
            ?? throw new NotFoundException("Submission not found.");
        RequireSubmitter(submission, userId);

        var member = await _repository.GetTeamMemberByIdAsync(memberId, cancellationToken);
        if (member is null || member.HeritageInnovationSubmissionId != submissionId)
        {
            throw new NotFoundException("Team member not found.");
        }

        _repository.RemoveTeamMember(member);
        await AddEventAsync(submissionId, userId, SubmissionEventType.TeamMemberRemoved, "Team member removed.", cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    // ---- review ----

    public async Task<SubmissionReviewDto> AddReviewAsync(
        Guid userId, bool isReviewer, Guid submissionId, CreateSubmissionReviewRequest request, CancellationToken cancellationToken)
    {
        var submission = await _repository.GetDetailAsync(submissionId, cancellationToken)
            ?? throw new NotFoundException("Submission not found.");

        if (!isReviewer)
        {
            throw new UnauthorizedAccessException("Only a GovernmentNGO or SuperAdmin reviewer can review submissions.");
        }

        if (submission.SubmitterUserId == userId || submission.TeamMembers.Any(m => m.UserId == userId))
        {
            throw new ConflictException("You cannot review a submission you are part of.");
        }

        if (submission.Status is not (InnovationSubmissionStatus.Submitted or InnovationSubmissionStatus.UnderReview))
        {
            throw new ConflictException("Only submitted or under-review submissions can be reviewed.");
        }

        if (!Enum.TryParse<SubmissionReviewDecision>(request.Decision, true, out var decision))
        {
            throw new ConflictException("Decision must be one of: Comment, RequestRevision, Approve, Reject.");
        }

        if (request.Score is < 0 or > 100)
        {
            throw new ConflictException("Score must be between 0 and 100.");
        }

        var now = DateTime.UtcNow;
        var review = new SubmissionReview
        {
            Id = Guid.NewGuid(),
            HeritageInnovationSubmissionId = submissionId,
            ReviewerUserId = userId,
            Decision = decision,
            Score = request.Score,
            Comments = request.Comments.Trim(),
            CreatedAt = now,
        };

        await _repository.AddReviewAsync(review, cancellationToken);

        var (newStatus, eventType, summary) = decision switch
        {
            SubmissionReviewDecision.Approve => (InnovationSubmissionStatus.Approved, SubmissionEventType.Approved, "Submission approved."),
            SubmissionReviewDecision.Reject => (InnovationSubmissionStatus.Rejected, SubmissionEventType.Rejected, "Submission rejected."),
            SubmissionReviewDecision.RequestRevision => (InnovationSubmissionStatus.RevisionRequested, SubmissionEventType.RevisionRequested, "Revision requested."),
            _ => (InnovationSubmissionStatus.UnderReview, SubmissionEventType.ReviewAdded, "Review comment added."),
        };

        submission.Status = newStatus;
        submission.UpdatedAt = now;
        if (decision is SubmissionReviewDecision.Approve or SubmissionReviewDecision.Reject)
        {
            submission.DecisionByUserId = userId;
            submission.DecisionAt = now;
            submission.DecisionNote = review.Comments;
        }

        await AddEventAsync(submissionId, userId, eventType, summary, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return (await _repository.GetReviewsAsync(submissionId, cancellationToken)).First(r => r.Id == review.Id).ToDto();
    }

    public async Task<List<SubmissionEventDto>> GetHistoryAsync(
        Guid userId, bool isReviewer, Guid submissionId, int take, CancellationToken cancellationToken)
    {
        var submission = await _repository.GetByIdAsync(submissionId, cancellationToken)
            ?? throw new NotFoundException("Submission not found.");
        RequireView(submission, userId, isReviewer);

        take = Math.Clamp(take, 1, 200);
        var events = await _repository.GetHistoryAsync(submissionId, take, cancellationToken);
        return events.Select(e => e.ToDto()).ToList();
    }

    // ---- helpers ----

    private async Task AddEventAsync(
        Guid submissionId, Guid actorUserId, SubmissionEventType type, string summary, CancellationToken cancellationToken)
    {
        await _repository.AddEventAsync(new SubmissionEvent
        {
            Id = Guid.NewGuid(),
            HeritageInnovationSubmissionId = submissionId,
            ActorUserId = actorUserId,
            EventType = type,
            Summary = summary.Length > 500 ? summary[..500] : summary,
            CreatedAt = DateTime.UtcNow,
        }, cancellationToken);
    }

    private static void RequireSubmitter(HeritageInnovationSubmission submission, Guid userId)
    {
        if (submission.SubmitterUserId != userId)
        {
            throw new UnauthorizedAccessException("Only the submitter can perform this action.");
        }
    }

    private static void RequireView(HeritageInnovationSubmission submission, Guid userId, bool isReviewer)
    {
        if (submission.SubmitterUserId == userId
            || submission.TeamMembers.Any(m => m.UserId == userId)
            || (isReviewer && ReviewerVisibleStatuses.Contains(submission.Status)))
        {
            return;
        }

        throw new NotFoundException("Submission not found.");
    }

    private static bool CanReview(HeritageInnovationSubmission submission, Guid userId, bool isReviewer)
        => isReviewer
            && submission.SubmitterUserId != userId
            && submission.TeamMembers.All(m => m.UserId != userId)
            && submission.Status is InnovationSubmissionStatus.Submitted or InnovationSubmissionStatus.UnderReview;

    private async Task ValidateLinksAsync(
        Guid userId, bool isResearcher, Guid? projectId, Guid? prototypeId, Guid? strategyId, Guid? datasetId,
        CancellationToken cancellationToken)
    {
        if (projectId.HasValue && !await _links.IsResearchProjectMemberAsync(projectId.Value, userId, cancellationToken))
        {
            throw new ConflictException("You can only link a research project you belong to.");
        }

        if (prototypeId.HasValue && !await _links.InnovationPrototypeOwnedByAsync(prototypeId.Value, userId, cancellationToken))
        {
            throw new ConflictException("You can only link a prototype you own.");
        }

        if (strategyId.HasValue && !await _links.PreservationStrategyOwnedByAsync(strategyId.Value, userId, cancellationToken))
        {
            throw new ConflictException("You can only link a preservation strategy you own.");
        }

        if (datasetId.HasValue && !await _links.IsDatasetAccessibleAsync(datasetId.Value, userId, isResearcher, cancellationToken))
        {
            throw new ConflictException("You do not have access to the linked dataset.");
        }
    }

    private static HeritageInnovationSubmissionDetailDto ToDetailDto(
        HeritageInnovationSubmission s, Guid userId, bool isReviewer) => new()
    {
        Id = s.Id,
        Title = s.Title,
        Problem = s.Problem,
        Solution = s.Solution,
        ResearchEvidence = s.ResearchEvidence,
        Status = s.Status.ToString(),
        SubmitterUserId = s.SubmitterUserId,
        SubmitterName = s.Submitter?.FullName ?? string.Empty,
        ResearchProjectId = s.ResearchProjectId,
        InnovationPrototypeId = s.InnovationPrototypeId,
        PreservationStrategyId = s.PreservationStrategyId,
        HeritageDatasetId = s.HeritageDatasetId,
        SubmittedAt = s.SubmittedAt,
        DecisionByUserId = s.DecisionByUserId,
        DecisionByName = s.DecisionBy?.FullName,
        DecisionAt = s.DecisionAt,
        DecisionNote = s.DecisionNote,
        CanReview = CanReview(s, userId, isReviewer),
        CreatedAt = s.CreatedAt,
        UpdatedAt = s.UpdatedAt,
        TeamMembers = s.TeamMembers.OrderBy(m => m.AddedAt).Select(m => m.ToDto()).ToList(),
        Reviews = s.Reviews.OrderByDescending(r => r.CreatedAt).Select(r => r.ToDto()).ToList(),
    };
}
