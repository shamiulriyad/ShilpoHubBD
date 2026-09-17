using ShilpoHubBD.Application.DTOs.Cms;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IAnnouncementService
{
    Task<List<AnnouncementDto>> GetAllAsync(bool activeOnly, CancellationToken cancellationToken);
    Task<AnnouncementDto> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<AnnouncementDto> CreateAsync(CreateAnnouncementRequest request, CancellationToken cancellationToken);
    Task<AnnouncementDto> UpdateAsync(Guid id, UpdateAnnouncementRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
