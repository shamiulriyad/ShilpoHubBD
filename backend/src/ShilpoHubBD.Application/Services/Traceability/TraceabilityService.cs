using ShilpoHubBD.Application.DTOs.Traceability;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Traceability;

namespace ShilpoHubBD.Application.Services.Traceability;

public class TraceabilityService : ITraceabilityService
{
    private readonly ITraceabilityRepository _traceabilityRepository;
    private readonly IProductRepository _productRepository;

    public TraceabilityService(ITraceabilityRepository traceabilityRepository, IProductRepository productRepository)
    {
        _traceabilityRepository = traceabilityRepository;
        _productRepository = productRepository;
    }

    public async Task<ProductTraceabilityDto> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken)
    {
        var traceability = await _traceabilityRepository.GetByProductIdAsync(productId, cancellationToken)
            ?? throw new NotFoundException("Traceability record not found for this product.");

        return ToDto(traceability);
    }

    public async Task<ProductTraceabilityDto> CreateAsync(Guid producerId, CreateProductTraceabilityRequest request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken)
            ?? throw new NotFoundException("Product not found.");

        if (product.ProducerId != producerId)
        {
            throw new UnauthorizedAccessException("You can only add traceability records for your own products.");
        }

        if (await _traceabilityRepository.ExistsByProductIdAsync(request.ProductId, cancellationToken))
        {
            throw new ConflictException("This product already has a traceability record.");
        }

        var now = DateTime.UtcNow;
        var traceability = new ProductTraceability
        {
            Id = Guid.NewGuid(),
            ProductId = request.ProductId,
            Summary = request.Summary.Trim(),
            CreatedAt = now,
            UpdatedAt = now,
        };

        AttachMaterialSources(traceability, request.MaterialSources);
        AttachTimelineEvents(traceability, request.TimelineEvents);

        await _traceabilityRepository.AddAsync(traceability, cancellationToken);
        await _traceabilityRepository.SaveChangesAsync(cancellationToken);

        traceability.Product = product;
        return ToDto(traceability);
    }

    public async Task<ProductTraceabilityDto> UpdateAsync(Guid id, Guid producerId, bool isAdmin, UpdateProductTraceabilityRequest request, CancellationToken cancellationToken)
    {
        var traceability = await _traceabilityRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Traceability record not found.");

        if (!isAdmin && traceability.Product.ProducerId != producerId)
        {
            throw new UnauthorizedAccessException("You do not have permission to manage this traceability record.");
        }

        traceability.Summary = request.Summary.Trim();
        traceability.UpdatedAt = DateTime.UtcNow;

        traceability.MaterialSources.Clear();
        AttachMaterialSources(traceability, request.MaterialSources);

        traceability.TimelineEvents.Clear();
        AttachTimelineEvents(traceability, request.TimelineEvents);

        await _traceabilityRepository.SaveChangesAsync(cancellationToken);

        return ToDto(traceability);
    }

    public async Task DeleteAsync(Guid id, Guid producerId, bool isAdmin, CancellationToken cancellationToken)
    {
        var traceability = await _traceabilityRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Traceability record not found.");

        if (!isAdmin && traceability.Product.ProducerId != producerId)
        {
            throw new UnauthorizedAccessException("You do not have permission to manage this traceability record.");
        }

        _traceabilityRepository.Remove(traceability);
        await _traceabilityRepository.SaveChangesAsync(cancellationToken);
    }

    private static void AttachMaterialSources(ProductTraceability traceability, List<MaterialSourceInput> materialSources)
    {
        for (var i = 0; i < materialSources.Count; i++)
        {
            traceability.MaterialSources.Add(new MaterialSource
            {
                Id = Guid.NewGuid(),
                MaterialName = materialSources[i].MaterialName.Trim(),
                SourceLocation = materialSources[i].SourceLocation.Trim(),
                Description = materialSources[i].Description.Trim(),
                DisplayOrder = i,
            });
        }
    }

    private static void AttachTimelineEvents(ProductTraceability traceability, List<TimelineEventInput> timelineEvents)
    {
        for (var i = 0; i < timelineEvents.Count; i++)
        {
            traceability.TimelineEvents.Add(new TimelineEvent
            {
                Id = Guid.NewGuid(),
                Title = timelineEvents[i].Title.Trim(),
                Description = timelineEvents[i].Description.Trim(),
                Location = string.IsNullOrWhiteSpace(timelineEvents[i].Location) ? null : timelineEvents[i].Location!.Trim(),
                EventDate = timelineEvents[i].EventDate,
                DisplayOrder = i,
            });
        }
    }

    private static ProductTraceabilityDto ToDto(ProductTraceability traceability) => new()
    {
        Id = traceability.Id,
        ProductId = traceability.ProductId,
        ProductName = traceability.Product.Name,
        Summary = traceability.Summary,
        MaterialSources = traceability.MaterialSources
            .OrderBy(m => m.DisplayOrder)
            .Select(m => new MaterialSourceDto
            {
                MaterialName = m.MaterialName,
                SourceLocation = m.SourceLocation,
                Description = m.Description,
                DisplayOrder = m.DisplayOrder,
            })
            .ToList(),
        TimelineEvents = traceability.TimelineEvents
            .OrderBy(t => t.DisplayOrder)
            .Select(t => new TimelineEventDto
            {
                Title = t.Title,
                Description = t.Description,
                Location = t.Location,
                EventDate = t.EventDate,
                DisplayOrder = t.DisplayOrder,
            })
            .ToList(),
        CreatedAt = traceability.CreatedAt,
        UpdatedAt = traceability.UpdatedAt,
    };
}
