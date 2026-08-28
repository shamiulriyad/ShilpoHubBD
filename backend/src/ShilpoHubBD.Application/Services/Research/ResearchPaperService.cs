using ShilpoHubBD.Application.DTOs.Research;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Research;

namespace ShilpoHubBD.Application.Services.Research;

public class ResearchPaperService : ResearchServiceBase, IResearchPaperService
{
    public ResearchPaperService(IResearchProjectRepository repository) : base(repository)
    {
    }

    public async Task<List<ResearchPaperDto>> GetForProjectAsync(
        Guid userId, Guid projectId, CancellationToken cancellationToken)
    {
        await LoadProjectWithReadAccessAsync(userId, projectId, cancellationToken);
        var papers = await Repository.GetPapersAsync(projectId, cancellationToken);
        return papers.Select(p => p.ToDto()).ToList();
    }

    public async Task<ResearchPaperDto> GetByIdAsync(
        Guid userId, Guid projectId, Guid paperId, CancellationToken cancellationToken)
    {
        await LoadProjectWithReadAccessAsync(userId, projectId, cancellationToken);
        var paper = await LoadPaperAsync(projectId, paperId, cancellationToken);
        return paper.ToDto();
    }

    public async Task<ResearchPaperDto> CreateAsync(
        Guid userId, Guid projectId, CreateResearchPaperRequest request, CancellationToken cancellationToken)
    {
        var member = await LoadProjectWithRoleAsync(userId, projectId, ResearchRole.Researcher, cancellationToken);

        var now = DateTime.UtcNow;
        var paper = new ResearchPaper
        {
            Id = Guid.NewGuid(),
            ResearchProjectId = projectId,
            Title = request.Title.Trim(),
            Abstract = request.Abstract?.Trim(),
            Authors = request.Authors?.Trim(),
            Keywords = request.Keywords?.Trim(),
            Status = ResearchPaperStatus.Draft,
            ManuscriptUrl = request.ManuscriptUrl?.Trim(),
            TargetVenue = request.TargetVenue?.Trim(),
            CreatedByUserId = userId,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await Repository.AddPaperAsync(paper, cancellationToken);
        await AddActivityAsync(projectId, userId, ResearchActivityType.PaperCreated,
            $"{member.User?.FullName} started paper \"{paper.Title}\".", cancellationToken);
        await Repository.SaveChangesAsync(cancellationToken);

        return (await LoadPaperAsync(projectId, paper.Id, cancellationToken)).ToDto();
    }

    public async Task<ResearchPaperDto> UpdateAsync(
        Guid userId, Guid projectId, Guid paperId, UpdateResearchPaperRequest request, CancellationToken cancellationToken)
    {
        var member = await LoadProjectWithRoleAsync(userId, projectId, ResearchRole.Researcher, cancellationToken);
        var paper = await LoadPaperAsync(projectId, paperId, cancellationToken);

        if (!Enum.TryParse<ResearchPaperStatus>(request.Status, true, out var status))
        {
            throw new ConflictException(
                "Status must be one of: Draft, InternalReview, Submitted, UnderReview, Revision, Accepted, Published, Rejected.");
        }

        var statusChanged = paper.Status != status;

        paper.Title = request.Title.Trim();
        paper.Abstract = request.Abstract?.Trim();
        paper.Authors = request.Authors?.Trim();
        paper.Keywords = request.Keywords?.Trim();
        paper.Status = status;
        paper.ManuscriptUrl = request.ManuscriptUrl?.Trim();
        paper.TargetVenue = request.TargetVenue?.Trim();
        paper.UpdatedAt = DateTime.UtcNow;

        await AddActivityAsync(projectId, userId,
            statusChanged ? ResearchActivityType.PaperStatusChanged : ResearchActivityType.PaperUpdated,
            statusChanged
                ? $"{member.User?.FullName} set paper \"{paper.Title}\" to {status}."
                : $"{member.User?.FullName} updated paper \"{paper.Title}\".",
            cancellationToken);
        await Repository.SaveChangesAsync(cancellationToken);

        return (await LoadPaperAsync(projectId, paper.Id, cancellationToken)).ToDto();
    }

    public async Task DeleteAsync(Guid userId, Guid projectId, Guid paperId, CancellationToken cancellationToken)
    {
        var member = await LoadProjectWithRoleAsync(userId, projectId, ResearchRole.Researcher, cancellationToken);
        var paper = await LoadPaperAsync(projectId, paperId, cancellationToken);

        if (paper.CreatedByUserId != userId && !member.Role.AtLeast(ResearchRole.Admin))
        {
            throw new UnauthorizedAccessException("Only the paper creator or a project admin can delete this paper.");
        }

        Repository.RemovePaper(paper);
        await AddActivityAsync(projectId, userId, ResearchActivityType.PaperDeleted,
            $"{member.User?.FullName} deleted paper \"{paper.Title}\".", cancellationToken);
        await Repository.SaveChangesAsync(cancellationToken);
    }

    private async Task<ResearchPaper> LoadPaperAsync(Guid projectId, Guid paperId, CancellationToken cancellationToken)
    {
        var paper = await Repository.GetPaperByIdAsync(paperId, cancellationToken);
        if (paper is null || paper.ResearchProjectId != projectId)
        {
            throw new NotFoundException("Research paper not found.");
        }

        return paper;
    }
}
