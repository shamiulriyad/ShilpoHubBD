using ShilpoHubBD.Application.DTOs.Passport;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Passport;

namespace ShilpoHubBD.Application.Services.Passport;

public class PassportService : IPassportService
{
    private readonly IPassportRepository _passportRepository;
    private readonly IDistrictRepository _districtRepository;
    private readonly IOrderRepository _orderRepository;

    public PassportService(
        IPassportRepository passportRepository,
        IDistrictRepository districtRepository,
        IOrderRepository orderRepository)
    {
        _passportRepository = passportRepository;
        _districtRepository = districtRepository;
        _orderRepository = orderRepository;
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
