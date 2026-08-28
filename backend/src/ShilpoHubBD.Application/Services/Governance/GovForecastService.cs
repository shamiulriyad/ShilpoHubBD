using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Governance;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Governance;

namespace ShilpoHubBD.Application.Services.Governance;

/// <summary>
/// Government &amp; NGO "AI Predictions": projects national heritage-economy metrics forward from
/// dashboard-snapshot history and current figures via a replaceable rule-based forecasting provider.
/// No real ML model yet.
/// </summary>
public class GovForecastService : IGovForecastService
{
    private const int DefaultHorizonMonths = 12;

    private readonly IGovAnalyticsRepository _repository;
    private readonly IGovForecastProvider _provider;

    public GovForecastService(IGovAnalyticsRepository repository, IGovForecastProvider provider)
    {
        _repository = repository;
        _provider = provider;
    }

    public async Task<GovForecastDto> GenerateAsync(
        Guid userId, GenerateGovForecastRequest request, CancellationToken cancellationToken)
    {
        var horizon = Math.Clamp(request.HorizonMonths ?? DefaultHorizonMonths, 1, 60);
        var input = await _repository.GatherForecastInputAsync(cancellationToken);

        var result = _provider.Forecast(new GovForecastInput
        {
            HorizonMonths = horizon,
            BaselineAsOf = input.AsOf,
            CurrentValues = input.CurrentValues,
            History = input.History,
        });

        var now = DateTime.UtcNow;
        var forecast = new GovForecast
        {
            Id = Guid.NewGuid(),
            Title = request.Title.Trim(),
            Method = result.Method,
            HorizonMonths = horizon,
            BaselineAsOf = input.AsOf,
            AssumptionsJson = result.AssumptionsJson,
            Summary = result.Summary,
            GeneratedAt = now,
            GeneratedByUserId = userId,
            CreatedAt = now,
        };

        var order = 0;
        foreach (var series in result.Series)
        {
            foreach (var p in series.Projections)
            {
                forecast.Points.Add(new GovForecastPoint
                {
                    Id = Guid.NewGuid(),
                    GovForecastId = forecast.Id,
                    Metric = series.Metric,
                    Unit = series.Unit,
                    MonthOffset = p.MonthOffset,
                    PeriodDate = p.PeriodDate,
                    BaselineValue = series.BaselineValue,
                    ProjectedValue = p.ProjectedValue,
                    LowerBound = p.LowerBound,
                    UpperBound = p.UpperBound,
                    Confidence = p.Confidence,
                    DisplayOrder = order++,
                });
            }
        }

        if (!request.Persist)
        {
            return forecast.ToDto();
        }

        await _repository.AddForecastAsync(forecast, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return (await _repository.GetForecastByIdAsync(forecast.Id, cancellationToken))!.ToDto();
    }

    public async Task<PagedResult<GovForecastListItemDto>> GetForecastsAsync(
        GovForecastQueryParameters query, CancellationToken cancellationToken)
    {
        query.Page = query.Page < 1 ? 1 : query.Page;
        query.PageSize = query.PageSize is < 1 or > 100 ? 20 : query.PageSize;

        var (items, totalCount) = await _repository.GetForecastsPagedAsync(query, cancellationToken);

        return new PagedResult<GovForecastListItemDto>
        {
            Items = items.Select(f => f.ToListItemDto()).ToList(),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }

    public async Task<GovForecastDto> GetForecastByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var forecast = await _repository.GetForecastByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Forecast not found.");
        return forecast.ToDto();
    }

    public async Task DeleteForecastAsync(Guid id, CancellationToken cancellationToken)
    {
        var forecast = await _repository.GetForecastByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Forecast not found.");

        _repository.RemoveForecast(forecast);
        await _repository.SaveChangesAsync(cancellationToken);
    }
}
