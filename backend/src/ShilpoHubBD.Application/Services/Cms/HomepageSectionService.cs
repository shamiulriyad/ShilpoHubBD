using ShilpoHubBD.Application.DTOs.Cms;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Cms;

namespace ShilpoHubBD.Application.Services.Cms;

public class HomepageSectionService : IHomepageSectionService
{
    private readonly IHomepageSectionRepository _repository;

    public HomepageSectionService(IHomepageSectionRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<HomepageSectionDto>> GetAllAsync(bool includeInactive, CancellationToken cancellationToken)
    {
        var sections = await _repository.GetAllAsync(includeInactive, cancellationToken);
        return sections.Select(ToDto).ToList();
    }

    public async Task<HomepageSectionDto> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var section = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Homepage section not found.");
        return ToDto(section);
    }

    public async Task<HomepageSectionDto> CreateAsync(CreateHomepageSectionRequest request, CancellationToken cancellationToken)
    {
        if (await _repository.ExistsBySectionKeyAsync(request.SectionKey.Trim(), cancellationToken))
        {
            throw new ConflictException($"A homepage section with key '{request.SectionKey}' already exists.");
        }

        var now = DateTime.UtcNow;
        var section = new HomepageSection
        {
            Id = Guid.NewGuid(),
            SectionKey = request.SectionKey.Trim(),
            Title = request.Title.Trim(),
            Subtitle = string.IsNullOrWhiteSpace(request.Subtitle) ? null : request.Subtitle.Trim(),
            ImageUrl = request.ImageUrl?.Trim(),
            LinkUrl = request.LinkUrl?.Trim(),
            DisplayOrder = request.DisplayOrder,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await _repository.AddAsync(section, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return ToDto(section);
    }

    public async Task<HomepageSectionDto> UpdateAsync(Guid id, UpdateHomepageSectionRequest request, CancellationToken cancellationToken)
    {
        var section = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Homepage section not found.");

        section.Title = request.Title.Trim();
        section.Subtitle = string.IsNullOrWhiteSpace(request.Subtitle) ? null : request.Subtitle.Trim();
        section.ImageUrl = request.ImageUrl?.Trim();
        section.LinkUrl = request.LinkUrl?.Trim();
        section.DisplayOrder = request.DisplayOrder;
        section.IsActive = request.IsActive;
        section.UpdatedAt = DateTime.UtcNow;

        await _repository.SaveChangesAsync(cancellationToken);
        return ToDto(section);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var section = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Homepage section not found.");

        _repository.Remove(section);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    private static HomepageSectionDto ToDto(HomepageSection section) => new()
    {
        Id = section.Id,
        SectionKey = section.SectionKey,
        Title = section.Title,
        Subtitle = section.Subtitle,
        ImageUrl = section.ImageUrl,
        LinkUrl = section.LinkUrl,
        DisplayOrder = section.DisplayOrder,
        IsActive = section.IsActive,
        CreatedAt = section.CreatedAt,
        UpdatedAt = section.UpdatedAt,
    };
}
