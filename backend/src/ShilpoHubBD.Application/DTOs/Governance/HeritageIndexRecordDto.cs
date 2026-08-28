namespace ShilpoHubBD.Application.DTOs.Governance;

public class HeritageIndexRecordDto
{
    public Guid Id { get; set; }
    public string IndexType { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;
    public Guid? ScopeId { get; set; }
    public string ScopeLabel { get; set; } = string.Empty;

    public decimal Score { get; set; }
    public string Rating { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;

    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public DateTime ComputedAt { get; set; }

    public string? Notes { get; set; }
    public Guid GeneratedByUserId { get; set; }
    public string? GeneratedByName { get; set; }
    public DateTime CreatedAt { get; set; }

    public List<HeritageIndexComponentDto> Components { get; set; } = new();
}

public class HeritageIndexComponentDto
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public decimal RawValue { get; set; }
    public decimal Weight { get; set; }
    public decimal ContributionScore { get; set; }
    public string? Detail { get; set; }
    public int DisplayOrder { get; set; }
}

public class HeritageIndexRecordListItemDto
{
    public Guid Id { get; set; }
    public string IndexType { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;
    public Guid? ScopeId { get; set; }
    public string ScopeLabel { get; set; } = string.Empty;
    public decimal Score { get; set; }
    public string Rating { get; set; } = string.Empty;
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public DateTime ComputedAt { get; set; }
    public string? GeneratedByName { get; set; }
}
