using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.ManufacturingPartnership;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IPartnershipService
{
    Task<PartnershipDto> CreateAsync(Guid businessPartnerId, CreatePartnershipRequest request, CancellationToken cancellationToken);

    Task<PagedResult<PartnershipListItemDto>> GetForBusinessPartnerAsync(Guid businessPartnerId, bool isAdmin, PartnershipQueryParameters parameters, CancellationToken cancellationToken);
    Task<PagedResult<PartnershipListItemDto>> GetForProducerAsync(Guid producerId, PartnershipQueryParameters parameters, CancellationToken cancellationToken);
    Task<PartnershipDto> GetByIdAsync(Guid id, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken);

    Task<PartnershipDto> RespondAsync(Guid id, Guid producerId, PartnershipResponseRequest request, CancellationToken cancellationToken);
    Task<MilestoneDto> AddMilestoneAsync(Guid id, Guid currentUserId, bool isAdmin, MilestoneInput request, CancellationToken cancellationToken);
    Task<MilestoneDto> UpdateMilestoneStatusAsync(Guid id, Guid milestoneId, Guid currentUserId, bool isAdmin, UpdateMilestoneStatusRequest request, CancellationToken cancellationToken);
    Task<PartnershipDto> CompleteAsync(Guid id, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken);
    Task<PartnershipDto> CancelAsync(Guid id, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken);
}
