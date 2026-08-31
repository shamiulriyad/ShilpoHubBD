using ShilpoHubBD.Application.DTOs.ArVr;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.ArVr;

namespace ShilpoHubBD.Application.Services.ArVr;

public class MuseumItemService : IMuseumItemService
{
    private readonly IMuseumItemRepository _museumItemRepository;
    private readonly IDistrictRepository _districtRepository;
    private readonly IProductRepository _productRepository;

    public MuseumItemService(
        IMuseumItemRepository museumItemRepository, IDistrictRepository districtRepository, IProductRepository productRepository)
    {
        _museumItemRepository = museumItemRepository;
        _districtRepository = districtRepository;
        _productRepository = productRepository;
    }

    public async Task<PagedResult<MuseumItemDto>> GetPagedAsync(MuseumItemQueryParameters query, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _museumItemRepository.GetPagedAsync(query, cancellationToken);
        return new PagedResult<MuseumItemDto>
        {
            Items = items.Select(ToDto).ToList(),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }

    public async Task<MuseumItemDto> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var item = await _museumItemRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Museum item not found.");
        return ToDto(item);
    }

    public async Task<MuseumItemDto> CreateAsync(CreateMuseumItemRequest request, CancellationToken cancellationToken)
    {
        if (await _districtRepository.GetByIdAsync(request.DistrictId, cancellationToken) is null)
        {
            throw new NotFoundException("District not found.");
        }

        if (request.ProductId.HasValue && await _productRepository.GetByIdAsync(request.ProductId.Value, cancellationToken) is null)
        {
            throw new NotFoundException("Product not found.");
        }

        var now = DateTime.UtcNow;
        var item = new MuseumItem
        {
            Id = Guid.NewGuid(),
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Category = request.Category.Trim(),
            Era = request.Era?.Trim(),
            CoverImageUrl = request.CoverImageUrl.Trim(),
            ModelUrl = request.ModelUrl?.Trim(),
            IsFeatured = request.IsFeatured,
            IsActive = true,
            DistrictId = request.DistrictId,
            ProductId = request.ProductId,
            CreatedAt = now,
            UpdatedAt = now,
        };

        AttachMedia(item, request.Media);

        await _museumItemRepository.AddAsync(item, cancellationToken);
        await _museumItemRepository.SaveChangesAsync(cancellationToken);

        var created = await _museumItemRepository.GetByIdAsync(item.Id, cancellationToken);
        return ToDto(created!);
    }

    public async Task<MuseumItemDto> UpdateAsync(Guid id, UpdateMuseumItemRequest request, CancellationToken cancellationToken)
    {
        var item = await _museumItemRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Museum item not found.");

        if (await _districtRepository.GetByIdAsync(request.DistrictId, cancellationToken) is null)
        {
            throw new NotFoundException("District not found.");
        }

        if (request.ProductId.HasValue && await _productRepository.GetByIdAsync(request.ProductId.Value, cancellationToken) is null)
        {
            throw new NotFoundException("Product not found.");
        }

        item.Title = request.Title.Trim();
        item.Description = request.Description.Trim();
        item.Category = request.Category.Trim();
        item.Era = request.Era?.Trim();
        item.CoverImageUrl = request.CoverImageUrl.Trim();
        item.ModelUrl = request.ModelUrl?.Trim();
        item.IsFeatured = request.IsFeatured;
        item.IsActive = request.IsActive;
        item.DistrictId = request.DistrictId;
        item.ProductId = request.ProductId;
        item.UpdatedAt = DateTime.UtcNow;

        item.Media.Clear();
        AttachMedia(item, request.Media);

        await _museumItemRepository.SaveChangesAsync(cancellationToken);

        var updated = await _museumItemRepository.GetByIdAsync(id, cancellationToken);
        return ToDto(updated!);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var item = await _museumItemRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Museum item not found.");

        _museumItemRepository.Remove(item);
        await _museumItemRepository.SaveChangesAsync(cancellationToken);
    }

    private static void AttachMedia(MuseumItem item, List<MediaInput> media)
    {
        for (var i = 0; i < media.Count; i++)
        {
            item.Media.Add(new MuseumItemMedia
            {
                Id = Guid.NewGuid(),
                MediaUrl = media[i].MediaUrl.Trim(),
                MediaType = media[i].MediaType,
                Caption = media[i].Caption?.Trim(),
                DisplayOrder = i,
            });
        }
    }

    private static MuseumItemDto ToDto(MuseumItem item) => new()
    {
        Id = item.Id,
        Title = item.Title,
        Description = item.Description,
        Category = item.Category,
        Era = item.Era,
        CoverImageUrl = item.CoverImageUrl,
        ModelUrl = item.ModelUrl,
        IsFeatured = item.IsFeatured,
        IsActive = item.IsActive,
        DistrictId = item.DistrictId,
        DistrictName = item.District.Name,
        ProductId = item.ProductId,
        ProductName = item.Product?.Name,
        Media = item.Media
            .OrderBy(m => m.DisplayOrder)
            .Select(m => new MuseumItemMediaDto
            {
                Id = m.Id,
                MediaUrl = m.MediaUrl,
                MediaType = m.MediaType.ToString(),
                Caption = m.Caption,
                DisplayOrder = m.DisplayOrder,
            })
            .ToList(),
        CreatedAt = item.CreatedAt,
        UpdatedAt = item.UpdatedAt,
    };
}
