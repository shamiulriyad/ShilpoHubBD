using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.ProductDevelopment;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IProductDevelopmentService
{
    Task<DevelopmentProjectDto> CreateAsync(Guid businessPartnerId, CreateDevelopmentProjectRequest request, CancellationToken cancellationToken);

    Task<PagedResult<DevelopmentProjectListItemDto>> GetForBusinessPartnerAsync(Guid businessPartnerId, bool isAdmin, DevelopmentProjectQueryParameters parameters, CancellationToken cancellationToken);
    Task<PagedResult<DevelopmentProjectListItemDto>> GetForProducerAsync(Guid producerId, DevelopmentProjectQueryParameters parameters, CancellationToken cancellationToken);
    Task<DevelopmentProjectDto> GetByIdAsync(Guid id, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken);

    Task<DevelopmentProjectDto> RespondAsync(Guid id, Guid producerId, DevelopmentResponseRequest request, CancellationToken cancellationToken);
    Task<DevelopmentCommentDto> AddCommentAsync(Guid id, Guid currentUserId, bool isAdmin, AddDevelopmentCommentRequest request, CancellationToken cancellationToken);
    Task<DevelopmentMilestoneDto> AddMilestoneAsync(Guid id, Guid currentUserId, bool isAdmin, DevelopmentMilestoneInput request, CancellationToken cancellationToken);
    Task<DevelopmentMilestoneDto> UpdateMilestoneStatusAsync(Guid id, Guid milestoneId, Guid currentUserId, bool isAdmin, UpdateDevelopmentMilestoneStatusRequest request, CancellationToken cancellationToken);
    Task<PrototypeVersionDto> SubmitPrototypeAsync(Guid id, Guid producerId, SubmitPrototypeRequest request, CancellationToken cancellationToken);
    Task<PrototypeVersionDto> DecidePrototypeAsync(Guid id, Guid prototypeVersionId, Guid businessPartnerId, bool isAdmin, PrototypeDecisionRequest request, CancellationToken cancellationToken);
    Task<DevelopmentProjectDto> ConvertToProductAsync(Guid id, Guid businessPartnerId, bool isAdmin, ConvertToProductRequest request, CancellationToken cancellationToken);
    Task<DevelopmentProjectDto> CancelAsync(Guid id, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken);
}
