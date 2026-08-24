using ShilpoHubBD.Application.DTOs.Learning;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Learning;

namespace ShilpoHubBD.Application.Services.Learning;

public class HeritageSkillService : IHeritageSkillService
{
    private readonly IHeritageSkillRepository _skillRepository;

    public HeritageSkillService(IHeritageSkillRepository skillRepository)
    {
        _skillRepository = skillRepository;
    }

    public async Task<List<HeritageSkillDto>> GetAllAsync(bool activeOnly, CancellationToken cancellationToken)
    {
        var skills = await _skillRepository.GetAllAsync(activeOnly, cancellationToken);
        return skills.Select(ToDto).ToList();
    }

    public async Task<HeritageSkillDto> CreateAsync(CreateHeritageSkillRequest request, CancellationToken cancellationToken)
    {
        if (await _skillRepository.ExistsByNameAsync(request.Name.Trim(), cancellationToken))
        {
            throw new ConflictException("A heritage skill with this name already exists.");
        }

        var skill = new HeritageSkill
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = request.Description.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
        };

        await _skillRepository.AddAsync(skill, cancellationToken);
        await _skillRepository.SaveChangesAsync(cancellationToken);

        return ToDto(skill);
    }

    private static HeritageSkillDto ToDto(HeritageSkill skill) => new()
    {
        Id = skill.Id,
        Name = skill.Name,
        Description = skill.Description,
        IsActive = skill.IsActive,
        CreatedAt = skill.CreatedAt,
    };
}
