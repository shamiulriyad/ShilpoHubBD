namespace ShilpoHubBD.Domain.Entities.Governance;

/// <summary>One weighted sub-factor that fed into a <see cref="HeritageIndexRecord"/>'s score.</summary>
public class HeritageIndexComponent
{
    public Guid Id { get; set; }

    public Guid HeritageIndexRecordId { get; set; }
    public HeritageIndexRecord Record { get; set; } = null!;

    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;

    /// <summary>The underlying signal value (a count, ratio or sum).</summary>
    public decimal RawValue { get; set; }

    /// <summary>Weight applied to this factor in the composite (0–1).</summary>
    public decimal Weight { get; set; }

    /// <summary>Points this factor contributed toward the final 0–100 score.</summary>
    public decimal ContributionScore { get; set; }

    public string? Detail { get; set; }

    public int DisplayOrder { get; set; }
}
