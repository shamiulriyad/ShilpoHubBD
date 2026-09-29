using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Logistics;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;
using ShilpoHubBD.Domain.Entities.Logistics;

namespace ShilpoHubBD.Application.Services.Logistics;

/// <summary>
/// Onboarding and administration of Logistics Partner operational profiles, including the districts
/// each partner services. A partner user manages only their own profile; SuperAdmin manages any
/// profile and is the only role that can change verification status.
/// </summary>
public class LogisticsPartnerService : ILogisticsPartnerService
{
    private readonly ILogisticsPartnerRepository _repository;

    public LogisticsPartnerService(ILogisticsPartnerRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<AvailableLogisticsOptionDto>> GetAvailableAsync(
        Guid districtId, string? areaName, CancellationToken cancellationToken)
    {
        var partners = await _repository.GetAvailableForLocationAsync(districtId, areaName, cancellationToken);
        var normalized = string.IsNullOrWhiteSpace(areaName) ? null : areaName.Trim();
        return partners.SelectMany(p => p.ServiceAreas
                .Where(a => a.IsActive && a.DistrictId == districtId
                    && (a.AreaName == null || string.Equals(a.AreaName, normalized, StringComparison.OrdinalIgnoreCase)))
                // Exact-area terms override district-wide terms for the same method.
                .GroupBy(a => a.DeliveryMethod, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.OrderByDescending(a => a.AreaName != null).First())
                .Select(a => new AvailableLogisticsOptionDto
                {
                    PartnerId = p.Id,
                    CompanyName = p.CompanyName,
                    LogoUrl = p.LogoUrl,
                    ServiceAreaId = a.Id,
                    DeliveryMethod = a.DeliveryMethod,
                    EstimatedDeliveryDays = a.StandardDeliveryDays,
                    DeliveryCharge = a.DeliveryCharge + (a.SurchargeAmount ?? 0),
                    PickupAvailable = a.PickupAvailable,
                    ReturnSupported = a.ReturnSupported,
                }))
            .OrderBy(x => x.DeliveryCharge).ThenBy(x => x.CompanyName).ToList();
    }

    public async Task<LogisticsPartnerPerformanceDto> GetPerformanceAsync(Guid profileId, CancellationToken cancellationToken)
    {
        _ = await _repository.GetByIdAsync(profileId, cancellationToken)
            ?? throw new NotFoundException("Logistics partner profile not found.");
        return await _repository.GetPerformanceAsync(profileId, cancellationToken);
    }

    public async Task<LogisticsPartnerProfileDto> CreateOfficialAsync(
        Guid adminUserId, UpsertLogisticsPartnerProfileRequest request, CancellationToken cancellationToken)
    {
        ValidateOperatingHours(request.OperatingDayStartHour, request.OperatingDayEndHour);
        var profile = new LogisticsPartnerProfile
        {
            Id = Guid.NewGuid(),
            VerificationStatus = LogisticsPartnerVerificationStatus.Verified,
            VerifiedByUserId = adminUserId,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
        };
        await ApplyProfileAsync(profile, request, cancellationToken);
        await _repository.AddAsync(profile, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        return (await _repository.GetByIdAsync(profile.Id, cancellationToken))!.ToDto();
    }

    public async Task<LogisticsPartnerProfileDto> UpdateOfficialAsync(
        Guid profileId, Guid adminUserId, UpsertLogisticsPartnerProfileRequest request, CancellationToken cancellationToken)
    {
        var profile = await _repository.GetByIdAsync(profileId, cancellationToken)
            ?? throw new NotFoundException("Logistics partner profile not found.");
        ValidateOperatingHours(request.OperatingDayStartHour, request.OperatingDayEndHour);
        await ApplyProfileAsync(profile, request, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        return (await _repository.GetByIdAsync(profile.Id, cancellationToken))!.ToDto();
    }

    public async Task<LogisticsPartnerProfileDto> UpsertOfficialServiceAreaAsync(
        Guid profileId, UpsertLogisticsServiceAreaRequest request, CancellationToken cancellationToken)
    {
        var profile = await _repository.GetByIdAsync(profileId, cancellationToken)
            ?? throw new NotFoundException("Logistics partner profile not found.");
        var district = await _repository.GetDistrictAsync(request.DistrictId, cancellationToken)
            ?? throw new ConflictException("District not found.");
        var areaName = string.IsNullOrWhiteSpace(request.AreaName) ? null : request.AreaName.Trim();
        var method = request.DeliveryMethod.Trim();
        var area = profile.ServiceAreas.FirstOrDefault(a => a.DistrictId == request.DistrictId
            && string.Equals(a.AreaName, areaName, StringComparison.OrdinalIgnoreCase)
            && string.Equals(a.DeliveryMethod, method, StringComparison.OrdinalIgnoreCase));
        if (area is null)
        {
            area = new LogisticsServiceArea { Id = Guid.NewGuid(), LogisticsPartnerProfileId = profile.Id, DistrictId = district.Id };
            profile.ServiceAreas.Add(area);
        }
        area.DistrictName = district.Name;
        area.Division = district.Division;
        area.AreaName = areaName;
        area.DeliveryMethod = method;
        area.StandardDeliveryDays = request.StandardDeliveryDays;
        area.SupportsSameDay = request.SupportsSameDay;
        area.SurchargeAmount = request.SurchargeAmount;
        area.DeliveryCharge = request.DeliveryCharge;
        area.PickupAvailable = request.PickupAvailable;
        area.ReturnSupported = request.ReturnSupported;
        area.IsActive = request.IsActive;
        profile.UpdatedAt = DateTime.UtcNow;
        await _repository.SaveChangesAsync(cancellationToken);
        return (await _repository.GetByIdAsync(profile.Id, cancellationToken))!.ToDto();
    }

    public async Task<LogisticsPartnerProfileDto> RemoveOfficialServiceAreaAsync(
        Guid profileId, Guid serviceAreaId, CancellationToken cancellationToken)
    {
        var profile = await _repository.GetByIdAsync(profileId, cancellationToken)
            ?? throw new NotFoundException("Logistics partner profile not found.");
        var area = profile.ServiceAreas.FirstOrDefault(a => a.Id == serviceAreaId)
            ?? throw new NotFoundException("Service area not found.");
        profile.ServiceAreas.Remove(area);
        profile.UpdatedAt = DateTime.UtcNow;
        await _repository.SaveChangesAsync(cancellationToken);
        return (await _repository.GetByIdAsync(profile.Id, cancellationToken))!.ToDto();
    }

    public async Task<LogisticsPartnerProfileDto> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var profile = await _repository.GetByUserIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("Logistics partner profile not found.");
        return profile.ToDto();
    }

    public async Task<LogisticsPartnerProfileDto> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var profile = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Logistics partner profile not found.");
        return profile.ToDto();
    }

    public async Task<PagedResult<LogisticsPartnerProfileListItemDto>> GetPagedAsync(
        LogisticsPartnerQueryParameters query, CancellationToken cancellationToken)
    {
        query.Page = query.Page < 1 ? 1 : query.Page;
        query.PageSize = query.PageSize is < 1 or > 100 ? 20 : query.PageSize;

        var (items, totalCount) = await _repository.GetPagedAsync(query, cancellationToken);

        return new PagedResult<LogisticsPartnerProfileListItemDto>
        {
            Items = items.Select(p => p.ToListItemDto()).ToList(),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }

    public async Task<LogisticsPartnerProfileDto> UpsertAsync(
        Guid targetUserId, Guid currentUserId, bool isAdmin,
        UpsertLogisticsPartnerProfileRequest request, CancellationToken cancellationToken)
    {
        EnsureCanManage(targetUserId, currentUserId, isAdmin);
        ValidateOperatingHours(request.OperatingDayStartHour, request.OperatingDayEndHour);

        var now = DateTime.UtcNow;
        var profile = await _repository.GetByUserIdAsync(targetUserId, cancellationToken);

        if (profile is null)
        {
            if (!await _repository.UserInRoleAsync(targetUserId, RoleNames.LogisticsPartner, cancellationToken))
            {
                throw new ConflictException("The target user is not in the LogisticsPartner role.");
            }

            profile = new LogisticsPartnerProfile
            {
                Id = Guid.NewGuid(),
                UserId = targetUserId,
                VerificationStatus = LogisticsPartnerVerificationStatus.Pending,
                CreatedAt = now,
            };
            await _repository.AddAsync(profile, cancellationToken);
        }

        await ApplyDistrictAsync(profile, request.BaseDistrictId, cancellationToken);

        profile.CompanyName = request.CompanyName.Trim();
        profile.LegalName = request.LegalName?.Trim();
        profile.RegistrationNumber = request.RegistrationNumber?.Trim();
        profile.ContactPersonName = request.ContactPersonName.Trim();
        profile.ContactPhone = request.ContactPhone.Trim();
        profile.ContactEmail = request.ContactEmail.Trim();
        profile.BaseAddressLine = request.BaseAddressLine.Trim();
        profile.BaseCity = request.BaseCity.Trim();
        profile.BasePostalCode = request.BasePostalCode?.Trim();
        profile.Country = string.IsNullOrWhiteSpace(request.Country) ? "Bangladesh" : request.Country.Trim();
        profile.FleetSize = request.FleetSize;
        profile.MaxDailyPickups = request.MaxDailyPickups;
        profile.MaxVehicleCapacityKg = request.MaxVehicleCapacityKg;
        profile.OperatingDayStartHour = request.OperatingDayStartHour;
        profile.OperatingDayEndHour = request.OperatingDayEndHour;
        profile.OffersCashOnDelivery = request.OffersCashOnDelivery;
        profile.OffersColdChain = request.OffersColdChain;
        profile.OffersFragileHandling = request.OffersFragileHandling;
        profile.IsAcceptingRequests = request.IsAcceptingRequests;
        profile.Notes = request.Notes?.Trim();
        profile.UpdatedAt = now;

        await _repository.SaveChangesAsync(cancellationToken);

        return (await _repository.GetByIdAsync(profile.Id, cancellationToken))!.ToDto();
    }

    public async Task<LogisticsPartnerProfileDto> VerifyAsync(
        Guid targetUserId, Guid verifierUserId, VerifyLogisticsPartnerRequest request,
        CancellationToken cancellationToken)
    {
        var profile = await _repository.GetByUserIdAsync(targetUserId, cancellationToken)
            ?? throw new NotFoundException("Logistics partner profile not found.");

        var status = ParseEnum<LogisticsPartnerVerificationStatus>(
            request.Status,
            "Status must be one of: Pending, Verified, Rejected, Suspended.");

        var now = DateTime.UtcNow;
        profile.VerificationStatus = status;
        profile.VerificationNotes = request.Notes?.Trim();
        profile.VerifiedByUserId = verifierUserId;
        profile.VerifiedAt = now;
        profile.UpdatedAt = now;

        if (status is LogisticsPartnerVerificationStatus.Rejected or LogisticsPartnerVerificationStatus.Suspended)
        {
            profile.IsAcceptingRequests = false;
        }

        await _repository.SaveChangesAsync(cancellationToken);

        return (await _repository.GetByIdAsync(profile.Id, cancellationToken))!.ToDto();
    }

    public async Task<LogisticsPartnerProfileDto> UpsertServiceAreaAsync(
        Guid targetUserId, Guid currentUserId, bool isAdmin,
        UpsertLogisticsServiceAreaRequest request, CancellationToken cancellationToken)
    {
        EnsureCanManage(targetUserId, currentUserId, isAdmin);

        var profile = await _repository.GetByUserIdAsync(targetUserId, cancellationToken)
            ?? throw new NotFoundException("Logistics partner profile not found.");

        var district = await _repository.GetDistrictAsync(request.DistrictId, cancellationToken)
            ?? throw new ConflictException("District not found.");

        var area = profile.ServiceAreas.FirstOrDefault(a => a.DistrictId == request.DistrictId);
        if (area is null)
        {
            area = new LogisticsServiceArea
            {
                Id = Guid.NewGuid(),
                LogisticsPartnerProfileId = profile.Id,
                DistrictId = district.Id,
            };
            profile.ServiceAreas.Add(area);
        }

        area.DistrictName = district.Name;
        area.Division = district.Division;
        area.StandardDeliveryDays = request.StandardDeliveryDays;
        area.SupportsSameDay = request.SupportsSameDay;
        area.SurchargeAmount = request.SurchargeAmount;
        area.IsActive = request.IsActive;

        profile.UpdatedAt = DateTime.UtcNow;
        await _repository.SaveChangesAsync(cancellationToken);

        return (await _repository.GetByIdAsync(profile.Id, cancellationToken))!.ToDto();
    }

    public async Task<LogisticsPartnerProfileDto> RemoveServiceAreaAsync(
        Guid targetUserId, Guid currentUserId, bool isAdmin, Guid serviceAreaId,
        CancellationToken cancellationToken)
    {
        EnsureCanManage(targetUserId, currentUserId, isAdmin);

        var profile = await _repository.GetByUserIdAsync(targetUserId, cancellationToken)
            ?? throw new NotFoundException("Logistics partner profile not found.");

        var area = profile.ServiceAreas.FirstOrDefault(a => a.Id == serviceAreaId)
            ?? throw new NotFoundException("Service area not found.");

        profile.ServiceAreas.Remove(area);
        profile.UpdatedAt = DateTime.UtcNow;
        await _repository.SaveChangesAsync(cancellationToken);

        return (await _repository.GetByIdAsync(profile.Id, cancellationToken))!.ToDto();
    }

    public async Task DeleteAsync(
        Guid targetUserId, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken)
    {
        EnsureCanManage(targetUserId, currentUserId, isAdmin);

        var profile = await _repository.GetByUserIdAsync(targetUserId, cancellationToken)
            ?? throw new NotFoundException("Logistics partner profile not found.");

        if (await _repository.HasPickupRequestsAsync(profile.Id, cancellationToken))
        {
            throw new ConflictException("Cannot delete a profile that has pickup requests.");
        }

        _repository.Remove(profile);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    // ---- helpers -----------------------------------------------------

    private async Task ApplyDistrictAsync(
        LogisticsPartnerProfile profile, Guid? districtId, CancellationToken cancellationToken)
    {
        if (districtId is null)
        {
            profile.BaseDistrictId = null;
            return;
        }

        var district = await _repository.GetDistrictAsync(districtId.Value, cancellationToken)
            ?? throw new ConflictException("Base district not found.");
        profile.BaseDistrictId = district.Id;
    }

    private async Task ApplyProfileAsync(LogisticsPartnerProfile profile, UpsertLogisticsPartnerProfileRequest request, CancellationToken cancellationToken)
    {
        await ApplyDistrictAsync(profile, request.BaseDistrictId, cancellationToken);
        profile.CompanyName = request.CompanyName.Trim();
        profile.LogoUrl = request.LogoUrl?.Trim();
        profile.Description = request.Description?.Trim();
        profile.LegalName = request.LegalName?.Trim();
        profile.RegistrationNumber = request.RegistrationNumber?.Trim();
        profile.ContactPersonName = request.ContactPersonName.Trim();
        profile.ContactPhone = request.ContactPhone.Trim();
        profile.ContactEmail = request.ContactEmail.Trim();
        profile.BaseAddressLine = request.BaseAddressLine.Trim();
        profile.BaseCity = request.BaseCity.Trim();
        profile.BasePostalCode = request.BasePostalCode?.Trim();
        profile.Country = string.IsNullOrWhiteSpace(request.Country) ? "Bangladesh" : request.Country.Trim();
        profile.FleetSize = request.FleetSize;
        profile.MaxDailyPickups = request.MaxDailyPickups;
        profile.MaxVehicleCapacityKg = request.MaxVehicleCapacityKg;
        profile.OperatingDayStartHour = request.OperatingDayStartHour;
        profile.OperatingDayEndHour = request.OperatingDayEndHour;
        profile.OffersCashOnDelivery = request.OffersCashOnDelivery;
        profile.OffersColdChain = request.OffersColdChain;
        profile.OffersFragileHandling = request.OffersFragileHandling;
        profile.OffersPickup = request.OffersPickup;
        profile.SupportsReturns = request.SupportsReturns;
        profile.IsAcceptingRequests = request.IsAcceptingRequests;
        profile.IsActive = request.IsActive;
        profile.Notes = request.Notes?.Trim();
        profile.UpdatedAt = DateTime.UtcNow;
    }

    private static void EnsureCanManage(Guid targetUserId, Guid currentUserId, bool isAdmin)
    {
        if (!isAdmin && targetUserId != currentUserId)
        {
            throw new UnauthorizedAccessException("You can only manage your own logistics partner profile.");
        }
    }

    private static void ValidateOperatingHours(int? start, int? end)
    {
        if (start is < 0 or > 24 || end is < 0 or > 24)
        {
            throw new ConflictException("Operating hours must be between 0 and 24.");
        }

        if (start.HasValue && end.HasValue && end.Value <= start.Value)
        {
            throw new ConflictException("OperatingDayEndHour must be after OperatingDayStartHour.");
        }
    }

    private static T ParseEnum<T>(string value, string message) where T : struct, Enum
        => Enum.TryParse<T>(value, true, out var parsed) ? parsed : throw new ConflictException(message);
}
