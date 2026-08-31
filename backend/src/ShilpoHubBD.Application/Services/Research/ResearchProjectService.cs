using ShilpoHubBD.Application.Common;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Research;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Research;

namespace ShilpoHubBD.Application.Services.Research;

public class ResearchProjectService : IResearchProjectService
{
    private readonly IResearchProjectRepository _repository;
    private readonly IUserRepository _userRepository;

    public ResearchProjectService(IResearchProjectRepository repository, IUserRepository userRepository)
    {
        _repository = repository;
        _userRepository = userRepository;
    }

    public async Task<PagedResult<ResearchProjectListItemDto>> GetMineAsync(
        Guid userId, ResearchProjectQueryParameters query, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _repository.GetPagedForMemberAsync(userId, query, cancellationToken);

        return new PagedResult<ResearchProjectListItemDto>
        {
            Items = items.Select(p => ToListItemDto(p, userId)).ToList(),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }

    public async Task<ResearchProjectDetailDto> GetByIdAsync(Guid userId, Guid projectId, CancellationToken cancellationToken)
    {
        var project = await _repository.GetByIdWithMembersAsync(projectId, cancellationToken)
            ?? throw new NotFoundException("Research project not found.");

        var membership = project.Members.FirstOrDefault(m => m.UserId == userId);
        ResearchAccess.RequireReadAccess(project, membership);

        var stats = await _repository.GetStatsAsync(projectId, cancellationToken);
        return ToDetailDto(project, membership, stats);
    }

    public async Task<ResearchProjectDetailDto> CreateAsync(
        Guid userId, CreateResearchProjectRequest request, CancellationToken cancellationToken)
    {
        var owner = await _userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        var visibility = ParseVisibility(request.Visibility) ?? ResearchProjectVisibility.Private;
        ValidateDateRange(request.StartDate, request.EndDate);

        var now = DateTime.UtcNow;
        var project = new ResearchProject
        {
            Id = Guid.NewGuid(),
            OwnerUserId = userId,
            Slug = await GenerateUniqueSlugAsync(request.Title, cancellationToken),
            Title = request.Title.Trim(),
            Summary = request.Summary.Trim(),
            Description = request.Description?.Trim(),
            Discipline = request.Discipline?.Trim(),
            Institution = request.Institution?.Trim(),
            Status = ResearchProjectStatus.Planning,
            Visibility = visibility,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            CreatedAt = now,
            UpdatedAt = now,
        };

        project.Members.Add(new ResearchProjectMember
        {
            Id = Guid.NewGuid(),
            ResearchProjectId = project.Id,
            UserId = userId,
            Role = ResearchRole.Owner,
            JoinedAt = now,
        });

        await _repository.AddAsync(project, cancellationToken);
        await AddActivityAsync(project.Id, userId, ResearchActivityType.ProjectCreated,
            $"{owner.FullName} created the research project.", cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(userId, project.Id, cancellationToken);
    }

    public async Task<ResearchProjectDetailDto> UpdateAsync(
        Guid userId, Guid projectId, UpdateResearchProjectRequest request, CancellationToken cancellationToken)
    {
        var project = await _repository.GetByIdWithMembersAsync(projectId, cancellationToken)
            ?? throw new NotFoundException("Research project not found.");

        var membership = project.Members.FirstOrDefault(m => m.UserId == userId);
        ResearchAccess.RequireRole(membership, ResearchRole.Admin);

        var visibility = ParseVisibility(request.Visibility)
            ?? throw new ConflictException("Visibility must be one of: Private, Institutional, Public.");
        ValidateDateRange(request.StartDate, request.EndDate);

        project.Title = request.Title.Trim();
        project.Summary = request.Summary.Trim();
        project.Description = request.Description?.Trim();
        project.Discipline = request.Discipline?.Trim();
        project.Institution = request.Institution?.Trim();
        project.Visibility = visibility;
        project.StartDate = request.StartDate;
        project.EndDate = request.EndDate;
        project.UpdatedAt = DateTime.UtcNow;

        await AddActivityAsync(projectId, userId, ResearchActivityType.ProjectUpdated,
            $"{membership!.User?.FullName} updated the project details.", cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        var stats = await _repository.GetStatsAsync(projectId, cancellationToken);
        return ToDetailDto(project, membership, stats);
    }

    public async Task<ResearchProjectDetailDto> UpdateStatusAsync(
        Guid userId, Guid projectId, UpdateResearchProjectStatusRequest request, CancellationToken cancellationToken)
    {
        var project = await _repository.GetByIdWithMembersAsync(projectId, cancellationToken)
            ?? throw new NotFoundException("Research project not found.");

        var membership = project.Members.FirstOrDefault(m => m.UserId == userId);
        ResearchAccess.RequireRole(membership, ResearchRole.Admin);

        if (!Enum.TryParse<ResearchProjectStatus>(request.Status, true, out var status))
        {
            throw new ConflictException("Status must be one of: Planning, Active, OnHold, Completed, Archived.");
        }

        if (project.Status == status)
        {
            throw new ConflictException($"Project is already {status}.");
        }

        project.Status = status;
        project.UpdatedAt = DateTime.UtcNow;

        await AddActivityAsync(projectId, userId, ResearchActivityType.ProjectStatusChanged,
            $"{membership!.User?.FullName} changed the project status to {status}.", cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        var stats = await _repository.GetStatsAsync(projectId, cancellationToken);
        return ToDetailDto(project, membership, stats);
    }

    public async Task DeleteAsync(Guid userId, Guid projectId, CancellationToken cancellationToken)
    {
        var project = await _repository.GetByIdWithMembersAsync(projectId, cancellationToken)
            ?? throw new NotFoundException("Research project not found.");

        if (project.OwnerUserId != userId)
        {
            throw new UnauthorizedAccessException("Only the project owner can delete a research project.");
        }

        _repository.Remove(project);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<ResearchProjectMemberDto>> GetMembersAsync(
        Guid userId, Guid projectId, CancellationToken cancellationToken)
    {
        await LoadReadableProjectAsync(userId, projectId, cancellationToken);
        var members = await _repository.GetMembersAsync(projectId, cancellationToken);
        return members.Select(m => m.ToDto()).ToList();
    }

    public async Task<ResearchProjectMemberDto> AddMemberAsync(
        Guid userId, Guid projectId, AddResearchProjectMemberRequest request, CancellationToken cancellationToken)
    {
        var project = await _repository.GetByIdWithMembersAsync(projectId, cancellationToken)
            ?? throw new NotFoundException("Research project not found.");

        var callerMembership = project.Members.FirstOrDefault(m => m.UserId == userId);
        var caller = ResearchAccess.RequireRole(callerMembership, ResearchRole.Admin);

        var role = ParseRole(request.Role) ?? throw new ConflictException(
            "Role must be one of: Viewer, Contributor, Researcher, Admin.");

        if (role == ResearchRole.Owner)
        {
            throw new ConflictException("Ownership cannot be assigned through member management.");
        }

        if (role == ResearchRole.Admin && caller.Role != ResearchRole.Owner)
        {
            throw new UnauthorizedAccessException("Only the project owner can grant the Admin role.");
        }

        if (project.Members.Any(m => m.UserId == request.UserId))
        {
            throw new ConflictException("This user is already a member of the project.");
        }

        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        var now = DateTime.UtcNow;
        var member = new ResearchProjectMember
        {
            Id = Guid.NewGuid(),
            ResearchProjectId = projectId,
            UserId = user.Id,
            Role = role,
            InvitedByUserId = userId,
            JoinedAt = now,
        };

        await _repository.AddMemberAsync(member, cancellationToken);
        await AddActivityAsync(projectId, userId, ResearchActivityType.MemberAdded,
            $"{caller.User?.FullName} added {user.FullName} as {role}.", cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        member.User = user;
        return member.ToDto();
    }

    public async Task<ResearchProjectMemberDto> UpdateMemberRoleAsync(
        Guid userId, Guid projectId, Guid memberId, UpdateResearchMemberRoleRequest request, CancellationToken cancellationToken)
    {
        var project = await _repository.GetByIdWithMembersAsync(projectId, cancellationToken)
            ?? throw new NotFoundException("Research project not found.");

        var callerMembership = project.Members.FirstOrDefault(m => m.UserId == userId);
        var caller = ResearchAccess.RequireRole(callerMembership, ResearchRole.Admin);

        var member = await _repository.GetMemberByIdAsync(memberId, cancellationToken);
        if (member is null || member.ResearchProjectId != projectId)
        {
            throw new NotFoundException("Project member not found.");
        }

        var role = ParseRole(request.Role) ?? throw new ConflictException(
            "Role must be one of: Viewer, Contributor, Researcher, Admin.");

        if (member.UserId == project.OwnerUserId)
        {
            throw new ConflictException("The project owner's role cannot be changed.");
        }

        if (member.UserId == userId)
        {
            throw new ConflictException("You cannot change your own role.");
        }

        if (role == ResearchRole.Owner)
        {
            throw new ConflictException("Ownership cannot be assigned through member management.");
        }

        if ((role == ResearchRole.Admin || member.Role == ResearchRole.Admin) && caller.Role != ResearchRole.Owner)
        {
            throw new UnauthorizedAccessException("Only the project owner can grant or revoke the Admin role.");
        }

        member.Role = role;
        await AddActivityAsync(projectId, userId, ResearchActivityType.MemberRoleChanged,
            $"{caller.User?.FullName} changed {member.User?.FullName}'s role to {role}.", cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return member.ToDto();
    }

    public async Task RemoveMemberAsync(Guid userId, Guid projectId, Guid memberId, CancellationToken cancellationToken)
    {
        var project = await _repository.GetByIdWithMembersAsync(projectId, cancellationToken)
            ?? throw new NotFoundException("Research project not found.");

        var callerMembership = project.Members.FirstOrDefault(m => m.UserId == userId);
        var caller = ResearchAccess.RequireRole(callerMembership, ResearchRole.Admin);

        var member = await _repository.GetMemberByIdAsync(memberId, cancellationToken);
        if (member is null || member.ResearchProjectId != projectId)
        {
            throw new NotFoundException("Project member not found.");
        }

        if (member.UserId == project.OwnerUserId)
        {
            throw new ConflictException("The project owner cannot be removed.");
        }

        if (member.Role == ResearchRole.Admin && caller.Role != ResearchRole.Owner)
        {
            throw new UnauthorizedAccessException("Only the project owner can remove an Admin.");
        }

        _repository.RemoveMember(member);
        await AddActivityAsync(projectId, userId, ResearchActivityType.MemberRemoved,
            $"{caller.User?.FullName} removed {member.User?.FullName} from the project.", cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<ResearchActivityDto>> GetActivityAsync(
        Guid userId, Guid projectId, int take, CancellationToken cancellationToken)
    {
        await LoadReadableProjectAsync(userId, projectId, cancellationToken);
        take = Math.Clamp(take, 1, 200);
        var activities = await _repository.GetActivitiesAsync(projectId, take, cancellationToken);
        return activities.Select(a => a.ToDto()).ToList();
    }

    // ---- helpers ----------------------------------------------------------

    private async Task<ResearchProject> LoadReadableProjectAsync(Guid userId, Guid projectId, CancellationToken cancellationToken)
    {
        var project = await _repository.GetByIdWithMembersAsync(projectId, cancellationToken)
            ?? throw new NotFoundException("Research project not found.");

        var membership = project.Members.FirstOrDefault(m => m.UserId == userId);
        ResearchAccess.RequireReadAccess(project, membership);
        return project;
    }

    private async Task AddActivityAsync(
        Guid projectId, Guid actorUserId, ResearchActivityType type, string summary, CancellationToken cancellationToken)
    {
        await _repository.AddActivityAsync(new ResearchActivity
        {
            Id = Guid.NewGuid(),
            ResearchProjectId = projectId,
            ActorUserId = actorUserId,
            Type = type,
            Summary = summary.Length > 500 ? summary[..500] : summary,
            CreatedAt = DateTime.UtcNow,
        }, cancellationToken);
    }

    private async Task<string> GenerateUniqueSlugAsync(string title, CancellationToken cancellationToken)
    {
        var baseSlug = SlugGenerator.Generate(title);
        var slug = baseSlug;
        var suffix = 1;

        while (await _repository.SlugExistsAsync(slug, cancellationToken))
        {
            suffix++;
            slug = $"{baseSlug}-{suffix}";
        }

        return slug;
    }

    private static void ValidateDateRange(DateTime? start, DateTime? end)
    {
        if (start.HasValue && end.HasValue && end.Value < start.Value)
        {
            throw new ConflictException("End date cannot be earlier than the start date.");
        }
    }

    private static ResearchProjectVisibility? ParseVisibility(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return Enum.TryParse<ResearchProjectVisibility>(value, true, out var parsed) ? parsed : null;
    }

    private static ResearchRole? ParseRole(string? value)
        => Enum.TryParse<ResearchRole>(value, true, out var parsed) ? parsed : null;

    private static ResearchProjectListItemDto ToListItemDto(ResearchProject project, Guid userId)
    {
        var myRole = project.Members.FirstOrDefault(m => m.UserId == userId)?.Role;
        return new ResearchProjectListItemDto
        {
            Id = project.Id,
            Slug = project.Slug,
            Title = project.Title,
            Summary = project.Summary,
            Discipline = project.Discipline,
            Status = project.Status.ToString(),
            Visibility = project.Visibility.ToString(),
            OwnerName = project.Owner?.FullName ?? string.Empty,
            MyRole = myRole?.ToString() ?? string.Empty,
            MemberCount = project.Members.Count,
            OpenTaskCount = project.Tasks.Count(t =>
                t.Status != ResearchTaskStatus.Done && t.Status != ResearchTaskStatus.Cancelled),
            StartDate = project.StartDate,
            EndDate = project.EndDate,
            CreatedAt = project.CreatedAt,
            UpdatedAt = project.UpdatedAt,
        };
    }

    private static ResearchProjectDetailDto ToDetailDto(
        ResearchProject project, ResearchProjectMember? membership, ResearchProjectStats stats) => new()
    {
        Id = project.Id,
        Slug = project.Slug,
        Title = project.Title,
        Summary = project.Summary,
        Description = project.Description,
        Discipline = project.Discipline,
        Institution = project.Institution,
        Status = project.Status.ToString(),
        Visibility = project.Visibility.ToString(),
        OwnerUserId = project.OwnerUserId,
        OwnerName = project.Owner?.FullName ?? string.Empty,
        MyRole = membership?.Role.ToString() ?? string.Empty,
        StartDate = project.StartDate,
        EndDate = project.EndDate,
        CreatedAt = project.CreatedAt,
        UpdatedAt = project.UpdatedAt,
        Members = project.Members
            .OrderByDescending(m => m.Role)
            .ThenBy(m => m.JoinedAt)
            .Select(m => m.ToDto())
            .ToList(),
        TaskCount = stats.TaskCount,
        OpenTaskCount = stats.OpenTaskCount,
        MilestoneCount = stats.MilestoneCount,
        NoteCount = stats.NoteCount,
        PaperCount = stats.PaperCount,
        PublicationCount = stats.PublicationCount,
    };
}
