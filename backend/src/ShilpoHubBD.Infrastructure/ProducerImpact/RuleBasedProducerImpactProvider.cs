using ShilpoHubBD.Application.DTOs.Governance;
using ShilpoHubBD.Application.Interfaces.Services;

namespace ShilpoHubBD.Infrastructure.ProducerImpact;

/// <summary>
/// Deterministic, template-based fallback used when Gemini has no API key configured or a call fails.
/// Every sentence is built directly from the already-classified metrics in the context — nothing is
/// invented, and no metric is ever reclassified (Status is used exactly as given).
/// </summary>
public class RuleBasedProducerImpactProvider : IProducerImpactAIProvider
{
    public Task<ProducerImpactNarrativeDto> GenerateNarrativeAsync(ProducerImpactAiContext context, CancellationToken cancellationToken)
    {
        var findings = new List<ProducerImpactFindingDto>();

        AddMainPerformanceChanges(context, findings);
        AddIdentifiedProblems(context, findings);
        AddObservedImprovements(context, findings);
        AddAreasNeedingAttention(context, findings);
        AddPossibleSupportAreas(context, findings);

        return Task.FromResult(new ProducerImpactNarrativeDto
        {
            IsAiGenerated = false,
            ProviderName = "RuleBased",
            Findings = findings,
        });
    }

    private static void AddMainPerformanceChanges(ProducerImpactAiContext context, List<ProducerImpactFindingDto> findings)
    {
        var measured = context.Metrics.Where(m => m.Status != "InsufficientData").ToList();
        if (measured.Count == 0)
        {
            Add(findings, ProducerImpactFindingCategoryStrings.MainPerformanceChange, Fact,
                "There is not enough monthly report data around the support date to describe a performance change.");
            return;
        }

        foreach (var m in measured)
        {
            Add(findings, ProducerImpactFindingCategoryStrings.MainPerformanceChange, Fact,
                m.ChangePercentage.HasValue
                    ? $"{Label(m.MetricType)} moved from {m.BeforeValue} to {m.AfterValue} ({Signed(m.ChangePercentage.Value)}%)."
                    : $"{Label(m.MetricType)} moved from {m.BeforeValue} to {m.AfterValue}.");
        }
    }

    private static void AddIdentifiedProblems(ProducerImpactAiContext context, List<ProducerImpactFindingDto> findings)
    {
        var declined = context.Metrics.Where(m => m.Status == "Declined").ToList();
        if (declined.Count == 0)
        {
            Add(findings, ProducerImpactFindingCategoryStrings.IdentifiedProblem, Fact,
                "No metric was classified as Declined in this comparison.");
            return;
        }

        foreach (var m in declined)
        {
            Add(findings, ProducerImpactFindingCategoryStrings.IdentifiedProblem, Fact,
                m.ChangePercentage.HasValue
                    ? $"{Label(m.MetricType)} declined ({Signed(m.ChangePercentage.Value)}%), from {m.BeforeValue} to {m.AfterValue}."
                    : $"{Label(m.MetricType)} declined, from {m.BeforeValue} to {m.AfterValue}.");
        }
    }

    private static void AddObservedImprovements(ProducerImpactAiContext context, List<ProducerImpactFindingDto> findings)
    {
        var improved = context.Metrics.Where(m => m.Status == "Improved").ToList();
        if (improved.Count == 0)
        {
            Add(findings, ProducerImpactFindingCategoryStrings.ObservedImprovement, Fact,
                "No metric met the improvement threshold in this comparison.");
            return;
        }

        // Never state the support caused the change — only that it coincided with the same period,
        // and only when both months' data actually exist (MonthsUsedForComparison == 2, which is
        // guaranteed here since a metric can only be Improved when both values are known).
        foreach (var m in improved)
        {
            Add(findings, ProducerImpactFindingCategoryStrings.ObservedImprovement, Fact,
                $"{Label(m.MetricType)} improved by {Signed(m.ChangePercentage!.Value)}% in the month the {context.SupportType} support was provided in, compared to the month before. " +
                "This is a coincidence in timing, not proof that the support caused the improvement.");
        }
    }

    private static void AddAreasNeedingAttention(ProducerImpactAiContext context, List<ProducerImpactFindingDto> findings)
    {
        var needsAttention = context.Metrics.Where(m => m.Status is "Declined" or "NoSignificantChange").ToList();
        foreach (var m in needsAttention)
        {
            Add(findings, ProducerImpactFindingCategoryStrings.AreaNeedingAttention, Recommendation,
                m.Status == "Declined"
                    ? $"Consider reviewing {Label(m.MetricType)} — it declined since the support was provided."
                    : $"Consider monitoring {Label(m.MetricType)} — it did not show a significant change since the support was provided.");
        }

        if (context.CategoryAverageSales.HasValue)
        {
            var afterSales = context.Metrics.FirstOrDefault(m => m.MetricType == "Sales")?.AfterValue;
            if (afterSales.HasValue && afterSales.Value < context.CategoryAverageSales.Value)
            {
                Add(findings, ProducerImpactFindingCategoryStrings.AreaNeedingAttention, Recommendation,
                    $"Sales after support ({afterSales.Value}) remain below the {context.CategoryName ?? "category"} average ({context.CategoryAverageSales.Value}).");
            }
        }

        if (context.DistrictAverageSales.HasValue)
        {
            var afterSales = context.Metrics.FirstOrDefault(m => m.MetricType == "Sales")?.AfterValue;
            if (afterSales.HasValue && afterSales.Value < context.DistrictAverageSales.Value)
            {
                Add(findings, ProducerImpactFindingCategoryStrings.AreaNeedingAttention, Recommendation,
                    $"Sales after support ({afterSales.Value}) remain below the {context.DistrictName ?? "district"} average ({context.DistrictAverageSales.Value}).");
            }
        }

        if (findings.All(f => f.Category != ProducerImpactFindingCategoryStrings.AreaNeedingAttention))
        {
            Add(findings, ProducerImpactFindingCategoryStrings.AreaNeedingAttention, Recommendation,
                "No specific area stands out for attention from the current data.");
        }
    }

    private static void AddPossibleSupportAreas(ProducerImpactAiContext context, List<ProducerImpactFindingDto> findings)
    {
        var weak = context.Metrics.Where(m => m.Status is "Declined" or "NoSignificantChange").Select(m => m.MetricType).ToHashSet();

        if (weak.Contains("Sales") || weak.Contains("NetIncome"))
        {
            Add(findings, ProducerImpactFindingCategoryStrings.PossibleSupportArea, Recommendation,
                "Consider marketing or market-access support to help grow sales and income.");
        }

        if (weak.Contains("Orders"))
        {
            Add(findings, ProducerImpactFindingCategoryStrings.PossibleSupportArea, Recommendation,
                "Consider support that helps convert interest into orders, such as marketing or improved product presentation.");
        }

        if (weak.Contains("CancellationRate"))
        {
            Add(findings, ProducerImpactFindingCategoryStrings.PossibleSupportArea, Recommendation,
                "Consider logistics or quality-control support to help reduce order cancellations.");
        }

        if (weak.Contains("AverageRating"))
        {
            Add(findings, ProducerImpactFindingCategoryStrings.PossibleSupportArea, Recommendation,
                "Consider quality-improvement or customer-service training to help raise ratings.");
        }

        if (findings.All(f => f.Category != ProducerImpactFindingCategoryStrings.PossibleSupportArea))
        {
            Add(findings, ProducerImpactFindingCategoryStrings.PossibleSupportArea, Recommendation,
                "No specific support area stands out from the current data; continue monitoring in future months.");
        }
    }

    private const string Fact = "MeasuredFact";
    private const string Recommendation = "Recommendation";

    private static void Add(List<ProducerImpactFindingDto> findings, string category, string kind, string text)
        => findings.Add(new ProducerImpactFindingDto { Category = category, Kind = kind, Text = text });

    private static string Signed(decimal value) => value >= 0 ? $"+{value}" : value.ToString();

    private static string Label(string metricType) => metricType switch
    {
        "NetIncome" => "Net income",
        "AverageRating" => "Average rating",
        "CancellationRate" => "Cancellation rate",
        "UnitsSold" => "Units sold",
        "CustomerRetention" => "Customer retention",
        "ProductActivity" => "Product activity",
        _ => metricType,
    };
}

internal static class ProducerImpactFindingCategoryStrings
{
    public const string MainPerformanceChange = "MainPerformanceChange";
    public const string IdentifiedProblem = "IdentifiedProblem";
    public const string ObservedImprovement = "ObservedImprovement";
    public const string AreaNeedingAttention = "AreaNeedingAttention";
    public const string PossibleSupportArea = "PossibleSupportArea";
}
