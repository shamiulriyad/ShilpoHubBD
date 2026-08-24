using ShilpoHubBD.Application.DTOs.Learning;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IHeritageSkillService
{
    Task<List<HeritageSkillDto>> GetAllAsync(bool activeOnly, CancellationToken cancellationToken);
    Task<HeritageSkillDto> CreateAsync(CreateHeritageSkillRequest request, CancellationToken cancellationToken);
}
