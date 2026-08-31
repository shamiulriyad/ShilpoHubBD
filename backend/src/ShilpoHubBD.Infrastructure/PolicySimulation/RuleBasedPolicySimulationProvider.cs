using System.Text.Json;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Governance;

namespace ShilpoHubBD.Infrastructure.PolicySimulation;

/// <summary>
/// Rule-based stand-in for a future ML/forecasting backend behind the Government / NGO Policy
/// Simulator. Every projection is a transparent elasticity applied to the live baseline it is handed
/// -- no external calls, no model weights. Swap for a real <see cref="IPolicySimulationProvider"/>
/// later without touching the service or controller.
/// </summary>
public class RuleBasedPolicySimulationProvider : IPolicySimulationProvider
{
    public const string Name = "rule-based-policy-sim-v1";

    public string ProviderName => Name;

    public PolicySimulationResult Simulate(PolicySimulationInput input)
    {
        var horizonYears = Math.Max(0.25, input.HorizonMonths / 12.0);
        var intensity = Math.Clamp((input.IntensityPercent ?? 60) / 100.0, 0.1, 1.0);
        var b = input.Baseline;

        return input.SimulationType switch
        {
            PolicySimulationType.GrantProgram => Grant(input, b, horizonYears, intensity),
            PolicySimulationType.TrainingProgram => Training(input, b, horizonYears, intensity),
            PolicySimulationType.TourismCampaign => Tourism(input, b, horizonYears, intensity),
            PolicySimulationType.ExportStrategy => Export(input, b, horizonYears, intensity),
            PolicySimulationType.EmploymentPrediction => Employment(input, b, horizonYears, intensity),
            _ => throw new ArgumentOutOfRangeException(nameof(input), input.SimulationType, "Unknown simulation type."),
        };
    }

    // ---- Grant -------------------------------------------------------

    private static PolicySimulationResult Grant(
        PolicySimulationInput input, PolicyBaselineSignals b, double years, double intensity)
    {
        const decimal grantPerProducer = 25_000m;
        const double outputUplift = 0.18;
        const double workersPerProducer = 0.4;

        var funded = input.TargetBeneficiaries
            ?? (input.Budget.HasValue ? (int)(input.Budget.Value / grantPerProducer) : 0);
        funded = Math.Max(0, funded);
        var fundedShare = b.ActiveProducers > 0 ? Math.Min(1.0, (double)funded / b.ActiveProducers) : 0;
        var horizonFactor = Math.Min(1.0, years);

        var economyProjected = Scale(b.EconomyValue, 1 + fundedShare * outputUplift * intensity * horizonFactor);
        var salesProjected = Scale(b.MarketplaceSalesValue, 1 + fundedShare * outputUplift * intensity * horizonFactor);
        var reactivated = (int)Math.Round(funded * 0.12 * intensity);
        var extraJobs = (int)Math.Round(funded * workersPerProducer * intensity * horizonFactor);

        var confidence = Confidence(b.EconomyValue > 0 && funded > 0, funded > 0 && fundedShare < 0.8);
        var assumptions = Json(new
        {
            grantPerProducer,
            fundedProducers = funded,
            outputUpliftPerFundedProducer = outputUplift,
            intensity,
            horizonYears = years,
        });

        var projections = new List<PolicyProjectionResult>
        {
            Proj("HeritageEconomyValue", "BDT", b.EconomyValue, economyProjected, confidence,
                $"{funded} producers funded (~{fundedShare:P0} of the active base)."),
            Proj("MarketplaceSalesValue", "BDT", b.MarketplaceSalesValue, salesProjected, confidence, null),
            Proj("ActiveProducers", "count", b.ActiveProducers, b.ActiveProducers + reactivated, confidence,
                $"~{reactivated} dormant producers re-activated by the grant."),
            Proj("Employment", "people", b.Employment, b.Employment + extraJobs, LowerOne(confidence),
                $"~{workersPerProducer} additional workers per funded producer."),
        };

        var recs = new List<PolicyRecommendationResult>
        {
            Rec(PolicyRecommendationPriority.High, "Tie disbursement to verified output",
                "Release grant tranches against sales or production evidence to keep the output elasticity realistic."),
            Rec(PolicyRecommendationPriority.Medium, "Prioritise at-risk crafts",
                "Direct at least half of the grants toward crafts flagged High/Critical in the Heritage Risk Index."),
        };
        if (funded == 0)
        {
            recs.Insert(0, Rec(PolicyRecommendationPriority.High, "Provide a budget or beneficiary target",
                "No budget or target beneficiaries were supplied, so the grant projection is effectively flat."));
        }

        return new PolicySimulationResult(
            $"A grant of {Money(input.Budget)} reaching ~{funded} producers is projected to lift the heritage economy "
            + $"for {input.ScopeLabel} by {Pct(b.EconomyValue, economyProjected)} over {input.HorizonMonths} months.",
            Name, confidence, assumptions, projections, recs);
    }

    // ---- Training ------------------------------------------------

    private static PolicySimulationResult Training(
        PolicySimulationInput input, PolicyBaselineSignals b, double years, double intensity)
    {
        const decimal costPerTrainee = 12_000m;
        const double completionRate = 0.7;
        const double absorptionRate = 0.6;
        const decimal outputPerNewProducer = 180_000m;

        var trainees = input.TargetBeneficiaries
            ?? (input.Budget.HasValue ? (int)(input.Budget.Value / costPerTrainee) : 0);
        trainees = Math.Max(0, trainees);
        var graduates = (int)Math.Round(trainees * completionRate * intensity);
        var newProducers = (int)Math.Round(graduates * absorptionRate * Math.Min(1.0, years));
        var economyProjected = b.EconomyValue + newProducers * outputPerNewProducer;

        var confidence = Confidence(trainees > 0, b.ApprenticesInPipeline >= 0 && trainees < 5000);
        var assumptions = Json(new
        {
            costPerTrainee, completionRate, absorptionRate, outputPerNewProducer,
            trainees, graduates, intensity, horizonYears = years,
        });

        var projections = new List<PolicyProjectionResult>
        {
            Proj("ApprenticesInPipeline", "people", b.ApprenticesInPipeline, b.ApprenticesInPipeline + trainees,
                confidence, $"{trainees} trainees enter the pipeline."),
            Proj("ActiveProducers", "count", b.ActiveProducers, b.ActiveProducers + newProducers, confidence,
                $"{graduates} graduates, ~{absorptionRate:P0} becoming active producers."),
            Proj("Employment", "people", b.Employment, b.Employment + graduates, LowerOne(confidence), null),
            Proj("HeritageEconomyValue", "BDT", b.EconomyValue, economyProjected, LowerOne(confidence),
                $"~{Money(outputPerNewProducer)} annual output per new producer."),
        };

        var recs = new List<PolicyRecommendationResult>
        {
            Rec(PolicyRecommendationPriority.High, "Pair training with market access",
                "Graduates only convert to active producers if they have a route to sell — bundle with marketplace onboarding."),
            Rec(PolicyRecommendationPriority.Medium, "Track completion, not enrolment",
                "Fund providers on graduate outcomes so the completion-rate assumption holds."),
        };
        if (trainees == 0)
        {
            recs.Insert(0, Rec(PolicyRecommendationPriority.High, "Provide a budget or trainee target",
                "Nothing was supplied to size the cohort; the projection is flat."));
        }

        return new PolicySimulationResult(
            $"Training ~{trainees} people for {input.ScopeLabel} is projected to add ~{newProducers} active producers "
            + $"and lift employment by {graduates} over {input.HorizonMonths} months.",
            Name, confidence, assumptions, projections, recs);
    }

    // ---- Tourism campaign -------------------------------------

    private static PolicySimulationResult Tourism(
        PolicySimulationInput input, PolicyBaselineSignals b, double years, double intensity)
    {
        const decimal acquisitionCostPerVisitor = 400m;
        const decimal fallbackBookingValue = 1_500m;
        const double marketplaceSpillover = 0.1;

        var avgBookingValue = b.TourismBookings > 0 && b.TourismRevenue > 0
            ? b.TourismRevenue / b.TourismBookings
            : fallbackBookingValue;
        var newBookings = input.Budget.HasValue
            ? (int)(input.Budget.Value / acquisitionCostPerVisitor * (decimal)intensity)
            : (input.TargetBeneficiaries ?? 0);
        newBookings = Math.Max(0, newBookings);

        var tourismRevenueProjected = b.TourismRevenue + newBookings * avgBookingValue;
        var tourismDelta = tourismRevenueProjected - b.TourismRevenue;
        var economyProjected = b.EconomyValue + tourismDelta + tourismDelta * (decimal)marketplaceSpillover;
        var tourismJobs = (int)(tourismDelta / 200_000m);

        var confidence = Confidence(newBookings > 0, b.TourismBookings > 0);
        var assumptions = Json(new
        {
            acquisitionCostPerVisitor, avgBookingValue, marketplaceSpillover,
            newBookings, intensity, horizonYears = years,
        });

        var projections = new List<PolicyProjectionResult>
        {
            Proj("TourismBookings", "count", b.TourismBookings, b.TourismBookings + newBookings, confidence,
                $"~{Money(acquisitionCostPerVisitor)} acquisition cost per visitor."),
            Proj("TourismRevenue", "BDT", b.TourismRevenue, tourismRevenueProjected, confidence,
                $"Average booking value ~{Money(avgBookingValue)}."),
            Proj("HeritageEconomyValue", "BDT", b.EconomyValue, economyProjected, LowerOne(confidence),
                $"Includes ~{marketplaceSpillover:P0} spillover into craft sales."),
            Proj("Employment", "people", b.Employment, b.Employment + tourismJobs, LowerOne(confidence), null),
        };

        var recs = new List<PolicyRecommendationResult>
        {
            Rec(PolicyRecommendationPriority.Medium, "Route campaigns through heritage routes & festivals",
                "Attach the campaign to existing festivals/routes so acquisition cost stays near the assumed figure."),
            Rec(PolicyRecommendationPriority.Medium, "Bundle craft purchases with visits",
                "Offer on-site purchase or shipping to capture the marketplace spillover."),
        };
        if (newBookings == 0)
        {
            recs.Insert(0, Rec(PolicyRecommendationPriority.High, "Provide a campaign budget",
                "No budget or booking target supplied; tourism projection is flat."));
        }

        return new PolicySimulationResult(
            $"A tourism campaign for {input.ScopeLabel} is projected to add ~{newBookings} bookings and "
            + $"{Pct(b.TourismRevenue, tourismRevenueProjected)} tourism revenue over {input.HorizonMonths} months.",
            Name, confidence, assumptions, projections, recs);
    }

    // ---- Export strategy ------------------------------------

    private static PolicySimulationResult Export(
        PolicySimulationInput input, PolicyBaselineSignals b, double years, double intensity)
    {
        const double exportRoi = 3.5;
        const decimal jobsPerExportBdt = 300_000m;

        var baseFacilitation = input.Budget ?? 0m;
        var maturityBoost = b.ExportValue > 0 ? 1.15 : 0.8; // existing exporters compound faster
        var exportDelta = baseFacilitation * (decimal)(exportRoi * intensity * maturityBoost * Math.Min(1.5, years));
        var exportProjected = b.ExportValue + exportDelta;
        var economyProjected = b.EconomyValue + exportDelta;
        var exportJobs = (int)(exportDelta / jobsPerExportBdt);
        var newExportProducers = (int)Math.Round((double)(exportDelta / 500_000m));

        var confidence = Confidence(baseFacilitation > 0, b.ExportValue > 0);
        var assumptions = Json(new
        {
            exportRoiPerBdt = exportRoi, maturityBoost, jobsPerExportBdt,
            facilitationBudget = baseFacilitation, intensity, horizonYears = years,
        });

        var projections = new List<PolicyProjectionResult>
        {
            Proj("ExportRevenue", "BDT", b.ExportValue, exportProjected, confidence,
                $"ROI of {exportRoi:0.0}x on facilitation spend, maturity factor {maturityBoost:0.00}."),
            Proj("HeritageEconomyValue", "BDT", b.EconomyValue, economyProjected, LowerOne(confidence), null),
            Proj("ActiveProducers", "count", b.ActiveProducers, b.ActiveProducers + newExportProducers, LowerOne(confidence),
                $"~{newExportProducers} producers entering export channels."),
            Proj("Employment", "people", b.Employment, b.Employment + exportJobs, LowerOne(confidence), null),
        };

        var recs = new List<PolicyRecommendationResult>
        {
            Rec(PolicyRecommendationPriority.High, "Concentrate on existing exporter partners first",
                "ROI is materially higher where an export channel already exists — scale those before recruiting new ones."),
            Rec(PolicyRecommendationPriority.Medium, "De-risk logistics and certification",
                "Fund shared compliance/logistics so smaller producers can join without fixed-cost barriers."),
        };
        if (baseFacilitation == 0)
        {
            recs.Insert(0, Rec(PolicyRecommendationPriority.High, "Provide a facilitation budget",
                "No budget supplied; export projection is flat."));
        }

        return new PolicySimulationResult(
            $"An export strategy for {input.ScopeLabel} with {Money(input.Budget)} facilitation is projected to lift "
            + $"export revenue by {Pct(b.ExportValue, exportProjected)} over {input.HorizonMonths} months.",
            Name, confidence, assumptions, projections, recs);
    }

    // ---- Employment prediction ----------------------------

    private static PolicySimulationResult Employment(
        PolicySimulationInput input, PolicyBaselineSignals b, double years, double intensity)
    {
        const double organicAnnualGrowth = 0.05;
        const double pipelineAbsorption = 0.55;

        var organic = (int)Math.Round(b.Employment * organicAnnualGrowth * years);
        var fromPipeline = (int)Math.Round(b.ApprenticesInPipeline * pipelineAbsorption * Math.Min(1.0, years));
        var fromProgram = (int)Math.Round((input.TargetBeneficiaries ?? 0) * intensity);
        var employmentProjected = b.Employment + organic + fromPipeline + fromProgram;
        var producerGrowth = (int)Math.Round(b.ActiveProducers * organicAnnualGrowth * years);

        var confidence = Confidence(b.Employment > 0 || b.ApprenticesInPipeline > 0, b.Employment > 0);
        var assumptions = Json(new
        {
            organicAnnualGrowth, pipelineAbsorption,
            organicJobs = organic, pipelineJobs = fromPipeline, programmeJobs = fromProgram,
            horizonYears = years,
        });

        var projections = new List<PolicyProjectionResult>
        {
            Proj("Employment", "people", b.Employment, employmentProjected, confidence,
                $"{organic} organic + {fromPipeline} from the training pipeline + {fromProgram} from a jobs programme."),
            Proj("ActiveProducers", "count", b.ActiveProducers, b.ActiveProducers + producerGrowth, LowerOne(confidence), null),
            Proj("HeritageEconomyValue", "BDT", b.EconomyValue,
                Scale(b.EconomyValue, 1 + organicAnnualGrowth * years), LowerOne(confidence), null),
        };

        var recs = new List<PolicyRecommendationResult>
        {
            Rec(PolicyRecommendationPriority.Medium, "Protect the training pipeline",
                $"About {fromPipeline} of the projected jobs depend on current apprentices converting — keep that funnel funded."),
            Rec(PolicyRecommendationPriority.Low, "Re-run quarterly",
                "Employment momentum shifts with orders and enrolments; refresh the prediction each quarter."),
        };

        return new PolicySimulationResult(
            $"Employment for {input.ScopeLabel} is projected at ~{employmentProjected:N0} "
            + $"({Pct(b.Employment, employmentProjected)}) over {input.HorizonMonths} months on current momentum.",
            Name, confidence, assumptions, projections, recs);
    }

    // ---- helpers -----------------------------------------------

    private static PolicyProjectionResult Proj(
        string metric, string unit, decimal baseline, decimal projected,
        PolicySimulationConfidence confidence, string? detail)
        => new(metric, unit, Math.Round(baseline, 2), Math.Round(projected, 2), confidence, detail);

    private static PolicyRecommendationResult Rec(PolicyRecommendationPriority p, string title, string detail)
        => new(p, title, detail);

    private static decimal Scale(decimal value, double factor) => Math.Round(value * (decimal)factor, 2);

    private static PolicySimulationConfidence Confidence(bool hasInput, bool baselineRealistic)
        => (hasInput, baselineRealistic) switch
        {
            (true, true) => PolicySimulationConfidence.Moderate,
            (true, false) => PolicySimulationConfidence.Low,
            _ => PolicySimulationConfidence.Low,
        };

    private static PolicySimulationConfidence LowerOne(PolicySimulationConfidence c)
        => c == PolicySimulationConfidence.High ? PolicySimulationConfidence.Moderate
            : c == PolicySimulationConfidence.Moderate ? PolicySimulationConfidence.Low
            : PolicySimulationConfidence.Low;

    private static string Json(object o) => JsonSerializer.Serialize(o);

    private static string Money(decimal? v) => v.HasValue ? $"BDT {v.Value:N0}" : "no budget";

    private static string Pct(decimal from, decimal to)
    {
        if (from == 0)
        {
            return to == 0 ? "0%" : "a new baseline";
        }

        var p = (double)((to - from) / from) * 100;
        return $"{(p >= 0 ? "+" : string.Empty)}{p:0.#}%";
    }
}
