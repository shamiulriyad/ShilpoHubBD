using ShilpoHubBD.Application.DTOs.AIIntelligence;

namespace ShilpoHubBD.Application.Interfaces.Services;

// Abstraction over the "intelligence" behind Business Partner AI features. Every method is a pure
// function of pre-fetched context -> result, so a future Gemini/OpenAI/custom-ML implementation can
// be registered in place of DummyBusinessPartnerAIProvider without touching BusinessPartnerAIService
// or AIIntelligenceController.
public interface IAIBusinessPartnerProvider
{
    Task<SupplierRankingResult> RankSuppliersAsync(SupplierRankingContext context, CancellationToken cancellationToken);

    Task<QualityPredictionResult> PredictQualityAsync(QualityPredictionContext context, CancellationToken cancellationToken);

    Task<PriceForecastResult> ForecastPriceAsync(PriceForecastContext context, CancellationToken cancellationToken);

    Task<DeliveryPredictionResult> PredictDeliveryAsync(DeliveryPredictionContext context, CancellationToken cancellationToken);

    Task<RiskAssessmentResult> AssessRiskAsync(RiskAssessmentContext context, CancellationToken cancellationToken);
}
