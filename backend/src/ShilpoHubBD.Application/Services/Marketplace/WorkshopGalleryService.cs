using ShilpoHubBD.Application.DTOs.Marketplace;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Application.Services.Marketplace;

public class WorkshopGalleryService : IWorkshopGalleryService
{
    private readonly IWorkshopGalleryRepository _workshopGalleryRepository;
    private readonly IUserRepository _userRepository;

    public WorkshopGalleryService(IWorkshopGalleryRepository workshopGalleryRepository, IUserRepository userRepository)
    {
        _workshopGalleryRepository = workshopGalleryRepository;
        _userRepository = userRepository;
    }

    public async Task<List<WorkshopGalleryItemDto>> GetByProducerIdAsync(Guid producerId, CancellationToken cancellationToken)
    {
        var items = await _workshopGalleryRepository.GetByProducerIdAsync(producerId, cancellationToken);
        return items.Select(ToDto).ToList();
    }

    public async Task<WorkshopGalleryItemDto> AddAsync(Guid producerId, Guid currentUserId, bool isAdmin, CreateWorkshopGalleryItemRequest request, CancellationToken cancellationToken)
    {
        if (!isAdmin && producerId != currentUserId)
        {
            throw new UnauthorizedAccessException("You do not have permission to modify this producer's workshop gallery.");
        }

        if (await _userRepository.GetByIdAsync(producerId, cancellationToken) is null)
        {
            throw new NotFoundException("Producer not found.");
        }

        var item = new WorkshopGalleryItem
        {
            Id = Guid.NewGuid(),
            ProducerId = producerId,
            MediaUrl = request.MediaUrl.Trim(),
            MediaType = request.MediaType,
            Caption = request.Caption?.Trim(),
            DisplayOrder = request.DisplayOrder,
            CreatedAt = DateTime.UtcNow,
        };

        await _workshopGalleryRepository.AddAsync(item, cancellationToken);
        await _workshopGalleryRepository.SaveChangesAsync(cancellationToken);

        return ToDto(item);
    }

    public async Task DeleteAsync(Guid producerId, Guid itemId, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken)
    {
        if (!isAdmin && producerId != currentUserId)
        {
            throw new UnauthorizedAccessException("You do not have permission to modify this producer's workshop gallery.");
        }

        var item = await _workshopGalleryRepository.GetByIdAsync(itemId, cancellationToken);
        if (item is null || item.ProducerId != producerId)
        {
            throw new NotFoundException("Workshop gallery item not found.");
        }

        _workshopGalleryRepository.Remove(item);
        await _workshopGalleryRepository.SaveChangesAsync(cancellationToken);
    }

    private static WorkshopGalleryItemDto ToDto(WorkshopGalleryItem item) => new()
    {
        Id = item.Id,
        ProducerId = item.ProducerId,
        MediaUrl = item.MediaUrl,
        MediaType = item.MediaType.ToString(),
        Caption = item.Caption,
        DisplayOrder = item.DisplayOrder,
    };
}
