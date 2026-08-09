using ShilpoHubBD.Application.DTOs.AIIntelligence;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IBusinessPartnerAIService
{
    Task<SupplierRankingResult> RankSuppliersAsync(SupplierRankingRequest request, CancellationToken cancellationToken);
    Task<QualityPredictionResult> PredictQualityAsync(QualityPredictionRequest request, CancellationToken cancellationToken);
    Task<PriceForecastResult> ForecastPriceAsync(PriceForecastRequest request, CancellationToken cancellationToken);
    Task<DeliveryPredictionResult> PredictDeliveryAsync(DeliveryPredictionRequest request, CancellationToken cancellationToken);
    Task<RiskAssessmentResult> AssessRiskAsync(RiskAssessmentRequest request, CancellationToken cancellationToken);
}
