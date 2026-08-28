using ShilpoHubBD.Application.DTOs.Apprenticeship;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Apprenticeship;
using ShilpoHubBD.Domain.Entities.Learning;

namespace ShilpoHubBD.Application.Services.Apprenticeship;

public class ApprenticeshipProgramService : IApprenticeshipProgramService
{
    private readonly IApprenticeshipProgramRepository _programRepository;
    private readonly IMentorRepository _mentorRepository;
    private readonly IAcademyMemberProfileRepository _academyMemberProfileRepository;
    private readonly IHeritageSkillRepository _heritageSkillRepository;

    public ApprenticeshipProgramService(
        IApprenticeshipProgramRepository programRepository,
        IMentorRepository mentorRepository,
        IAcademyMemberProfileRepository academyMemberProfileRepository,
        IHeritageSkillRepository heritageSkillRepository)
    {
        _programRepository = programRepository;
        _mentorRepository = mentorRepository;
        _academyMemberProfileRepository = academyMemberProfileRepository;
        _heritageSkillRepository = heritageSkillRepository;
    }

    public async Task<ApprenticeshipProgramDto> CreateAsync(Guid userId, CreateApprenticeshipProgramRequest request, CancellationToken cancellationToken)
    {
        var (mentor, trainer) = await ResolveProviderAsync(userId, cancellationToken);
        await EnsureHeritageSkillExistsAsync(request.HeritageSkillId, cancellationToken);

        if (!Enum.TryParse<ProgramType>(request.Type, true, out var type))
        {
            throw new ConflictException("Type must be either 'Internship' or 'Apprenticeship'.");
        }

        var now = DateTime.UtcNow;
        var program = new ApprenticeshipProgram
        {
            Id = Guid.NewGuid(),
            MentorId = mentor?.Id,
            TrainerProfileId = trainer?.Id,
            Type = type,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            HeritageSkillId = request.HeritageSkillId,
            Location = request.Location?.Trim(),
            DurationWeeks = request.DurationWeeks,
            Capacity = request.Capacity,
            EligibilityRequirements = request.EligibilityRequirements.Trim(),
            Status = ProgramStatus.Draft,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await _programRepository.AddAsync(program, cancellationToken);
        await _programRepository.SaveChangesAsync(cancellationToken);

        var created = await _programRepository.GetByIdAsync(program.Id, cancellationToken);
        return ToDto(created!);
    }

    public async Task<ApprenticeshipProgramDto> UpdateAsync(Guid userId, Guid programId, UpdateApprenticeshipProgramRequest request, CancellationToken cancellationToken)
    {
        var program = await GetOwnedProgramAsync(userId, programId, cancellationToken);
        await EnsureHeritageSkillExistsAsync(request.HeritageSkillId, cancellationToken);

        program.Title = request.Title.Trim();
        program.Description = request.Description.Trim();
        program.HeritageSkillId = request.HeritageSkillId;
        program.Location = request.Location?.Trim();
        program.DurationWeeks = request.DurationWeeks;
        program.Capacity = request.Capacity;
        program.EligibilityRequirements = request.EligibilityRequirements.Trim();
        program.StartDate = request.StartDate;
        program.EndDate = request.EndDate;
        program.UpdatedAt = DateTime.UtcNow;

        await _programRepository.SaveChangesAsync(cancellationToken);
        return ToDto(program);
    }

    public async Task<ApprenticeshipProgramDto> PublishAsync(Guid userId, Guid programId, CancellationToken cancellationToken)
    {
        var program = await GetOwnedProgramAsync(userId, programId, cancellationToken);

        if (program.Status == ProgramStatus.Published)
        {
            throw new ConflictException("This program is already published.");
        }

        if (program.Status != ProgramStatus.Draft)
        {
            throw new ConflictException("Only a draft program can be published.");
        }

        var now = DateTime.UtcNow;
        program.Status = ProgramStatus.Published;
        program.PublishedAt = now;
        program.UpdatedAt = now;

        await _programRepository.SaveChangesAsync(cancellationToken);
        return ToDto(program);
    }

    public async Task<ApprenticeshipProgramDto> CloseAsync(Guid userId, Guid programId, CancellationToken cancellationToken)
    {
        var program = await GetOwnedProgramAsync(userId, programId, cancellationToken);

        if (program.Status != ProgramStatus.Published)
        {
            throw new ConflictException("Only a published program can be closed.");
        }

        program.Status = ProgramStatus.Closed;
        program.UpdatedAt = DateTime.UtcNow;

        await _programRepository.SaveChangesAsync(cancellationToken);
        return ToDto(program);
    }

    public async Task DeleteAsync(Guid userId, Guid programId, CancellationToken cancellationToken)
    {
        var program = await GetOwnedProgramAsync(userId, programId, cancellationToken);

        if (program.Status != ProgramStatus.Draft)
        {
            throw new ConflictException("Only draft programs can be deleted. Close it instead.");
        }

        _programRepository.Remove(program);
        await _programRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task<ApprenticeshipProgramDto> GetByIdAsync(Guid programId, Guid? currentUserId, CancellationToken cancellationToken)
    {
        var program = await _programRepository.GetByIdAsync(programId, cancellationToken)
            ?? throw new NotFoundException("Program not found.");

        var isOwner = program.Mentor?.UserId == currentUserId || program.TrainerProfile?.UserId == currentUserId;
        if (program.Status == ProgramStatus.Draft && !isOwner)
        {
            throw new NotFoundException("Program not found.");
        }

        return ToDto(program);
    }

    public async Task<PagedResult<ApprenticeshipProgramListItemDto>> GetPublishedAsync(ApprenticeshipProgramQueryParameters query, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _programRepository.GetPagedAsync(query, cancellationToken);
        return new PagedResult<ApprenticeshipProgramListItemDto>
        {
            Items = items.Select(ToListItemDto).ToList(),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }

    public async Task<List<ApprenticeshipProgramListItemDto>> GetMineAsync(Guid userId, CancellationToken cancellationToken)
    {
        var mentor = await _mentorRepository.GetByUserIdAsync(userId, cancellationToken);
        if (mentor is not null)
        {
            var mentorPrograms = await _programRepository.GetByMentorAsync(mentor.Id, cancellationToken);
            return mentorPrograms.Select(ToListItemDto).ToList();
        }

        var trainerProfile = await _academyMemberProfileRepository.GetByUserIdAsync(userId, cancellationToken);
        if (trainerProfile is not null && trainerProfile.Role == AcademyMemberRole.Trainer)
        {
            var trainerPrograms = await _programRepository.GetByTrainerProfileAsync(trainerProfile.Id, cancellationToken);
            return trainerPrograms.Select(ToListItemDto).ToList();
        }

        throw new ConflictException("You must have a mentor profile or a trainer academy profile before managing programs.");
    }

    public async Task<TrainingMilestoneDto> AddMilestoneAsync(Guid userId, Guid programId, CreateTrainingMilestoneRequest request, CancellationToken cancellationToken)
    {
        var program = await GetOwnedProgramAsync(userId, programId, cancellationToken);

        var milestone = new TrainingMilestone
        {
            Id = Guid.NewGuid(),
            ProgramId = program.Id,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            DisplayOrder = request.DisplayOrder,
            CreatedAt = DateTime.UtcNow,
        };

        await _programRepository.AddMilestoneAsync(milestone, cancellationToken);
        program.UpdatedAt = DateTime.UtcNow;
        await _programRepository.SaveChangesAsync(cancellationToken);

        return ToMilestoneDto(milestone);
    }

    public async Task<TrainingMilestoneDto> UpdateMilestoneAsync(
        Guid userId, Guid programId, Guid milestoneId, UpdateTrainingMilestoneRequest request, CancellationToken cancellationToken)
    {
        await GetOwnedProgramAsync(userId, programId, cancellationToken);

        var milestone = await _programRepository.GetMilestoneByIdAsync(milestoneId, cancellationToken)
            ?? throw new NotFoundException("Milestone not found.");

        if (milestone.ProgramId != programId)
        {
            throw new NotFoundException("Milestone not found.");
        }

        milestone.Title = request.Title.Trim();
        milestone.Description = request.Description.Trim();
        milestone.DisplayOrder = request.DisplayOrder;

        await _programRepository.SaveChangesAsync(cancellationToken);
        return ToMilestoneDto(milestone);
    }

    public async Task DeleteMilestoneAsync(Guid userId, Guid programId, Guid milestoneId, CancellationToken cancellationToken)
    {
        await GetOwnedProgramAsync(userId, programId, cancellationToken);

        var milestone = await _programRepository.GetMilestoneByIdAsync(milestoneId, cancellationToken)
            ?? throw new NotFoundException("Milestone not found.");

        if (milestone.ProgramId != programId)
        {
            throw new NotFoundException("Milestone not found.");
        }

        _programRepository.RemoveMilestone(milestone);
        await _programRepository.SaveChangesAsync(cancellationToken);
    }

    private async Task<(MentorProfile? Mentor, AcademyMemberProfile? Trainer)> ResolveProviderAsync(Guid userId, CancellationToken cancellationToken)
    {
        var mentor = await _mentorRepository.GetByUserIdAsync(userId, cancellationToken);
        if (mentor is not null)
        {
            return (mentor, null);
        }

        var trainerProfile = await _academyMemberProfileRepository.GetByUserIdAsync(userId, cancellationToken);
        if (trainerProfile is not null && trainerProfile.Role == AcademyMemberRole.Trainer)
        {
            return (null, trainerProfile);
        }

        throw new ConflictException("You must have a mentor profile or a trainer academy profile before managing programs.");
    }

    private async Task EnsureHeritageSkillExistsAsync(Guid? heritageSkillId, CancellationToken cancellationToken)
    {
        if (heritageSkillId.HasValue && await _heritageSkillRepository.GetByIdAsync(heritageSkillId.Value, cancellationToken) is null)
        {
            throw new NotFoundException("Heritage skill not found.");
        }
    }

    private async Task<ApprenticeshipProgram> GetOwnedProgramAsync(Guid userId, Guid programId, CancellationToken cancellationToken)
    {
        var program = await _programRepository.GetByIdAsync(programId, cancellationToken)
            ?? throw new NotFoundException("Program not found.");

        var isOwner = program.Mentor?.UserId == userId || program.TrainerProfile?.UserId == userId;
        if (!isOwner)
        {
            throw new UnauthorizedAccessException("You do not have permission to manage this program.");
        }

        return program;
    }

    private static string ProviderNameOf(ApprenticeshipProgram program)
        => program.Mentor?.User.FullName ?? program.TrainerProfile?.User.FullName ?? string.Empty;

    private static ApprenticeshipProgramListItemDto ToListItemDto(ApprenticeshipProgram program) => new()
    {
        Id = program.Id,
        ProviderName = ProviderNameOf(program),
        Type = program.Type.ToString(),
        Title = program.Title,
        Location = program.Location,
        DurationWeeks = program.DurationWeeks,
        Capacity = program.Capacity,
        HeritageSkillName = program.HeritageSkill?.Name,
        Status = program.Status.ToString(),
        StartDate = program.StartDate,
        EndDate = program.EndDate,
        ActiveEnrollmentCount = program.Enrollments.Count(e => e.Status == ApprenticeEnrollmentStatus.Active),
        PublishedAt = program.PublishedAt,
    };

    private static ApprenticeshipProgramDto ToDto(ApprenticeshipProgram program) => new()
    {
        Id = program.Id,
        MentorId = program.MentorId,
        TrainerProfileId = program.TrainerProfileId,
        ProviderName = ProviderNameOf(program),
        Type = program.Type.ToString(),
        Title = program.Title,
        Description = program.Description,
        HeritageSkillId = program.HeritageSkillId,
        HeritageSkillName = program.HeritageSkill?.Name,
        Location = program.Location,
        DurationWeeks = program.DurationWeeks,
        Capacity = program.Capacity,
        EligibilityRequirements = program.EligibilityRequirements,
        Status = program.Status.ToString(),
        StartDate = program.StartDate,
        EndDate = program.EndDate,
        ActiveEnrollmentCount = program.Enrollments.Count(e => e.Status == ApprenticeEnrollmentStatus.Active),
        Milestones = program.Milestones.OrderBy(m => m.DisplayOrder).Select(ToMilestoneDto).ToList(),
        CreatedAt = program.CreatedAt,
        UpdatedAt = program.UpdatedAt,
        PublishedAt = program.PublishedAt,
    };

    private static TrainingMilestoneDto ToMilestoneDto(TrainingMilestone milestone) => new()
    {
        Id = milestone.Id,
        ProgramId = milestone.ProgramId,
        Title = milestone.Title,
        Description = milestone.Description,
        DisplayOrder = milestone.DisplayOrder,
    };
}
