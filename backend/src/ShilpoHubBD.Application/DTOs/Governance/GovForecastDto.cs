namespace ShilpoHubBD.Application.DTOs.Governance;

public class GovForecastDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public int HorizonMonths { get; set; }
    public DateTime BaselineAsOf { get; set; }
    public string? AssumptionsJson { get; set; }
    public string Summary { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    public Guid GeneratedByUserId { get; set; }
    public string? GeneratedByName { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<GovForecastSeriesDto> Series { get; set; } = new();
}

public class GovForecastSeriesDto
{
    public string Metric { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public decimal BaselineValue { get; set; }
    public List<GovForecastPointDto> Points { get; set; } = new();
}

public class GovForecastPointDto
{
    public int MonthOffset { get; set; }
    public DateTime PeriodDate { get; set; }
    public decimal ProjectedValue { get; set; }
    public decimal LowerBound { get; set; }
    public decimal UpperBound { get; set; }
    public string Confidence { get; set; } = string.Empty;
}

public class GovForecastListItemDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int HorizonMonths { get; set; }
    public DateTime BaselineAsOf { get; set; }
    public DateTime GeneratedAt { get; set; }
    public string? GeneratedByName { get; set; }
}

public class GenerateGovForecastRequest
{
    public string Title { get; set; } = string.Empty;

    /// <summary>Months to project forward (1–60). Defaults to 12.</summary>
    public int? HorizonMonths { get; set; }

    /// <summary>When false the forecast is returned but not saved. Defaults to true.</summary>
    public bool Persist { get; set; } = true;
}

public class GovForecastQueryParameters
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
