using ShilpoHubBD.Application.DTOs.Research;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Research;

namespace ShilpoHubBD.Application.Services.Research;

public class ResearchNoteService : ResearchServiceBase, IResearchNoteService
{
    public ResearchNoteService(IResearchProjectRepository repository) : base(repository)
    {
    }

    public async Task<List<ResearchNoteDto>> GetForProjectAsync(
        Guid userId, Guid projectId, CancellationToken cancellationToken)
    {
        var membership = await LoadProjectWithReadAccessAsync(userId, projectId, cancellationToken);
        var canSeePrivate = membership is not null && membership.Role.AtLeast(ResearchRole.Admin);

        var notes = await Repository.GetNotesAsync(projectId, cancellationToken);
        return notes
            .Where(n => n.Visibility == ResearchNoteVisibility.Team
                        || n.AuthorUserId == userId
                        || canSeePrivate)
            .Select(n => n.ToDto())
            .ToList();
    }

    public async Task<ResearchNoteDto> CreateAsync(
        Guid userId, Guid projectId, CreateResearchNoteRequest request, CancellationToken cancellationToken)
    {
        var member = await LoadProjectWithRoleAsync(userId, projectId, ResearchRole.Contributor, cancellationToken);
        var visibility = ParseVisibility(request.Visibility) ?? ResearchNoteVisibility.Team;

        var now = DateTime.UtcNow;
        var note = new ResearchNote
        {
            Id = Guid.NewGuid(),
            ResearchProjectId = projectId,
            AuthorUserId = userId,
            Title = request.Title.Trim(),
            Content = request.Content.Trim(),
            Visibility = visibility,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await Repository.AddNoteAsync(note, cancellationToken);
        await AddActivityAsync(projectId, userId, ResearchActivityType.NoteCreated,
            $"{member.User?.FullName} added note \"{note.Title}\".", cancellationToken);
        await Repository.SaveChangesAsync(cancellationToken);

        note.Author = member.User!;
        return note.ToDto();
    }

    public async Task<ResearchNoteDto> UpdateAsync(
        Guid userId, Guid projectId, Guid noteId, UpdateResearchNoteRequest request, CancellationToken cancellationToken)
    {
        var member = await LoadProjectWithRoleAsync(userId, projectId, ResearchRole.Contributor, cancellationToken);
        var note = await LoadNoteAsync(projectId, noteId, cancellationToken);
        EnsureCanMutateNote(note, member);

        var visibility = ParseVisibility(request.Visibility)
            ?? throw new ConflictException("Visibility must be one of: Private, Team.");

        note.Title = request.Title.Trim();
        note.Content = request.Content.Trim();
        note.Visibility = visibility;
        note.UpdatedAt = DateTime.UtcNow;

        await AddActivityAsync(projectId, userId, ResearchActivityType.NoteUpdated,
            $"{member.User?.FullName} updated note \"{note.Title}\".", cancellationToken);
        await Repository.SaveChangesAsync(cancellationToken);

        return note.ToDto();
    }

    public async Task DeleteAsync(Guid userId, Guid projectId, Guid noteId, CancellationToken cancellationToken)
    {
        var member = await LoadProjectWithRoleAsync(userId, projectId, ResearchRole.Contributor, cancellationToken);
        var note = await LoadNoteAsync(projectId, noteId, cancellationToken);
        EnsureCanMutateNote(note, member);

        Repository.RemoveNote(note);
        await AddActivityAsync(projectId, userId, ResearchActivityType.NoteDeleted,
            $"{member.User?.FullName} deleted note \"{note.Title}\".", cancellationToken);
        await Repository.SaveChangesAsync(cancellationToken);
    }

    private async Task<ResearchNote> LoadNoteAsync(Guid projectId, Guid noteId, CancellationToken cancellationToken)
    {
        var note = await Repository.GetNoteByIdAsync(noteId, cancellationToken);
        if (note is null || note.ResearchProjectId != projectId)
        {
            throw new NotFoundException("Research note not found.");
        }

        return note;
    }

    private static void EnsureCanMutateNote(ResearchNote note, ResearchProjectMember member)
    {
        if (note.AuthorUserId != member.UserId && !member.Role.AtLeast(ResearchRole.Admin))
        {
            throw new UnauthorizedAccessException("Only the note author or a project admin can modify this note.");
        }
    }

    private static ResearchNoteVisibility? ParseVisibility(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return Enum.TryParse<ResearchNoteVisibility>(value, true, out var parsed) ? parsed : null;
    }
}
