using System.Text.Json;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Governance;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Governance;

namespace ShilpoHubBD.Application.Services.Governance;

/// <summary>
/// Government &amp; NGO Policy Simulator. Captures a live baseline, runs the scenario through a
/// replaceable rule-based provider, and stores the inputs, baseline and projected outcomes so
/// scenarios can be compared. No real forecasting model yet. Employment and training-pipeline
/// figures in the baseline are platform-wide levels regardless of scope.
/// </summary>
public class PolicySimulationService : IPolicySimulationService
{
    private const int DefaultHorizonMonths = 12;
    private const int MinHorizonMonths = 3;
    private const int MaxHorizonMonths = 120;
    private const int BaselineWindowMonths = 12;

    private readonly IPolicySimulationRepository _repository;
    private readonly IPolicySimulationProvider _provider;

    public PolicySimulationService(
        IPolicySimulationRepository repository, IPolicySimulationProvider provider)
    {
        _repository = repository;
        _provider = provider;
    }

    public async Task<PolicySimulationDto> RunAsync(
        Guid userId, RunPolicySimulationRequest request, CancellationToken cancellationToken)
    {
        var simulationType = ParseEnum<PolicySimulationType>(request.SimulationType,
            "SimulationType must be one of: GrantProgram, TrainingProgram, TourismCampaign, "
            + "ExportStrategy, EmploymentPrediction.");
        var scope = ParseEnum<HeritageIndexScope>(request.Scope,
            "Scope must be one of: National, District, Village, Craft.");

        var horizonMonths = Math.Clamp(request.HorizonMonths ?? DefaultHorizonMonths, MinHorizonMonths, MaxHorizonMonths);

        if (request.Budget is < 0)
        {
            throw new ConflictException("Budget cannot be negative.");
        }

        if (request.IntensityPercent is < 0 or > 100)
        {
            throw new ConflictException("IntensityPercent must be between 0 and 100.");
        }

        var (scopeId, scopeLabel) = await ResolveScopeAsync(scope, request.ScopeId, cancellationToken);

        var to = DateTime.UtcNow;
        var from = to.AddMonths(-BaselineWindowMonths);
        var baseline = await _repository.GatherBaselineAsync(scope, scopeId, from, to, cancellationToken);

        var input = new PolicySimulationInput
        {
            SimulationType = simulationType,
            Scope = scope,
            ScopeLabel = scopeLabel,
            HorizonMonths = horizonMonths,
            Budget = request.Budget,
            TargetBeneficiaries = request.TargetBeneficiaries,
            DurationMonths = request.DurationMonths,
            IntensityPercent = request.IntensityPercent,
            FocusCraft = request.FocusCraft?.Trim(),
            Baseline = baseline,
        };

        var now = DateTime.UtcNow;
        var simulation = new PolicySimulation
        {
            Id = Guid.NewGuid(),
            Title = request.Title.Trim(),
            SimulationType = simulationType,
            Scope = scope,
            ScopeId = scopeId,
            ScopeLabel = scopeLabel,
            HorizonMonths = horizonMonths,
            InputsJson = JsonSerializer.Serialize(new
            {
                request.Budget,
                request.TargetBeneficiaries,
                request.DurationMonths,
                request.IntensityPercent,
                focusCraft = request.FocusCraft?.Trim(),
            }),
            BaselineProducers = baseline.Producers,
            BaselineActiveProducers = baseline.ActiveProducers,
            BaselineEmployment = baseline.Employment,
            BaselineExportValue = baseline.ExportValue,
            BaselineTourismRevenue = baseline.TourismRevenue,
            BaselineEconomyValue = baseline.EconomyValue,
            Notes = request.Notes?.Trim(),
            GeneratedByUserId = userId,
            CreatedAt = now,
            UpdatedAt = now,
        };

        try
        {
            var result = _provider.Simulate(input);

            simulation.Status = PolicySimulationStatus.Completed;
            simulation.CompletedAt = now;
            simulation.Method = result.Method;
            simulation.Summary = result.Summary;
            simulation.Confidence = result.Confidence;
            simulation.AssumptionsJson = result.AssumptionsJson;

            var pOrder = 0;
            foreach (var p in result.Projections)
            {
                var delta = p.ProjectedValue - p.BaselineValue;
                simulation.Projections.Add(new PolicySimulationProjection
                {
                    Id = Guid.NewGuid(),
                    PolicySimulationId = simulation.Id,
                    Metric = p.Metric,
                    Unit = p.Unit,
                    BaselineValue = p.BaselineValue,
                    ProjectedValue = p.ProjectedValue,
                    DeltaValue = delta,
                    DeltaPercent = p.BaselineValue == 0
                        ? 0
                        : Math.Round((double)(delta / p.BaselineValue) * 100, 2),
                    HorizonMonths = horizonMonths,
                    Confidence = p.Confidence,
                    Detail = p.Detail,
                    DisplayOrder = pOrder++,
                });
            }

            var rOrder = 0;
            foreach (var r in result.Recommendations)
            {
                simulation.Recommendations.Add(new PolicySimulationRecommendation
                {
                    Id = Guid.NewGuid(),
                    PolicySimulationId = simulation.Id,
                    Priority = r.Priority,
                    Title = r.Title,
                    Detail = r.Detail,
                    DisplayOrder = rOrder++,
                });
            }
        }
        catch (Exception ex) when (ex is not ConflictException and not NotFoundException)
        {
            simulation.Status = PolicySimulationStatus.Failed;
            simulation.FailureReason = ex.Message;
        }

        if (!request.Persist)
        {
            return simulation.ToDto();
        }

        await _repository.AddAsync(simulation, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return (await _repository.GetByIdAsync(simulation.Id, cancellationToken))!.ToDto();
    }

    public async Task<PagedResult<PolicySimulationListItemDto>> GetSimulationsAsync(
        PolicySimulationQueryParameters query, CancellationToken cancellationToken)
    {
        query.Page = query.Page < 1 ? 1 : query.Page;
        query.PageSize = query.PageSize is < 1 or > 100 ? 20 : query.PageSize;

        var (items, totalCount) = await _repository.GetPagedAsync(query, cancellationToken);

        return new PagedResult<PolicySimulationListItemDto>
        {
            Items = items.Select(s => s.ToListItemDto()).ToList(),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }

    public async Task<PolicySimulationDto> GetSimulationByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var simulation = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Policy simulation not found.");
        return simulation.ToDto();
    }

    public async Task DeleteSimulationAsync(Guid id, CancellationToken cancellationToken)
    {
        var simulation = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Policy simulation not found.");

        _repository.Remove(simulation);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    // ---- helpers -------------------------------------------------------

    private async Task<(Guid? ScopeId, string ScopeLabel)> ResolveScopeAsync(
        HeritageIndexScope scope, Guid? requestedScopeId, CancellationToken cancellationToken)
    {
        switch (scope)
        {
            case HeritageIndexScope.National:
            case HeritageIndexScope.Craft:
                return (null, scope == HeritageIndexScope.National ? "National" : "All crafts");

            case HeritageIndexScope.District:
                if (requestedScopeId is not { } districtId)
                {
                    throw new ConflictException("ScopeId (district id) is required for District scope.");
                }

                var district = await _repository.GetDistrictAsync(districtId, cancellationToken)
                    ?? throw new NotFoundException("District not found.");
                return (district.Id, district.Name);

            case HeritageIndexScope.Village:
                if (requestedScopeId is not { } villageId)
                {
                    throw new ConflictException("ScopeId (village id) is required for Village scope.");
                }

                var village = await _repository.GetVillageAsync(villageId, cancellationToken)
                    ?? throw new NotFoundException("Village not found.");
                return (village.Id, $"{village.Name} ({village.DistrictName})");

            default:
                throw new ConflictException("Unsupported scope.");
        }
    }

    private static T ParseEnum<T>(string value, string message) where T : struct, Enum
        => Enum.TryParse<T>(value, true, out var parsed) ? parsed : throw new ConflictException(message);
}
