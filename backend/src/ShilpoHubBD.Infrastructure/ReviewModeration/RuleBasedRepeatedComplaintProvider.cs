using ShilpoHubBD.Application.DTOs.Reviews;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Reviews;

namespace ShilpoHubBD.Infrastructure.ReviewModeration;

/// <summary>
/// Deterministic fallback used when Gemini is unavailable: a complaint is "repeated" only if at least one
/// ACTUALLY RETRIEVED historical review shares the new review's own complaint type — never invented, always
/// traceable to real rows the caller already fetched from Postgres.
/// </summary>
public class RuleBasedRepeatedComplaintProvider : IRepeatedComplaintAIProvider
{
    private const int MinMatchingComplaints = 1;

    public Task<RepeatedComplaintResultDto> CompareAsync(RepeatedComplaintContext context, CancellationToken cancellationToken)
    {
        var matching = context.HistoricalReviews
            .Where(h => h.ComplaintType == context.NewComplaintType)
            .ToList();

        if (context.NewComplaintType == ReviewComplaintType.None || matching.Count < MinMatchingComplaints)
        {
            return Task.FromResult(new RepeatedComplaintResultDto
            {
                IsRepeatedComplaint = false,
                ComplaintType = context.NewComplaintType,
                Severity = context.NewSeverity,
                Reason = $"None of the {context.HistoricalReviews.Count} retrieved historical review(s) reported the same complaint type.",
                Confidence = 0.4,
                IsAiGenerated = false,
            });
        }

        var maxSeverity = matching.Select(m => m.Severity).Append(context.NewSeverity).Max();
        var sameProductCount = matching.Count(m => m.SameProduct);

        return Task.FromResult(new RepeatedComplaintResultDto
        {
            IsRepeatedComplaint = true,
            ComplaintType = context.NewComplaintType,
            Severity = maxSeverity,
            Reason = $"{matching.Count} retrieved historical review(s) ({sameProductCount} for this same product) reported a similar " +
                $"{context.NewComplaintType} issue.",
            Confidence = Math.Min(0.4 + 0.1 * matching.Count, 0.75),
            IsAiGenerated = false,
        });
    }
}
