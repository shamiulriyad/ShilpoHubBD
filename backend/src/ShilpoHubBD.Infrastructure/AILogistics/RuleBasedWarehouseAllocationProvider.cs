using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Logistics;

namespace ShilpoHubBD.Infrastructure.AILogistics;

/// <summary>
/// Rule-based stand-in for a warehouse-allocation optimiser. Each active candidate gets a
/// capacity-headroom score, a proximity score (same district as destination = best), a cold-chain
/// fit score and a low-utilisation score; the four are combined with weights chosen by the
/// objective. No external calls.
/// </summary>
public class RuleBasedWarehouseAllocationProvider : IWarehouseAllocationProvider
{
    public const string Name = "rule-based-warehouse-allocation-v1";

    public string ProviderName => Name;

    public WarehouseAllocationResult Recommend(WarehouseAllocationInput input)
    {
        var weights = Weights(input.Objective);
        var qty = Math.Max(0, input.Quantity ?? 0);

        var scored = new List<(WarehouseCandidate C, double Score, double ProjUtil, bool SameDistrict, string Why)>();
        foreach (var c in input.Candidates)
        {
            var active = c.Status.Equals("Active", StringComparison.OrdinalIgnoreCase);
            if (!active)
            {
                continue;
            }

            if (input.RequireColdChain && !c.HasColdChain)
            {
                continue;
            }

            var capacity = c.TotalCapacityUnits > 0 ? c.TotalCapacityUnits : Math.Max(1, c.UsedCapacityUnits);
            var free = Math.Max(0, capacity - c.UsedCapacityUnits);
            if (qty > 0 && free < qty)
            {
                continue;
            }

            var headroomScore = c.TotalCapacityUnits > 0
                ? Math.Clamp((free - qty) / (double)capacity, 0.0, 1.0)
                : 0.6;

            var projUtil = c.TotalCapacityUnits > 0
                ? Math.Clamp((c.UsedCapacityUnits + qty) * 100.0 / capacity, 0.0, 200.0)
                : 0.0;

            var sameDistrict = input.DestinationDistrictId.HasValue
                && c.DistrictId.HasValue
                && c.DistrictId.Value == input.DestinationDistrictId.Value;
            var proximityScore = !input.DestinationDistrictId.HasValue
                ? 0.5
                : sameDistrict ? 1.0 : c.DistrictId.HasValue ? 0.35 : 0.25;

            var coldChainScore = !input.RequireColdChain
                ? c.HasColdChain ? 0.7 : 0.6
                : 1.0;

            var utilisationScore = c.TotalCapacityUnits > 0
                ? Math.Clamp(1.0 - c.UsedCapacityUnits / (double)capacity, 0.0, 1.0)
                : 0.5;

            var score =
                weights.Headroom * headroomScore
                + weights.Proximity * proximityScore
                + weights.ColdChain * coldChainScore
                + weights.Utilisation * utilisationScore;

            var why =
                $"headroom {Math.Round(headroomScore, 2)}, proximity {Math.Round(proximityScore, 2)}, "
                + $"cold-chain {Math.Round(coldChainScore, 2)}, low-util {Math.Round(utilisationScore, 2)}"
                + (sameDistrict ? "; same district as destination" : string.Empty)
                + (qty > 0 ? $"; {free} free unit(s)" : string.Empty);

            scored.Add((c, score, Math.Round(projUtil, 1), sameDistrict, why));
        }

        var ranked = scored
            .OrderByDescending(s => s.Score)
            .ThenBy(s => s.ProjUtil)
            .ThenBy(s => s.C.Code)
            .ToList();

        var options = ranked
            .Select((s, i) => new WarehouseAllocationOptionResult(
                s.C.WarehouseId, s.C.Code, s.C.Name, i + 1,
                Math.Round(s.Score, 4), s.ProjUtil, s.SameDistrict, s.Why))
            .ToList();

        var confidence = options.Count switch
        {
            >= 4 => AiLogisticsConfidence.High,
            >= 2 => AiLogisticsConfidence.Moderate,
            _ => AiLogisticsConfidence.Low,
        };

        var summary = options.Count == 0
            ? "No active candidate warehouse matched the constraints."
            : $"Ranked {options.Count} warehouse(s) for objective '{input.Objective}'; "
              + $"top pick {options[0].Code} (score {options[0].Score}).";

        return new WarehouseAllocationResult(
            Name,
            summary,
            confidence,
            options.Count > 0 ? options[0].WarehouseId : (Guid?)null,
            options);
    }

    private static (double Headroom, double Proximity, double ColdChain, double Utilisation) Weights(string objective)
        => objective.ToLowerInvariant() switch
        {
            "proximity" => (0.15, 0.60, 0.05, 0.20),
            "capacity" => (0.55, 0.10, 0.05, 0.30),
            "coldchain" => (0.20, 0.20, 0.45, 0.15),
            "cost" => (0.30, 0.40, 0.05, 0.25),
            _ => (0.30, 0.30, 0.15, 0.25), // balanced
        };
}
