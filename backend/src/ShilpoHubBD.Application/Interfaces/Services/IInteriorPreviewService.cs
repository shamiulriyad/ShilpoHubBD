using ShilpoHubBD.Application.DTOs.AIShopping;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IInteriorPreviewService
{
    Task<InteriorPreviewDto> GetPreviewAsync(InteriorPreviewRequest request, CancellationToken cancellationToken);
}
