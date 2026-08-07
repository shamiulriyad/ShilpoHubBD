using ShilpoHubBD.Application.DTOs.Community;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Community;

namespace ShilpoHubBD.Application.Services.Community;

public class VillageService : IVillageService
{
    private readonly IVillageRepository _villageRepository;
    private readonly IDistrictRepository _districtRepository;

    public VillageService(IVillageRepository villageRepository, IDistrictRepository districtRepository)
    {
        _villageRepository = villageRepository;
        _districtRepository = districtRepository;
    }

    public async Task<List<VillageDto>> GetAllAsync(Guid? currentUserId, CancellationToken cancellationToken)
    {
        var villages = await _villageRepository.GetAllAsync(includeInactive: false, cancellationToken);
        var favoritedIds = currentUserId.HasValue
            ? await _villageRepository.GetFavoritedVillageIdsAsync(currentUserId.Value, cancellationToken)
            : new HashSet<Guid>();

        return villages.Select(v => ToDto(v, favoritedIds.Contains(v.Id))).ToList();
    }

    public async Task<VillageDto> GetByIdAsync(Guid id, Guid? currentUserId, CancellationToken cancellationToken)
    {
        var village = await _villageRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Village not found.");

        var isFavorited = currentUserId.HasValue &&
            await _villageRepository.GetFavoriteAsync(currentUserId.Value, id, cancellationToken) is not null;

        return ToDto(village, isFavorited);
    }

    public async Task<VillageDto> CreateAsync(CreateVillageRequest request, CancellationToken cancellationToken)
    {
        if (await _districtRepository.GetByIdAsync(request.DistrictId, cancellationToken) is null)
        {
            throw new NotFoundException("District not found.");
        }

        var now = DateTime.UtcNow;
        var village = new Village
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Craft = request.Craft.Trim(),
            Description = request.Description?.Trim(),
            ImageUrl = request.ImageUrl?.Trim(),
            DistrictId = request.DistrictId,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await _villageRepository.AddAsync(village, cancellationToken);
        await _villageRepository.SaveChangesAsync(cancellationToken);

        var created = await _villageRepository.GetByIdAsync(village.Id, cancellationToken);
        return ToDto(created!, false);
    }

    public async Task<VillageDto> UpdateAsync(Guid id, UpdateVillageRequest request, CancellationToken cancellationToken)
    {
        var village = await _villageRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Village not found.");

        if (await _districtRepository.GetByIdAsync(request.DistrictId, cancellationToken) is null)
        {
            throw new NotFoundException("District not found.");
        }

        village.Name = request.Name.Trim();
        village.Craft = request.Craft.Trim();
        village.Description = request.Description?.Trim();
        village.ImageUrl = request.ImageUrl?.Trim();
        village.DistrictId = request.DistrictId;
        village.IsActive = request.IsActive;
        village.UpdatedAt = DateTime.UtcNow;

        await _villageRepository.SaveChangesAsync(cancellationToken);

        var updated = await _villageRepository.GetByIdAsync(id, cancellationToken);
        return ToDto(updated!, false);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var village = await _villageRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Village not found.");

        _villageRepository.Remove(village);
        await _villageRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<VillageDto>> GetMyFavoritesAsync(Guid userId, CancellationToken cancellationToken)
    {
        var favorites = await _villageRepository.GetFavoritesByUserAsync(userId, cancellationToken);
        return favorites.Select(f => ToDto(f.Village, true)).ToList();
    }

    public async Task FavoriteAsync(Guid userId, Guid villageId, CancellationToken cancellationToken)
    {
        if (await _villageRepository.GetByIdAsync(villageId, cancellationToken) is null)
        {
            throw new NotFoundException("Village not found.");
        }

        if (await _villageRepository.GetFavoriteAsync(userId, villageId, cancellationToken) is not null)
        {
            return;
        }

        await _villageRepository.AddFavoriteAsync(new VillageFavorite
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            VillageId = villageId,
            CreatedAt = DateTime.UtcNow,
        }, cancellationToken);

        await _villageRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task UnfavoriteAsync(Guid userId, Guid villageId, CancellationToken cancellationToken)
    {
        var favorite = await _villageRepository.GetFavoriteAsync(userId, villageId, cancellationToken)
            ?? throw new NotFoundException("This village is not in your favorites.");

        _villageRepository.RemoveFavorite(favorite);
        await _villageRepository.SaveChangesAsync(cancellationToken);
    }

    private static VillageDto ToDto(Village village, bool isFavorited) => new()
    {
        Id = village.Id,
        Name = village.Name,
        Craft = village.Craft,
        Description = village.Description,
        ImageUrl = village.ImageUrl,
        IsActive = village.IsActive,
        DistrictId = village.DistrictId,
        DistrictName = village.District.Name,
        IsFavorited = isFavorited,
    };
}
