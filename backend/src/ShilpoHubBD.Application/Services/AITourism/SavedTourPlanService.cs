using System.Text.Json;
using ShilpoHubBD.Application.DTOs.AITourism;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Tourism;

namespace ShilpoHubBD.Application.Services.AITourism;

public class SavedTourPlanService : ISavedTourPlanService
{
    private const int MaxPageSize = 50;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly ISavedTourPlanRepository _repository;
    private readonly IDistrictRepository _districtRepository;

    public SavedTourPlanService(ISavedTourPlanRepository repository, IDistrictRepository districtRepository)
    {
        _repository = repository;
        _districtRepository = districtRepository;
    }

    public async Task<Guid> SaveAsync(Guid userId, TourPlanRequest request, TourPlanResult result, CancellationToken cancellationToken)
    {
        var districtName = "Bangladesh";
        if (request.DistrictId.HasValue)
        {
            var district = await _districtRepository.GetByIdAsync(request.DistrictId.Value, cancellationToken);
            districtName = district?.Name ?? districtName;
        }

        var days = Math.Clamp(request.DurationDays, 1, 30);
        var plan = new SavedTourPlan
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = $"{districtName} trip · {days} day{(days == 1 ? "" : "s")}",
            DistrictId = request.DistrictId,
            DistrictName = districtName,
            OriginText = request.OriginText,
            DurationDays = days,
            PartySize = Math.Max(1, request.PartySize),
            StartDate = ToUtc(request.StartDate),
            TransportMode = request.TransportMode,
            TotalEstimatedCost = result.EstimatedBudget?.TotalEstimatedCost,
            IsAiGenerated = result.IsAiGenerated,
            RequestJson = JsonSerializer.Serialize(request),
            PlanJson = JsonSerializer.Serialize(result),
            CreatedAt = DateTime.UtcNow,
        };

        await _repository.AddAsync(plan, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        return plan.Id;
    }

    public async Task<PagedResult<SavedTourPlanSummaryDto>> GetMyPlansAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);
        var (items, total) = await _repository.GetPagedForUserAsync(userId, page, pageSize, cancellationToken);
        return new PagedResult<SavedTourPlanSummaryDto>
        {
            Items = items.Select(p => ToSummary(p, new SavedTourPlanSummaryDto())).ToList(),
            TotalCount = total,
            Page = page,
            PageSize = pageSize,
        };
    }

    public async Task<SavedTourPlanDto> GetMyPlanAsync(Guid userId, Guid id, CancellationToken cancellationToken)
    {
        var plan = await _repository.GetForUserAsync(userId, id, cancellationToken)
            ?? throw new NotFoundException("Saved trip plan not found.");

        var dto = ToSummary(plan, new SavedTourPlanDto());
        dto.Request = JsonSerializer.Deserialize<TourPlanRequest>(plan.RequestJson, JsonOptions) ?? new TourPlanRequest();
        dto.Plan = JsonSerializer.Deserialize<TourPlanResult>(plan.PlanJson, JsonOptions) ?? new TourPlanResult();
        dto.Plan.SavedPlanId = plan.Id;
        return dto;
    }

    public async Task DeleteMyPlanAsync(Guid userId, Guid id, CancellationToken cancellationToken)
    {
        var plan = await _repository.GetForUserAsync(userId, id, cancellationToken)
            ?? throw new NotFoundException("Saved trip plan not found.");
        _repository.Remove(plan);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    // A date typed into the planner form has Kind=Unspecified, which Npgsql refuses to write to a
    // timestamptz column. It is a calendar date, so it is stored as that date at 00:00 UTC.
    private static DateTime? ToUtc(DateTime? value) => value.HasValue
        ? value.Value.Kind == DateTimeKind.Utc ? value.Value : DateTime.SpecifyKind(value.Value.Date, DateTimeKind.Utc)
        : null;

    private static T ToSummary<T>(SavedTourPlan plan, T dto) where T : SavedTourPlanSummaryDto
    {
        dto.Id = plan.Id;
        dto.Title = plan.Title;
        dto.DistrictName = plan.DistrictName;
        dto.OriginText = plan.OriginText;
        dto.DurationDays = plan.DurationDays;
        dto.PartySize = plan.PartySize;
        dto.StartDate = plan.StartDate;
        dto.TransportMode = plan.TransportMode;
        dto.TotalEstimatedCost = plan.TotalEstimatedCost;
        dto.IsAiGenerated = plan.IsAiGenerated;
        dto.CreatedAt = plan.CreatedAt;
        return dto;
    }
}
