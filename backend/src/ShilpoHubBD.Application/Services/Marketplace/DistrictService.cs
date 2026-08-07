using ShilpoHubBD.Application.DTOs.Marketplace;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;

namespace ShilpoHubBD.Application.Services.Marketplace;

public class DistrictService : IDistrictService
{
    private readonly IDistrictRepository _districtRepository;

    public DistrictService(IDistrictRepository districtRepository)
    {
        _districtRepository = districtRepository;
    }

    public async Task<List<DistrictDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var districts = await _districtRepository.GetAllAsync(cancellationToken);
        return districts.Select(d => new DistrictDto
        {
            Id = d.Id,
            Name = d.Name,
            Division = d.Division,
        }).ToList();
    }
}
