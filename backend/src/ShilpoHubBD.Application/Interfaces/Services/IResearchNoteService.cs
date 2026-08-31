using ShilpoHubBD.Application.DTOs.Research;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IResearchNoteService
{
    Task<List<ResearchNoteDto>> GetForProjectAsync(Guid userId, Guid projectId, CancellationToken cancellationToken);

    Task<ResearchNoteDto> CreateAsync(
        Guid userId, Guid projectId, CreateResearchNoteRequest request, CancellationToken cancellationToken);

    Task<ResearchNoteDto> UpdateAsync(
        Guid userId, Guid projectId, Guid noteId, UpdateResearchNoteRequest request, CancellationToken cancellationToken);

    Task DeleteAsync(Guid userId, Guid projectId, Guid noteId, CancellationToken cancellationToken);
}
