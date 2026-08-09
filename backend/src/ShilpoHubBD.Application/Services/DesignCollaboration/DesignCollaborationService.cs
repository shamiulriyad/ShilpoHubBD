using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.DesignCollaboration;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;
using ShilpoHubBD.Domain.Entities.DesignCollaboration;

namespace ShilpoHubBD.Application.Services.DesignCollaboration;

public class DesignCollaborationService : IDesignCollaborationService
{
    private readonly IDesignCollaborationRepository _repository;
    private readonly IUserRepository _userRepository;

    public DesignCollaborationService(IDesignCollaborationRepository repository, IUserRepository userRepository)
    {
        _repository = repository;
        _userRepository = userRepository;
    }

    public async Task<ProjectDto> CreateAsync(Guid businessPartnerId, CreateProjectRequest request, CancellationToken cancellationToken)
    {
        var producer = await _userRepository.GetByIdWithRolesAsync(request.ProducerId, cancellationToken);
        if (producer is null || !producer.UserRoles.Any(ur => ur.Role.Name == RoleNames.Producer))
        {
            throw new NotFoundException("Producer not found.");
        }

        var now = DateTime.UtcNow;
        var project = new DesignCollaborationProject
        {
            Id = Guid.NewGuid(),
            BusinessPartnerId = businessPartnerId,
            ProducerId = request.ProducerId,
            ReferenceNumber = await GenerateUniqueReferenceNumberAsync(cancellationToken),
            Title = request.Title.Trim(),
            DesignRequirements = request.DesignRequirements.Trim(),
            Status = CollaborationStatus.Invited,
            CreatedAt = now,
            UpdatedAt = now,
        };

        foreach (var file in request.InitialFiles)
        {
            project.Files.Add(new DesignFile
            {
                Id = Guid.NewGuid(),
                FileName = file.FileName.Trim(),
                FileUrl = file.FileUrl.Trim(),
                FileType = file.FileType.Trim(),
                UploadedByUserId = businessPartnerId,
                UploadedAt = now,
            });
        }

        project.StatusHistory.Add(new CollaborationStatusEvent
        {
            Id = Guid.NewGuid(),
            Status = CollaborationStatus.Invited,
            Note = "Producer invited to collaborate.",
            CreatedAt = now,
        });

        await _repository.AddAsync(project, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        var created = await _repository.GetByIdWithDetailsAsync(project.Id, cancellationToken)
            ?? throw new NotFoundException("Design collaboration project not found.");
        return ToDto(created);
    }

    public async Task<PagedResult<ProjectListItemDto>> GetForBusinessPartnerAsync(
        Guid businessPartnerId, bool isAdmin, ProjectQueryParameters parameters, CancellationToken cancellationToken)
    {
        var (items, totalCount) = isAdmin
            ? await _repository.GetPagedAllAsync(parameters, cancellationToken)
            : await _repository.GetPagedForBusinessPartnerAsync(businessPartnerId, parameters, cancellationToken);

        return ToPagedListDto(items, totalCount, parameters);
    }

    public async Task<PagedResult<ProjectListItemDto>> GetForProducerAsync(
        Guid producerId, ProjectQueryParameters parameters, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _repository.GetPagedForProducerAsync(producerId, parameters, cancellationToken);
        return ToPagedListDto(items, totalCount, parameters);
    }

    public async Task<ProjectDto> GetByIdAsync(Guid id, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken)
    {
        var project = await GetPartyAsync(id, currentUserId, isAdmin, cancellationToken);
        return ToDto(project);
    }

    public async Task<ProjectDto> RespondAsync(Guid id, Guid producerId, CollaborationResponseRequest request, CancellationToken cancellationToken)
    {
        var project = await _repository.GetByIdWithDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException("Design collaboration project not found.");

        if (project.ProducerId != producerId)
        {
            throw new UnauthorizedAccessException("You do not have permission to respond to this project.");
        }

        if (project.Status != CollaborationStatus.Invited)
        {
            throw new ConflictException("Only an invited project can be responded to.");
        }

        var now = DateTime.UtcNow;
        var newStatus = request.Accept ? CollaborationStatus.Active : CollaborationStatus.Declined;

        project.Status = newStatus;
        project.RespondedAt = now;
        project.UpdatedAt = now;
        project.StatusHistory.Add(new CollaborationStatusEvent
        {
            Id = Guid.NewGuid(),
            Status = newStatus,
            CreatedAt = now,
        });

        await _repository.SaveChangesAsync(cancellationToken);
        return ToDto(project);
    }

    public async Task<DesignCommentDto> AddCommentAsync(
        Guid id, Guid currentUserId, bool isAdmin, AddCommentRequest request, CancellationToken cancellationToken)
    {
        var project = await GetPartyAsync(id, currentUserId, isAdmin, cancellationToken);

        var author = await _userRepository.GetByIdAsync(currentUserId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        var comment = new DesignComment
        {
            Id = Guid.NewGuid(),
            AuthorUserId = currentUserId,
            Content = request.Content.Trim(),
            CreatedAt = DateTime.UtcNow,
        };

        project.Comments.Add(comment);
        project.UpdatedAt = DateTime.UtcNow;

        await _repository.SaveChangesAsync(cancellationToken);

        return new DesignCommentDto
        {
            Id = comment.Id,
            AuthorUserId = currentUserId,
            AuthorName = author.FullName,
            Content = comment.Content,
            CreatedAt = comment.CreatedAt,
        };
    }

    public async Task<DesignFileDto> AddFileAsync(
        Guid id, Guid currentUserId, bool isAdmin, DesignFileInput request, CancellationToken cancellationToken)
    {
        var project = await GetPartyAsync(id, currentUserId, isAdmin, cancellationToken);

        if (project.Status is not (CollaborationStatus.Invited or CollaborationStatus.Active))
        {
            throw new ConflictException("Files can only be added to an invited or active project.");
        }

        var uploader = await _userRepository.GetByIdAsync(currentUserId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        var file = new DesignFile
        {
            Id = Guid.NewGuid(),
            FileName = request.FileName.Trim(),
            FileUrl = request.FileUrl.Trim(),
            FileType = request.FileType.Trim(),
            UploadedByUserId = currentUserId,
            UploadedAt = DateTime.UtcNow,
        };

        project.Files.Add(file);
        project.UpdatedAt = DateTime.UtcNow;

        await _repository.SaveChangesAsync(cancellationToken);

        return new DesignFileDto
        {
            Id = file.Id,
            RevisionId = file.RevisionId,
            FileName = file.FileName,
            FileUrl = file.FileUrl,
            FileType = file.FileType,
            UploadedByUserId = currentUserId,
            UploadedByName = uploader.FullName,
            UploadedAt = file.UploadedAt,
        };
    }

    public async Task<DesignRevisionDto> SubmitRevisionAsync(
        Guid id, Guid producerId, SubmitRevisionRequest request, CancellationToken cancellationToken)
    {
        var project = await _repository.GetByIdWithDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException("Design collaboration project not found.");

        if (project.ProducerId != producerId)
        {
            throw new UnauthorizedAccessException("You do not have permission to submit a revision for this project.");
        }

        if (project.Status != CollaborationStatus.Active)
        {
            throw new ConflictException("Revisions can only be submitted to an active project.");
        }

        var now = DateTime.UtcNow;
        var revision = new DesignRevision
        {
            Id = Guid.NewGuid(),
            RevisionNumber = project.Revisions.Count + 1,
            Description = request.Description.Trim(),
            Status = RevisionStatus.Pending,
            SubmittedByUserId = producerId,
            SubmittedAt = now,
        };

        foreach (var file in request.Files)
        {
            revision.Files.Add(new DesignFile
            {
                Id = Guid.NewGuid(),
                FileName = file.FileName.Trim(),
                FileUrl = file.FileUrl.Trim(),
                FileType = file.FileType.Trim(),
                UploadedByUserId = producerId,
                UploadedAt = now,
            });
        }

        project.Revisions.Add(revision);
        project.UpdatedAt = now;

        await _repository.SaveChangesAsync(cancellationToken);

        revision.SubmittedBy = project.Producer;
        return ToRevisionDto(revision);
    }

    public async Task<DesignRevisionDto> DecideRevisionAsync(
        Guid id, Guid revisionId, Guid businessPartnerId, bool isAdmin, RevisionDecisionRequest request, CancellationToken cancellationToken)
    {
        var project = await _repository.GetByIdWithDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException("Design collaboration project not found.");

        if (!isAdmin && project.BusinessPartnerId != businessPartnerId)
        {
            throw new UnauthorizedAccessException("You do not have permission to decide on this revision.");
        }

        var revision = project.Revisions.FirstOrDefault(r => r.Id == revisionId)
            ?? throw new NotFoundException("Revision not found.");

        if (revision.Status != RevisionStatus.Pending)
        {
            throw new ConflictException("This revision has already been decided.");
        }

        var now = DateTime.UtcNow;
        revision.Status = request.Status;
        revision.DecidedAt = now;
        revision.DecisionNotes = string.IsNullOrWhiteSpace(request.DecisionNotes) ? null : request.DecisionNotes.Trim();
        project.UpdatedAt = now;

        await _repository.SaveChangesAsync(cancellationToken);
        return ToRevisionDto(revision);
    }

    public async Task<ProjectDto> CompleteAsync(Guid id, Guid businessPartnerId, bool isAdmin, CancellationToken cancellationToken)
    {
        var project = await _repository.GetByIdWithDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException("Design collaboration project not found.");

        if (!isAdmin && project.BusinessPartnerId != businessPartnerId)
        {
            throw new UnauthorizedAccessException("You do not have permission to complete this project.");
        }

        if (project.Status != CollaborationStatus.Active)
        {
            throw new ConflictException("Only an active project can be marked as completed.");
        }

        var now = DateTime.UtcNow;
        project.Status = CollaborationStatus.Completed;
        project.CompletedAt = now;
        project.UpdatedAt = now;
        project.StatusHistory.Add(new CollaborationStatusEvent
        {
            Id = Guid.NewGuid(),
            Status = CollaborationStatus.Completed,
            CreatedAt = now,
        });

        await _repository.SaveChangesAsync(cancellationToken);
        return ToDto(project);
    }

    public async Task<ProjectDto> CancelAsync(Guid id, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken)
    {
        var project = await GetPartyAsync(id, currentUserId, isAdmin, cancellationToken);

        if (project.Status is CollaborationStatus.Completed or CollaborationStatus.Cancelled or CollaborationStatus.Declined)
        {
            throw new ConflictException("This project can no longer be cancelled.");
        }

        var now = DateTime.UtcNow;
        project.Status = CollaborationStatus.Cancelled;
        project.UpdatedAt = now;
        project.StatusHistory.Add(new CollaborationStatusEvent
        {
            Id = Guid.NewGuid(),
            Status = CollaborationStatus.Cancelled,
            CreatedAt = now,
        });

        await _repository.SaveChangesAsync(cancellationToken);
        return ToDto(project);
    }

    private async Task<DesignCollaborationProject> GetPartyAsync(Guid id, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken)
    {
        var project = await _repository.GetByIdWithDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException("Design collaboration project not found.");

        if (!isAdmin && project.BusinessPartnerId != currentUserId && project.ProducerId != currentUserId)
        {
            throw new UnauthorizedAccessException("You do not have permission to access this project.");
        }

        return project;
    }

    private async Task<string> GenerateUniqueReferenceNumberAsync(CancellationToken cancellationToken)
    {
        var year = DateTime.UtcNow.Year;
        string referenceNumber;

        do
        {
            referenceNumber = $"DES-{year}-{Random.Shared.Next(100000, 999999)}";
        }
        while (await _repository.ExistsByReferenceNumberAsync(referenceNumber, cancellationToken));

        return referenceNumber;
    }

    private static PagedResult<ProjectListItemDto> ToPagedListDto(
        List<DesignCollaborationProject> items, int totalCount, ProjectQueryParameters parameters)
    {
        return new PagedResult<ProjectListItemDto>
        {
            Items = items.Select(p => new ProjectListItemDto
            {
                Id = p.Id,
                ReferenceNumber = p.ReferenceNumber,
                Title = p.Title,
                ProducerName = p.Producer.FullName,
                Status = p.Status,
                RevisionCount = p.Revisions.Count,
                CreatedAt = p.CreatedAt,
            }).ToList(),
            TotalCount = totalCount,
            Page = parameters.Page,
            PageSize = parameters.PageSize,
        };
    }

    private static DesignFileDto ToFileDto(DesignFile file) => new()
    {
        Id = file.Id,
        RevisionId = file.RevisionId,
        FileName = file.FileName,
        FileUrl = file.FileUrl,
        FileType = file.FileType,
        UploadedByUserId = file.UploadedByUserId,
        UploadedByName = file.UploadedBy?.FullName ?? string.Empty,
        UploadedAt = file.UploadedAt,
    };

    private static DesignRevisionDto ToRevisionDto(DesignRevision revision) => new()
    {
        Id = revision.Id,
        RevisionNumber = revision.RevisionNumber,
        Description = revision.Description,
        Status = revision.Status,
        SubmittedByUserId = revision.SubmittedByUserId,
        SubmittedByName = revision.SubmittedBy?.FullName ?? string.Empty,
        SubmittedAt = revision.SubmittedAt,
        DecidedAt = revision.DecidedAt,
        DecisionNotes = revision.DecisionNotes,
        Files = revision.Files.Select(ToFileDto).ToList(),
    };

    private static ProjectDto ToDto(DesignCollaborationProject project) => new()
    {
        Id = project.Id,
        ReferenceNumber = project.ReferenceNumber,
        BusinessPartnerId = project.BusinessPartnerId,
        BusinessPartnerName = project.BusinessPartner.FullName,
        ProducerId = project.ProducerId,
        ProducerName = project.Producer.FullName,
        Title = project.Title,
        DesignRequirements = project.DesignRequirements,
        Status = project.Status,
        RespondedAt = project.RespondedAt,
        CompletedAt = project.CompletedAt,
        Files = project.Files.Where(f => f.RevisionId == null).Select(ToFileDto).ToList(),
        Comments = project.Comments
            .OrderBy(c => c.CreatedAt)
            .Select(c => new DesignCommentDto
            {
                Id = c.Id,
                AuthorUserId = c.AuthorUserId,
                AuthorName = c.Author.FullName,
                Content = c.Content,
                CreatedAt = c.CreatedAt,
            }).ToList(),
        Revisions = project.Revisions.OrderBy(r => r.RevisionNumber).Select(ToRevisionDto).ToList(),
        StatusHistory = project.StatusHistory
            .OrderByDescending(h => h.CreatedAt)
            .Select(h => new CollaborationStatusEventDto
            {
                Status = h.Status,
                Note = h.Note,
                CreatedAt = h.CreatedAt,
            }).ToList(),
        CreatedAt = project.CreatedAt,
        UpdatedAt = project.UpdatedAt,
    };
}
