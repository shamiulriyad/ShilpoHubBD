using ShilpoHubBD.Application.DTOs.Marketplace;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Application.Services.Marketplace;

public class DistrictService : IDistrictService
{
    private readonly IDistrictRepository _districtRepository;

    public DistrictService(IDistrictRepository districtRepository)
    {
        _districtRepository = districtRepository;
    }

    public async Task<List<DistrictDto>> GetAllAsync(bool includeInactive, CancellationToken cancellationToken)
    {
        var districts = await _districtRepository.GetAllAsync(includeInactive, cancellationToken);
        return districts.Select(ToDto).ToList();
    }

    public async Task<DistrictDto> UpdateAsync(Guid id, UpdateDistrictRequest request, CancellationToken cancellationToken)
    {
        var district = await _districtRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("District not found.");

        district.Division = request.Division.Trim();
        district.DisplayOrder = request.DisplayOrder;
        district.IsActive = request.IsActive;

        await _districtRepository.SaveChangesAsync(cancellationToken);
        return ToDto(district);
    }

    private static DistrictDto ToDto(District d) => new()
    {
        Id = d.Id,
        Name = d.Name,
        Division = d.Division,
        DisplayOrder = d.DisplayOrder,
        IsActive = d.IsActive,
        Description = d.Description,
        KnownFor = d.KnownFor,
        SourceUrl = d.SourceUrl,
        ImageUrl = d.ImageUrl,
        ImageCredit = d.ImageCredit,
    };
}
