using ShilpoHubBD.Application.DTOs.AIIntelligence;
using ShilpoHubBD.Application.DTOs.SupplierDiscovery;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;

namespace ShilpoHubBD.Application.Services.AIBusinessPartner;

public class BusinessPartnerAIService : IBusinessPartnerAIService
{
    private readonly IAIBusinessPartnerProvider _aiProvider;
    private readonly IAIIntelligenceRepository _aiIntelligenceRepository;
    private readonly ISupplierDiscoveryRepository _supplierDiscoveryRepository;
    private readonly ICategoryRepository _categoryRepository;

    public BusinessPartnerAIService(
        IAIBusinessPartnerProvider aiProvider,
        IAIIntelligenceRepository aiIntelligenceRepository,
        ISupplierDiscoveryRepository supplierDiscoveryRepository,
        ICategoryRepository categoryRepository)
    {
        _aiProvider = aiProvider;
        _aiIntelligenceRepository = aiIntelligenceRepository;
        _supplierDiscoveryRepository = supplierDiscoveryRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<SupplierRankingResult> RankSuppliersAsync(SupplierRankingRequest request, CancellationToken cancellationToken)
    {
        var (candidates, _) = await _supplierDiscoveryRepository.SearchAsync(new SupplierSearchParameters
        {
            CategoryId = request.CategoryId,
            DistrictId = request.DistrictId,
            PageSize = Math.Clamp(request.MaxResults * 3, request.MaxResults, 100),
        }, cancellationToken);

        var context = new SupplierRankingContext
        {
            Candidates = candidates.Select(c => new SupplierRankingCandidateDto
            {
                ProducerId = c.ProducerId,
                ProducerName = c.ProducerName,
                AverageRating = c.AverageRating,
                ReviewCount = c.TotalReviewCount,
                ProductCount = c.ProductCount,
                EstimatedProductionCapacity = c.EstimatedProductionCapacity,
                CertificationCount = c.CertificationCount,
                IsHandmadeVerified = c.IsHandmadeVerified,
            }).ToList(),
        };

        var result = await _aiProvider.RankSuppliersAsync(context, cancellationToken);
        result.Rankings = result.Rankings.Take(request.MaxResults).ToList();
        return result;
    }

    public async Task<QualityPredictionResult> PredictQualityAsync(QualityPredictionRequest request, CancellationToken cancellationToken)
    {
        var profile = await _aiIntelligenceRepository.GetProducerIntelligenceProfileAsync(request.ProducerId, cancellationToken)
            ?? throw new NotFoundException("Producer not found.");

        var context = new QualityPredictionContext
        {
            ProducerName = profile.ProducerName,
            AverageRating = profile.AverageRating,
            ReviewCount = profile.ReviewCount,
            ProductCount = profile.ProductCount,
            HandmadeVerifiedProductCount = profile.HandmadeVerifiedProductCount,
            DeliveredOrderItemCount = profile.DeliveredOrderItemCount,
            CancelledOrderItemCount = profile.CancelledOrderItemCount,
        };

        return await _aiProvider.PredictQualityAsync(context, cancellationToken);
    }

    public async Task<PriceForecastResult> ForecastPriceAsync(PriceForecastRequest request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken)
            ?? throw new NotFoundException("Category not found.");

        var history = await _aiIntelligenceRepository.GetCategoryMonthlyAveragePriceAsync(request.CategoryId, 12, cancellationToken);

        var context = new PriceForecastContext
        {
            CategoryName = category.Name,
            HorizonMonths = request.HorizonMonths,
            HistoricalMonthlyAveragePrice = history,
        };

        return await _aiProvider.ForecastPriceAsync(context, cancellationToken);
    }

    public async Task<DeliveryPredictionResult> PredictDeliveryAsync(DeliveryPredictionRequest request, CancellationToken cancellationToken)
    {
        var profile = await _aiIntelligenceRepository.GetProducerIntelligenceProfileAsync(request.ProducerId, cancellationToken)
            ?? throw new NotFoundException("Producer not found.");

        var context = new DeliveryPredictionContext
        {
            ProducerName = profile.ProducerName,
            HistoricalDeliveryDays = profile.HistoricalDeliveryDays,
            RequestedQuantity = request.Quantity,
            EstimatedProductionCapacity = profile.EstimatedProductionCapacity,
        };

        return await _aiProvider.PredictDeliveryAsync(context, cancellationToken);
    }

    public async Task<RiskAssessmentResult> AssessRiskAsync(RiskAssessmentRequest request, CancellationToken cancellationToken)
    {
        var profile = await _aiIntelligenceRepository.GetProducerIntelligenceProfileAsync(request.ProducerId, cancellationToken)
            ?? throw new NotFoundException("Producer not found.");

        var context = new RiskAssessmentContext
        {
            ProducerName = profile.ProducerName,
            AverageRating = profile.AverageRating,
            ReviewCount = profile.ReviewCount,
            TotalOrderItemCount = profile.TotalOrderItemCount,
            CancelledOrderItemCount = profile.CancelledOrderItemCount,
            TotalQuotationResponseCount = profile.TotalQuotationResponseCount,
            RejectedQuotationResponseCount = profile.RejectedQuotationResponseCount,
            TotalProcurementCount = profile.TotalProcurementCount,
            CancelledProcurementCount = profile.CancelledProcurementCount,
            HasVerifiedCertification = profile.HasVerifiedCertification,
        };

        return await _aiProvider.AssessRiskAsync(context, cancellationToken);
    }
}
