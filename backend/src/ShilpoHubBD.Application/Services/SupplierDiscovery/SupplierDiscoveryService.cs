using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.SupplierDiscovery;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Application.Services.SupplierDiscovery;

public class SupplierDiscoveryService : ISupplierDiscoveryService
{
    private readonly ISupplierDiscoveryRepository _supplierDiscoveryRepository;
    private readonly IUserRepository _userRepository;

    public SupplierDiscoveryService(ISupplierDiscoveryRepository supplierDiscoveryRepository, IUserRepository userRepository)
    {
        _supplierDiscoveryRepository = supplierDiscoveryRepository;
        _userRepository = userRepository;
    }

    public async Task<PagedResult<SupplierSearchResultDto>> SearchAsync(SupplierSearchParameters parameters, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _supplierDiscoveryRepository.SearchAsync(parameters, cancellationToken);

        return new PagedResult<SupplierSearchResultDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = parameters.Page,
            PageSize = parameters.PageSize,
        };
    }

    public async Task<SupplierProfileDto> GetProducerProfileAsync(Guid producerId, CancellationToken cancellationToken)
    {
        var producer = await _userRepository.GetByIdWithRolesAsync(producerId, cancellationToken)
            ?? throw new NotFoundException("Producer not found.");

        if (!producer.UserRoles.Any(ur => ur.Role.Name == RoleNames.Producer))
        {
            throw new NotFoundException("Producer not found.");
        }

        return await _supplierDiscoveryRepository.GetProducerProfileAsync(producerId, cancellationToken)
            ?? throw new NotFoundException("Producer not found.");
    }
}
