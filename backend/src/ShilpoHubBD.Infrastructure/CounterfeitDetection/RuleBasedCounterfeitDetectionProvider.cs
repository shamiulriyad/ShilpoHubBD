using ShilpoHubBD.Application.DTOs.CounterfeitDetection;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Infrastructure.CounterfeitDetection;

/// <summary>Heuristic risk scoring from signals already on the product record — no image/visual analysis
/// is performed (that would need a real vision model). Swap in a model-backed
/// <see cref="ICounterfeitDetectionProvider"/> later if desired.</summary>
public class RuleBasedCounterfeitDetectionProvider : ICounterfeitDetectionProvider
{
    public (decimal RiskScore, string RiskLevel, List<string> Signals) Check(CounterfeitCheckContext context)
    {
        decimal score = 0;
        var signals = new List<string>();

        if (context.HandmadeVerificationStatus == HandmadeVerificationStatus.Rejected)
        {
            score += 40;
            signals.Add("Failed handmade verification.");
        }

        if (context.ApprovalStatus == ProductApprovalStatus.Rejected)
        {
            score += 30;
            signals.Add("Listing was rejected at admin approval.");
        }

        if (context.CategorySampleSize >= 5 && context.Price < context.CategoryAveragePrice * 0.4m)
        {
            score += 25;
            signals.Add(
                $"Price (৳{context.Price:N0}) is far below the category average (৳{context.CategoryAveragePrice:N0}).");
        }

        if (context.SalesCount > 20 && context.ReviewCount == 0)
        {
            score += 15;
            signals.Add("Significant sales volume with no reviews.");
        }

        score = Math.Clamp(score, 0, 100);
        var level = score >= 60 ? "High" : score >= 30 ? "Medium" : "Low";

        if (signals.Count == 0)
        {
            signals.Add("No counterfeit risk signals detected.");
        }

        return (score, level, signals);
    }
}
