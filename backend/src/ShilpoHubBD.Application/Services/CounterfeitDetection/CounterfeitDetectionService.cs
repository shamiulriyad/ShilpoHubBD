using ShilpoHubBD.Application.DTOs.CounterfeitDetection;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;

namespace ShilpoHubBD.Application.Services.CounterfeitDetection;

public class CounterfeitDetectionService : ICounterfeitDetectionService
{
    private readonly IProductRepository _productRepository;
    private readonly ICounterfeitDetectionProvider _provider;

    public CounterfeitDetectionService(IProductRepository productRepository, ICounterfeitDetectionProvider provider)
    {
        _productRepository = productRepository;
        _provider = provider;
    }

    public async Task<CounterfeitCheckResultDto> CheckAsync(Guid productId, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(productId, cancellationToken)
            ?? throw new NotFoundException("Product not found.");

        var (averagePrice, sampleSize) = await _productRepository.GetCategoryPriceStatsAsync(product.CategoryId, cancellationToken);

        var context = new CounterfeitCheckContext
        {
            ProductName = product.Name,
            Price = product.Price,
            CategoryAveragePrice = averagePrice,
            CategorySampleSize = sampleSize,
            HandmadeVerificationStatus = product.HandmadeVerificationStatus,
            ApprovalStatus = product.ApprovalStatus,
            ReviewCount = product.ReviewCount,
            SalesCount = product.SalesCount,
        };

        var (riskScore, riskLevel, signals) = _provider.Check(context);

        return new CounterfeitCheckResultDto
        {
            ProductId = product.Id,
            ProductName = product.Name,
            RiskScore = riskScore,
            RiskLevel = riskLevel,
            Signals = signals,
        };
    }
}
