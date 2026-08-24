using ShilpoHubBD.Application.DTOs.Achievement;
using ShilpoHubBD.Application.DTOs.Passport;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Passport;

namespace ShilpoHubBD.Application.Services.Passport;

public class PassportService : IPassportService
{
    private const double EarthRadiusKm = 6371.0;
    private const double CheckInMaxDistanceKm = 0.5;
    private const int CheckInXpReward = 20;

    private readonly IPassportRepository _passportRepository;
    private readonly IDistrictRepository _districtRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IHeritageCheckInRepository _checkInRepository;
    private readonly ITravelJournalRepository _journalRepository;
    private readonly IHeritagePlaceRepository _heritagePlaceRepository;
    private readonly IAchievementService _achievementService;

    public PassportService(
        IPassportRepository passportRepository,
        IDistrictRepository districtRepository,
        IOrderRepository orderRepository,
        IHeritageCheckInRepository checkInRepository,
        ITravelJournalRepository journalRepository,
        IHeritagePlaceRepository heritagePlaceRepository,
        IAchievementService achievementService)
    {
        _passportRepository = passportRepository;
        _districtRepository = districtRepository;
        _orderRepository = orderRepository;
        _checkInRepository = checkInRepository;
        _journalRepository = journalRepository;
        _heritagePlaceRepository = heritagePlaceRepository;
        _achievementService = achievementService;
    }

    public async Task<List<BadgeDto>> GetAllBadgesAsync(CancellationToken cancellationToken)
    {
        var badges = await _passportRepository.GetAllBadgesAsync(cancellationToken);
        return badges.Select(ToBadgeDto).ToList();
    }

    public async Task<List<UserBadgeDto>> GetMyBadgesAsync(Guid userId, CancellationToken cancellationToken)
    {
        var userBadges = await _passportRepository.GetUserBadgesAsync(userId, cancellationToken);
        return userBadges.Select(ToUserBadgeDto).ToList();
    }

    public async Task<BadgeDto> CreateBadgeAsync(CreateBadgeRequest request, CancellationToken cancellationToken)
    {
        Domain.Entities.Marketplace.District? district = null;

        if (request.Type == BadgeType.District)
        {
            district = await _districtRepository.GetByIdAsync(request.DistrictId!.Value, cancellationToken)
                ?? throw new NotFoundException("District not found.");

            if (await _passportRepository.GetDistrictBadgeAsync(request.DistrictId.Value, cancellationToken) is not null)
            {
                throw new ConflictException("This district already has a badge.");
            }
        }

        var badge = new Badge
        {
            Id = Guid.NewGuid(),
            Type = request.Type,
            Name = request.Name.Trim(),
            Description = request.Description.Trim(),
            IconUrl = request.IconUrl?.Trim(),
            DistrictId = request.Type == BadgeType.District ? request.DistrictId : null,
            FestivalName = request.Type == BadgeType.Festival ? request.FestivalName!.Trim() : null,
            RequiredPurchaseCount = request.Type == BadgeType.Purchase ? request.RequiredPurchaseCount : null,
            RequiredCheckInCount = request.Type == BadgeType.CheckIn ? request.RequiredCheckInCount : null,
            CreatedAt = DateTime.UtcNow,
        };

        await _passportRepository.AddBadgeAsync(badge, cancellationToken);
        await _passportRepository.SaveChangesAsync(cancellationToken);

        badge.District = district;
        return ToBadgeDto(badge);
    }

    public async Task<UserBadgeDto> ClaimDistrictBadgeAsync(Guid userId, ClaimDistrictBadgeRequest request, CancellationToken cancellationToken)
    {
        var badge = await _passportRepository.GetDistrictBadgeAsync(request.DistrictId, cancellationToken)
            ?? throw new NotFoundException("No badge is configured for this district.");

        if (await _passportRepository.HasUserBadgeAsync(userId, badge.Id, cancellationToken))
        {
            throw new ConflictException("You have already earned this badge.");
        }

        if (!await _orderRepository.HasCompletedOrderFromDistrictAsync(userId, request.DistrictId, cancellationToken))
        {
            throw new ConflictException("You need a completed order containing a product from this district to earn this badge.");
        }

        return await AwardAsync(userId, badge, cancellationToken);
    }

    public async Task<UserBadgeDto> ClaimFestivalBadgeAsync(Guid userId, ClaimFestivalBadgeRequest request, CancellationToken cancellationToken)
    {
        var badge = await _passportRepository.GetBadgeByIdAsync(request.BadgeId, cancellationToken)
            ?? throw new NotFoundException("Badge not found.");

        if (badge.Type != BadgeType.Festival)
        {
            throw new ConflictException("This is not a festival badge.");
        }

        if (await _passportRepository.HasUserBadgeAsync(userId, badge.Id, cancellationToken))
        {
            throw new ConflictException("You have already earned this badge.");
        }

        return await AwardAsync(userId, badge, cancellationToken);
    }

    public async Task<List<UserBadgeDto>> EvaluatePurchaseBadgesAsync(Guid userId, CancellationToken cancellationToken)
    {
        var purchaseCount = await _orderRepository.GetCompletedOrderCountAsync(userId, cancellationToken);
        var purchaseBadges = await _passportRepository.GetBadgesByTypeAsync(BadgeType.Purchase, cancellationToken);

        var newlyAwarded = new List<UserBadgeDto>();

        foreach (var badge in purchaseBadges.Where(b => b.RequiredPurchaseCount.HasValue && b.RequiredPurchaseCount <= purchaseCount))
        {
            if (await _passportRepository.HasUserBadgeAsync(userId, badge.Id, cancellationToken))
            {
                continue;
            }

            newlyAwarded.Add(await AwardAsync(userId, badge, cancellationToken));
        }

        return newlyAwarded;
    }

    public async Task<List<UserBadgeDto>> EvaluateCheckInBadgesAsync(Guid userId, CancellationToken cancellationToken)
    {
        var checkInCount = await _checkInRepository.GetCheckInCountAsync(userId, cancellationToken);
        var checkInBadges = await _passportRepository.GetBadgesByTypeAsync(BadgeType.CheckIn, cancellationToken);

        var newlyAwarded = new List<UserBadgeDto>();

        foreach (var badge in checkInBadges.Where(b => b.RequiredCheckInCount.HasValue && b.RequiredCheckInCount <= checkInCount))
        {
            if (await _passportRepository.HasUserBadgeAsync(userId, badge.Id, cancellationToken))
            {
                continue;
            }

            newlyAwarded.Add(await AwardAsync(userId, badge, cancellationToken));
        }

        return newlyAwarded;
    }

    public async Task<CheckInDto> CheckInAsync(Guid userId, CreateCheckInRequest request, CancellationToken cancellationToken)
    {
        var place = await _heritagePlaceRepository.GetByIdAsync(request.HeritagePlaceId, cancellationToken)
            ?? throw new NotFoundException("Heritage place not found.");

        if (!place.IsActive)
        {
            throw new ConflictException("This heritage place is not active.");
        }

        if (request.Latitude.HasValue && request.Longitude.HasValue)
        {
            var distanceKm = HaversineDistanceKm(request.Latitude.Value, request.Longitude.Value, place.Latitude, place.Longitude);
            if (distanceKm > CheckInMaxDistanceKm)
            {
                throw new ConflictException("You must be near the heritage place to check in.");
            }
        }

        var now = DateTime.UtcNow;
        var checkInDate = DateOnly.FromDateTime(now);

        if (await _checkInRepository.ExistsForDateAsync(userId, place.Id, checkInDate, cancellationToken))
        {
            throw new ConflictException("You have already checked in to this place today.");
        }

        var checkIn = new HeritageCheckIn
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            HeritagePlaceId = place.Id,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            CheckInDate = checkInDate,
            CheckedInAt = now,
        };

        await _checkInRepository.AddAsync(checkIn, cancellationToken);
        await _checkInRepository.SaveChangesAsync(cancellationToken);

        await EvaluateCheckInBadgesAsync(userId, cancellationToken);
        await _achievementService.AwardXpAsync(
            new AwardXpRequest { UserId = userId, Amount = CheckInXpReward, Reason = $"Heritage check-in: {place.Name}" },
            cancellationToken);

        checkIn.HeritagePlace = place;
        return ToCheckInDto(checkIn);
    }

    public async Task<List<CheckInDto>> GetMyCheckInsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var checkIns = await _checkInRepository.GetMyCheckInsAsync(userId, cancellationToken);
        return checkIns.Select(ToCheckInDto).ToList();
    }

    public async Task<TravelJournalEntryDto> AddJournalEntryAsync(Guid userId, CreateJournalEntryRequest request, CancellationToken cancellationToken)
    {
        Domain.Entities.HeritageDiscovery.HeritagePlace? place = null;
        if (request.HeritagePlaceId.HasValue)
        {
            place = await _heritagePlaceRepository.GetByIdAsync(request.HeritagePlaceId.Value, cancellationToken)
                ?? throw new NotFoundException("Heritage place not found.");
        }

        var now = DateTime.UtcNow;
        var entry = new TravelJournalEntry
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = request.Title.Trim(),
            Content = request.Content.Trim(),
            PhotoUrl = request.PhotoUrl?.Trim(),
            HeritagePlaceId = request.HeritagePlaceId,
            CheckInId = request.CheckInId,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await _journalRepository.AddAsync(entry, cancellationToken);
        await _journalRepository.SaveChangesAsync(cancellationToken);

        entry.HeritagePlace = place;
        return ToJournalEntryDto(entry);
    }

    public async Task<List<TravelJournalEntryDto>> GetMyJournalAsync(Guid userId, CancellationToken cancellationToken)
    {
        var entries = await _journalRepository.GetMyEntriesAsync(userId, cancellationToken);
        return entries.Select(ToJournalEntryDto).ToList();
    }

    public async Task<TravelJournalEntryDto> UpdateJournalEntryAsync(Guid userId, Guid id, UpdateJournalEntryRequest request, CancellationToken cancellationToken)
    {
        var entry = await _journalRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Journal entry not found.");

        if (entry.UserId != userId)
        {
            throw new UnauthorizedAccessException("You do not have permission to modify this journal entry.");
        }

        entry.Title = request.Title.Trim();
        entry.Content = request.Content.Trim();
        entry.PhotoUrl = request.PhotoUrl?.Trim();
        entry.UpdatedAt = DateTime.UtcNow;

        await _journalRepository.SaveChangesAsync(cancellationToken);

        return ToJournalEntryDto(entry);
    }

    public async Task DeleteJournalEntryAsync(Guid userId, Guid id, CancellationToken cancellationToken)
    {
        var entry = await _journalRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Journal entry not found.");

        if (entry.UserId != userId)
        {
            throw new UnauthorizedAccessException("You do not have permission to delete this journal entry.");
        }

        _journalRepository.Remove(entry);
        await _journalRepository.SaveChangesAsync(cancellationToken);
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180.0;

    private static double HaversineDistanceKm(double lat1, double lon1, double lat2, double lon2)
    {
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
            + Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return EarthRadiusKm * c;
    }

    private static CheckInDto ToCheckInDto(HeritageCheckIn checkIn) => new()
    {
        Id = checkIn.Id,
        HeritagePlaceId = checkIn.HeritagePlaceId,
        HeritagePlaceName = checkIn.HeritagePlace.Name,
        Latitude = checkIn.Latitude,
        Longitude = checkIn.Longitude,
        CheckedInAt = checkIn.CheckedInAt,
    };

    private static TravelJournalEntryDto ToJournalEntryDto(TravelJournalEntry entry) => new()
    {
        Id = entry.Id,
        Title = entry.Title,
        Content = entry.Content,
        PhotoUrl = entry.PhotoUrl,
        HeritagePlaceId = entry.HeritagePlaceId,
        HeritagePlaceName = entry.HeritagePlace?.Name,
        CheckInId = entry.CheckInId,
        CreatedAt = entry.CreatedAt,
        UpdatedAt = entry.UpdatedAt,
    };

    private async Task<UserBadgeDto> AwardAsync(Guid userId, Badge badge, CancellationToken cancellationToken)
    {
        var userBadge = new UserBadge
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            BadgeId = badge.Id,
            EarnedAt = DateTime.UtcNow,
        };

        await _passportRepository.AddUserBadgeAsync(userBadge, cancellationToken);
        await _passportRepository.SaveChangesAsync(cancellationToken);

        userBadge.Badge = badge;
        return ToUserBadgeDto(userBadge);
    }

    private static BadgeDto ToBadgeDto(Badge badge) => new()
    {
        Id = badge.Id,
        Type = badge.Type.ToString(),
        Name = badge.Name,
        Description = badge.Description,
        IconUrl = badge.IconUrl,
        DistrictId = badge.DistrictId,
        DistrictName = badge.District?.Name,
        FestivalName = badge.FestivalName,
        RequiredPurchaseCount = badge.RequiredPurchaseCount,
        RequiredCheckInCount = badge.RequiredCheckInCount,
        CreatedAt = badge.CreatedAt,
    };

    private static UserBadgeDto ToUserBadgeDto(UserBadge userBadge) => new()
    {
        Id = userBadge.Id,
        BadgeId = userBadge.BadgeId,
        BadgeName = userBadge.Badge.Name,
        BadgeType = userBadge.Badge.Type.ToString(),
        IconUrl = userBadge.Badge.IconUrl,
        EarnedAt = userBadge.EarnedAt,
    };
}
