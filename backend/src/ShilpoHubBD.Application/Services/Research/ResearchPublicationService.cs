using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Research;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Research;

namespace ShilpoHubBD.Application.Services.Research;

public class ResearchPublicationService : ResearchServiceBase, IResearchPublicationService
{
    public ResearchPublicationService(IResearchProjectRepository repository) : base(repository)
    {
    }

    public async Task<PagedResult<ResearchPublicationDto>> BrowseAsync(
        Guid userId, ResearchPublicationQueryParameters query, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await Repository.GetPublicationRepositoryAsync(userId, query, cancellationToken);
        return new PagedResult<ResearchPublicationDto>
        {
            Items = items.Select(p => p.ToDto()).ToList(),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }

    public async Task<ResearchPublicationDto> GetByIdAsync(Guid userId, Guid publicationId, CancellationToken cancellationToken)
    {
        var publication = await Repository.GetPublicationByIdAsync(publicationId, cancellationToken)
            ?? throw new NotFoundException("Publication not found.");

        if (!publication.IsPublic)
        {
            var membership = await Repository.GetMembershipAsync(publication.ResearchProjectId, userId, cancellationToken);
            if (membership is null)
            {
                throw new NotFoundException("Publication not found.");
            }
        }

        return publication.ToDto();
    }

    public async Task<List<ResearchPublicationDto>> GetForProjectAsync(
        Guid userId, Guid projectId, CancellationToken cancellationToken)
    {
        await LoadProjectWithReadAccessAsync(userId, projectId, cancellationToken);
        var publications = await Repository.GetPublicationsForProjectAsync(projectId, cancellationToken);
        return publications.Select(p => p.ToDto()).ToList();
    }

    public async Task<ResearchPublicationDto> CreateAsync(
        Guid userId, Guid projectId, CreateResearchPublicationRequest request, CancellationToken cancellationToken)
    {
        var member = await LoadProjectWithRoleAsync(userId, projectId, ResearchRole.Researcher, cancellationToken);

        var type = ParseType(request.Type) ?? ResearchPublicationType.JournalArticle;
        await ValidatePaperAsync(projectId, request.ResearchPaperId, cancellationToken);

        var now = DateTime.UtcNow;
        var publication = new ResearchPublication
        {
            Id = Guid.NewGuid(),
            ResearchProjectId = projectId,
            ResearchPaperId = request.ResearchPaperId,
            Title = request.Title.Trim(),
            Authors = request.Authors.Trim(),
            Venue = request.Venue?.Trim(),
            Type = type,
            Doi = request.Doi?.Trim(),
            Url = request.Url?.Trim(),
            Abstract = request.Abstract?.Trim(),
            Citation = request.Citation?.Trim(),
            PublishedOn = request.PublishedOn,
            IsPublic = request.IsPublic,
            CreatedByUserId = userId,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await Repository.AddPublicationAsync(publication, cancellationToken);
        await AddActivityAsync(projectId, userId, ResearchActivityType.PublicationCreated,
            $"{member.User?.FullName} recorded publication \"{publication.Title}\".", cancellationToken);
        await Repository.SaveChangesAsync(cancellationToken);

        return (await Repository.GetPublicationByIdAsync(publication.Id, cancellationToken))!.ToDto();
    }

    public async Task<ResearchPublicationDto> UpdateAsync(
        Guid userId, Guid projectId, Guid publicationId, UpdateResearchPublicationRequest request, CancellationToken cancellationToken)
    {
        var member = await LoadProjectWithRoleAsync(userId, projectId, ResearchRole.Researcher, cancellationToken);
        var publication = await LoadPublicationAsync(projectId, publicationId, cancellationToken);

        var type = ParseType(request.Type)
            ?? throw new ConflictException("Type must be a valid publication type.");
        await ValidatePaperAsync(projectId, request.ResearchPaperId, cancellationToken);

        publication.ResearchPaperId = request.ResearchPaperId;
        publication.Title = request.Title.Trim();
        publication.Authors = request.Authors.Trim();
        publication.Venue = request.Venue?.Trim();
        publication.Type = type;
        publication.Doi = request.Doi?.Trim();
        publication.Url = request.Url?.Trim();
        publication.Abstract = request.Abstract?.Trim();
        publication.Citation = request.Citation?.Trim();
        publication.PublishedOn = request.PublishedOn;
        publication.IsPublic = request.IsPublic;
        publication.UpdatedAt = DateTime.UtcNow;

        await AddActivityAsync(projectId, userId, ResearchActivityType.PublicationUpdated,
            $"{member.User?.FullName} updated publication \"{publication.Title}\".", cancellationToken);
        await Repository.SaveChangesAsync(cancellationToken);

        return (await Repository.GetPublicationByIdAsync(publication.Id, cancellationToken))!.ToDto();
    }

    public async Task DeleteAsync(Guid userId, Guid projectId, Guid publicationId, CancellationToken cancellationToken)
    {
        var member = await LoadProjectWithRoleAsync(userId, projectId, ResearchRole.Researcher, cancellationToken);
        var publication = await LoadPublicationAsync(projectId, publicationId, cancellationToken);

        if (publication.CreatedByUserId != userId && !member.Role.AtLeast(ResearchRole.Admin))
        {
            throw new UnauthorizedAccessException(
                "Only the publication creator or a project admin can delete this entry.");
        }

        Repository.RemovePublication(publication);
        await AddActivityAsync(projectId, userId, ResearchActivityType.PublicationDeleted,
            $"{member.User?.FullName} removed publication \"{publication.Title}\".", cancellationToken);
        await Repository.SaveChangesAsync(cancellationToken);
    }

    private async Task<ResearchPublication> LoadPublicationAsync(Guid projectId, Guid publicationId, CancellationToken cancellationToken)
    {
        var publication = await Repository.GetPublicationByIdAsync(publicationId, cancellationToken);
        if (publication is null || publication.ResearchProjectId != projectId)
        {
            throw new NotFoundException("Publication not found.");
        }

        return publication;
    }

    private async Task ValidatePaperAsync(Guid projectId, Guid? paperId, CancellationToken cancellationToken)
    {
        if (!paperId.HasValue)
        {
            return;
        }

        var paper = await Repository.GetPaperByIdAsync(paperId.Value, cancellationToken);
        if (paper is null || paper.ResearchProjectId != projectId)
        {
            throw new NotFoundException("Linked paper not found in this project.");
        }
    }

    private static ResearchPublicationType? ParseType(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return Enum.TryParse<ResearchPublicationType>(value, true, out var parsed) ? parsed : null;
    }
}
