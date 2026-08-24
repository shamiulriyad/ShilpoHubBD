using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.LiveClass;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Learning;
using ShilpoHubBD.Domain.Entities.LiveClass;

namespace ShilpoHubBD.Application.Services.Learning;

public class LiveClassService : ILiveClassService
{
    private readonly ILiveClassRepository _liveClassRepository;
    private readonly IMentorRepository _mentorRepository;
    private readonly IAcademyMemberProfileRepository _academyMemberProfileRepository;
    private readonly ILiveClassNotifier _notifier;

    public LiveClassService(
        ILiveClassRepository liveClassRepository,
        IMentorRepository mentorRepository,
        IAcademyMemberProfileRepository academyMemberProfileRepository,
        ILiveClassNotifier notifier)
    {
        _liveClassRepository = liveClassRepository;
        _mentorRepository = mentorRepository;
        _academyMemberProfileRepository = academyMemberProfileRepository;
        _notifier = notifier;
    }

    public async Task<LiveClassDto> CreateAsync(Guid userId, CreateLiveClassRequest request, CancellationToken cancellationToken)
    {
        await EnsureCanInstructAsync(userId, cancellationToken);

        var now = DateTime.UtcNow;
        var liveClass = new LiveClass
        {
            Id = Guid.NewGuid(),
            InstructorUserId = userId,
            CourseId = request.CourseId,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            MeetingUrl = request.MeetingUrl?.Trim(),
            MaxParticipants = request.MaxParticipants,
            Status = LiveClassStatus.Scheduled,
            ScheduledStartAt = request.ScheduledStartAt,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await _liveClassRepository.AddAsync(liveClass, cancellationToken);
        await _liveClassRepository.SaveChangesAsync(cancellationToken);

        var created = await _liveClassRepository.GetByIdAsync(liveClass.Id, cancellationToken);
        return ToDto(created!);
    }

    public async Task<LiveClassDto> UpdateAsync(Guid userId, Guid liveClassId, UpdateLiveClassRequest request, CancellationToken cancellationToken)
    {
        var liveClass = await GetOwnedLiveClassAsync(userId, liveClassId, cancellationToken);

        if (liveClass.Status != LiveClassStatus.Scheduled)
        {
            throw new ConflictException("Only a scheduled live class can be updated.");
        }

        liveClass.CourseId = request.CourseId;
        liveClass.Title = request.Title.Trim();
        liveClass.Description = request.Description.Trim();
        liveClass.MeetingUrl = request.MeetingUrl?.Trim();
        liveClass.MaxParticipants = request.MaxParticipants;
        liveClass.ScheduledStartAt = request.ScheduledStartAt;
        liveClass.UpdatedAt = DateTime.UtcNow;

        await _liveClassRepository.SaveChangesAsync(cancellationToken);
        return ToDto(liveClass);
    }

    public async Task<LiveClassDto> StartAsync(Guid userId, Guid liveClassId, CancellationToken cancellationToken)
    {
        var liveClass = await GetOwnedLiveClassAsync(userId, liveClassId, cancellationToken);

        if (liveClass.Status != LiveClassStatus.Scheduled)
        {
            throw new ConflictException("Only a scheduled live class can be started.");
        }

        var now = DateTime.UtcNow;
        liveClass.Status = LiveClassStatus.Live;
        liveClass.StartedAt = now;
        liveClass.UpdatedAt = now;

        await _liveClassRepository.SaveChangesAsync(cancellationToken);
        var dto = ToDto(liveClass);
        await _notifier.NotifyStatusChangedAsync(liveClass.Id, dto, cancellationToken);
        return dto;
    }

    public async Task<LiveClassDto> EndAsync(Guid userId, Guid liveClassId, CancellationToken cancellationToken)
    {
        var liveClass = await GetOwnedLiveClassAsync(userId, liveClassId, cancellationToken);

        if (liveClass.Status != LiveClassStatus.Live)
        {
            throw new ConflictException("Only a live class in progress can be ended.");
        }

        var now = DateTime.UtcNow;
        liveClass.Status = LiveClassStatus.Ended;
        liveClass.EndedAt = now;
        liveClass.UpdatedAt = now;

        foreach (var attendance in liveClass.Attendances.Where(a => a.LeftAt == null))
        {
            attendance.LeftAt = now;
        }

        await _liveClassRepository.SaveChangesAsync(cancellationToken);
        var dto = ToDto(liveClass);
        await _notifier.NotifyStatusChangedAsync(liveClass.Id, dto, cancellationToken);
        return dto;
    }

    public async Task<LiveClassDto> CancelAsync(Guid userId, Guid liveClassId, CancellationToken cancellationToken)
    {
        var liveClass = await GetOwnedLiveClassAsync(userId, liveClassId, cancellationToken);

        if (liveClass.Status != LiveClassStatus.Scheduled)
        {
            throw new ConflictException("Only a scheduled live class can be cancelled.");
        }

        liveClass.Status = LiveClassStatus.Cancelled;
        liveClass.UpdatedAt = DateTime.UtcNow;

        await _liveClassRepository.SaveChangesAsync(cancellationToken);
        var dto = ToDto(liveClass);
        await _notifier.NotifyStatusChangedAsync(liveClass.Id, dto, cancellationToken);
        return dto;
    }

    public async Task DeleteAsync(Guid userId, Guid liveClassId, CancellationToken cancellationToken)
    {
        var liveClass = await GetOwnedLiveClassAsync(userId, liveClassId, cancellationToken);

        if (liveClass.Status != LiveClassStatus.Scheduled)
        {
            throw new ConflictException("Only a scheduled live class can be deleted. Cancel it instead.");
        }

        _liveClassRepository.Remove(liveClass);
        await _liveClassRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task<LiveClassDto> GetByIdAsync(Guid liveClassId, CancellationToken cancellationToken)
    {
        var liveClass = await _liveClassRepository.GetByIdAsync(liveClassId, cancellationToken)
            ?? throw new NotFoundException("Live class not found.");

        return ToDto(liveClass);
    }

    public async Task<PagedResult<LiveClassListItemDto>> GetPagedAsync(LiveClassQueryParameters query, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _liveClassRepository.GetPagedAsync(query, cancellationToken);
        return new PagedResult<LiveClassListItemDto>
        {
            Items = items.Select(ToListItemDto).ToList(),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }

    public async Task<List<LiveClassListItemDto>> GetMineAsync(Guid userId, CancellationToken cancellationToken)
    {
        var liveClasses = await _liveClassRepository.GetByInstructorAsync(userId, cancellationToken);
        return liveClasses.Select(ToListItemDto).ToList();
    }

    public async Task<List<LiveClassListItemDto>> GetRegisteredAsync(Guid userId, CancellationToken cancellationToken)
    {
        var liveClasses = await _liveClassRepository.GetMyRegisteredAsync(userId, cancellationToken);
        return liveClasses.Select(ToListItemDto).ToList();
    }

    public async Task<LiveClassParticipantDto> RegisterAsync(Guid userId, Guid liveClassId, CancellationToken cancellationToken)
    {
        var liveClass = await _liveClassRepository.GetByIdAsync(liveClassId, cancellationToken)
            ?? throw new NotFoundException("Live class not found.");

        if (liveClass.Status is not (LiveClassStatus.Scheduled or LiveClassStatus.Live))
        {
            throw new ConflictException("This live class is no longer open for registration.");
        }

        if (await _liveClassRepository.GetParticipantAsync(liveClassId, userId, cancellationToken) is not null)
        {
            throw new ConflictException("You are already registered for this live class.");
        }

        if (liveClass.MaxParticipants.HasValue && liveClass.Participants.Count >= liveClass.MaxParticipants.Value)
        {
            throw new ConflictException("This live class has reached its maximum number of participants.");
        }

        var participant = new LiveClassParticipant
        {
            Id = Guid.NewGuid(),
            LiveClassId = liveClassId,
            UserId = userId,
            RegisteredAt = DateTime.UtcNow,
        };

        await _liveClassRepository.AddParticipantAsync(participant, cancellationToken);
        await _liveClassRepository.SaveChangesAsync(cancellationToken);

        var reloaded = await _liveClassRepository.GetParticipantAsync(liveClassId, userId, cancellationToken);
        var dto = ToParticipantDto(reloaded ?? participant);
        await _notifier.NotifyParticipantJoinedAsync(liveClassId, dto, cancellationToken);
        return dto;
    }

    public async Task JoinAsync(Guid userId, Guid liveClassId, CancellationToken cancellationToken)
    {
        var liveClass = await _liveClassRepository.GetByIdAsync(liveClassId, cancellationToken)
            ?? throw new NotFoundException("Live class not found.");

        var isInstructor = liveClass.InstructorUserId == userId;
        var isRegistered = liveClass.Participants.Any(p => p.UserId == userId);
        if (!isInstructor && !isRegistered)
        {
            throw new ConflictException("You must register for this live class before joining it.");
        }

        if (await _liveClassRepository.GetOpenAttendanceAsync(liveClassId, userId, cancellationToken) is not null)
        {
            return;
        }

        var attendance = new LiveClassAttendance
        {
            Id = Guid.NewGuid(),
            LiveClassId = liveClassId,
            UserId = userId,
            JoinedAt = DateTime.UtcNow,
        };

        await _liveClassRepository.AddAttendanceAsync(attendance, cancellationToken);
        await _liveClassRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task LeaveAsync(Guid userId, Guid liveClassId, CancellationToken cancellationToken)
    {
        var attendance = await _liveClassRepository.GetOpenAttendanceAsync(liveClassId, userId, cancellationToken);
        if (attendance is null)
        {
            return;
        }

        attendance.LeftAt = DateTime.UtcNow;
        await _liveClassRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<LiveClassAttendanceDto>> GetAttendanceAsync(Guid userId, Guid liveClassId, CancellationToken cancellationToken)
    {
        await GetOwnedLiveClassAsync(userId, liveClassId, cancellationToken);

        var attendance = await _liveClassRepository.GetAttendanceAsync(liveClassId, cancellationToken);
        return attendance.Select(ToAttendanceDto).ToList();
    }

    public async Task<LiveClassQuestionDto> AskQuestionAsync(
        Guid userId, Guid liveClassId, AskQuestionRequest request, CancellationToken cancellationToken)
    {
        var liveClass = await _liveClassRepository.GetByIdAsync(liveClassId, cancellationToken)
            ?? throw new NotFoundException("Live class not found.");

        if (liveClass.Status != LiveClassStatus.Live)
        {
            throw new ConflictException("Questions can only be asked while the class is live.");
        }

        var isInstructor = liveClass.InstructorUserId == userId;
        var isRegistered = liveClass.Participants.Any(p => p.UserId == userId);
        if (!isInstructor && !isRegistered)
        {
            throw new ConflictException("You must register for this live class before asking a question.");
        }

        var question = new LiveClassQuestion
        {
            Id = Guid.NewGuid(),
            LiveClassId = liveClassId,
            UserId = userId,
            Body = request.Body.Trim(),
            IsAnswered = false,
            CreatedAt = DateTime.UtcNow,
        };

        await _liveClassRepository.AddQuestionAsync(question, cancellationToken);
        await _liveClassRepository.SaveChangesAsync(cancellationToken);

        var created = await _liveClassRepository.GetQuestionByIdAsync(question.Id, cancellationToken);
        var dto = ToQuestionDto(created!);
        await _notifier.NotifyQuestionAskedAsync(liveClassId, dto, cancellationToken);
        return dto;
    }

    public async Task<LiveClassQuestionDto> AnswerQuestionAsync(
        Guid userId, Guid liveClassId, Guid questionId, AnswerQuestionRequest request, CancellationToken cancellationToken)
    {
        await GetOwnedLiveClassAsync(userId, liveClassId, cancellationToken);

        var question = await _liveClassRepository.GetQuestionByIdAsync(questionId, cancellationToken)
            ?? throw new NotFoundException("Question not found.");

        if (question.LiveClassId != liveClassId)
        {
            throw new NotFoundException("Question not found.");
        }

        question.AnswerBody = request.AnswerBody.Trim();
        question.IsAnswered = true;
        question.AnsweredAt = DateTime.UtcNow;

        await _liveClassRepository.SaveChangesAsync(cancellationToken);

        var dto = ToQuestionDto(question);
        await _notifier.NotifyQuestionAnsweredAsync(liveClassId, dto, cancellationToken);
        return dto;
    }

    private async Task EnsureCanInstructAsync(Guid userId, CancellationToken cancellationToken)
    {
        var mentor = await _mentorRepository.GetByUserIdAsync(userId, cancellationToken);
        if (mentor is not null)
        {
            return;
        }

        var trainerProfile = await _academyMemberProfileRepository.GetByUserIdAsync(userId, cancellationToken);
        if (trainerProfile is not null && trainerProfile.Role == AcademyMemberRole.Trainer)
        {
            return;
        }

        throw new ConflictException("You must have a mentor profile or a trainer academy profile before hosting live classes.");
    }

    private async Task<LiveClass> GetOwnedLiveClassAsync(Guid userId, Guid liveClassId, CancellationToken cancellationToken)
    {
        var liveClass = await _liveClassRepository.GetByIdAsync(liveClassId, cancellationToken)
            ?? throw new NotFoundException("Live class not found.");

        if (liveClass.InstructorUserId != userId)
        {
            throw new UnauthorizedAccessException("You do not have permission to manage this live class.");
        }

        return liveClass;
    }

    private static LiveClassListItemDto ToListItemDto(LiveClass liveClass) => new()
    {
        Id = liveClass.Id,
        InstructorName = liveClass.Instructor.FullName,
        Title = liveClass.Title,
        Status = liveClass.Status.ToString(),
        MaxParticipants = liveClass.MaxParticipants,
        ParticipantCount = liveClass.Participants.Count,
        ScheduledStartAt = liveClass.ScheduledStartAt,
    };

    private static LiveClassDto ToDto(LiveClass liveClass) => new()
    {
        Id = liveClass.Id,
        InstructorUserId = liveClass.InstructorUserId,
        InstructorName = liveClass.Instructor.FullName,
        CourseId = liveClass.CourseId,
        CourseTitle = liveClass.Course?.Title,
        Title = liveClass.Title,
        Description = liveClass.Description,
        MeetingUrl = liveClass.MeetingUrl,
        MaxParticipants = liveClass.MaxParticipants,
        ParticipantCount = liveClass.Participants.Count,
        Status = liveClass.Status.ToString(),
        ScheduledStartAt = liveClass.ScheduledStartAt,
        StartedAt = liveClass.StartedAt,
        EndedAt = liveClass.EndedAt,
        CreatedAt = liveClass.CreatedAt,
        UpdatedAt = liveClass.UpdatedAt,
        Participants = liveClass.Participants.OrderBy(p => p.RegisteredAt).Select(ToParticipantDto).ToList(),
        Questions = liveClass.Questions.OrderByDescending(q => q.CreatedAt).Select(ToQuestionDto).ToList(),
    };

    private static LiveClassParticipantDto ToParticipantDto(LiveClassParticipant participant) => new()
    {
        Id = participant.Id,
        UserId = participant.UserId,
        UserName = participant.User.FullName,
        RegisteredAt = participant.RegisteredAt,
    };

    private static LiveClassQuestionDto ToQuestionDto(LiveClassQuestion question) => new()
    {
        Id = question.Id,
        LiveClassId = question.LiveClassId,
        UserId = question.UserId,
        UserName = question.User.FullName,
        Body = question.Body,
        IsAnswered = question.IsAnswered,
        AnswerBody = question.AnswerBody,
        CreatedAt = question.CreatedAt,
        AnsweredAt = question.AnsweredAt,
    };

    private static LiveClassAttendanceDto ToAttendanceDto(LiveClassAttendance attendance) => new()
    {
        Id = attendance.Id,
        UserId = attendance.UserId,
        UserName = attendance.User.FullName,
        JoinedAt = attendance.JoinedAt,
        LeftAt = attendance.LeftAt,
    };
}
