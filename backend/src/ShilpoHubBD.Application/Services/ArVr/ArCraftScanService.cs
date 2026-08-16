using ShilpoHubBD.Application.DTOs.ArVr;
using ShilpoHubBD.Application.DTOs.Marketplace;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.QRVerification;

namespace ShilpoHubBD.Application.Services.ArVr;

// Scanning a physical AR marker (printed/etched on a craft item) is the same action as scanning its
// QR code, so this reuses the existing QRVerification code/record tables instead of introducing a
// parallel marker registry, then decorates the result with craft/producer story, traceability and
// certificate data for the AR overlay -- no rendering, just the data future AR/VR clients need.
public class ArCraftScanService : IArCraftScanService
{
    private readonly IQRVerificationRepository _qrVerificationRepository;
    private readonly IProductRepository _productRepository;
    private readonly ICraftStoryRepository _craftStoryRepository;
    private readonly IProducerStoryRepository _producerStoryRepository;
    private readonly ITraceabilityRepository _traceabilityRepository;
    private readonly ICertificateRepository _certificateRepository;

    public ArCraftScanService(
        IQRVerificationRepository qrVerificationRepository,
        IProductRepository productRepository,
        ICraftStoryRepository craftStoryRepository,
        IProducerStoryRepository producerStoryRepository,
        ITraceabilityRepository traceabilityRepository,
        ICertificateRepository certificateRepository)
    {
        _qrVerificationRepository = qrVerificationRepository;
        _productRepository = productRepository;
        _craftStoryRepository = craftStoryRepository;
        _producerStoryRepository = producerStoryRepository;
        _traceabilityRepository = traceabilityRepository;
        _certificateRepository = certificateRepository;
    }

    public async Task<ArCraftScanResultDto> ScanAsync(Guid? scannedByUserId, ArCraftScanRequest request, CancellationToken cancellationToken)
    {
        var code = request.Code.Trim();
        var qrCode = await _qrVerificationRepository.GetByCodeAsync(code, cancellationToken);
        var now = DateTime.UtcNow;
        var isRecognized = qrCode is { IsActive: true };

        var record = new QRVerificationRecord
        {
            Id = Guid.NewGuid(),
            ScannedCode = code,
            QRCodeId = qrCode?.Id,
            VerifiedByUserId = scannedByUserId,
            IsValid = isRecognized,
            VerifiedAt = now,
        };

        await _qrVerificationRepository.AddVerificationRecordAsync(record, cancellationToken);
        await _qrVerificationRepository.SaveChangesAsync(cancellationToken);

        if (!isRecognized)
        {
            return new ArCraftScanResultDto { IsRecognized = false, ScannedAt = now };
        }

        var product = await _productRepository.GetByIdAsync(qrCode!.ProductId, cancellationToken);
        if (product is null)
        {
            return new ArCraftScanResultDto { IsRecognized = false, ScannedAt = now };
        }

        var craftStory = await _craftStoryRepository.GetByCategoryIdAsync(product.CategoryId, cancellationToken);
        var producerStory = await _producerStoryRepository.GetByProducerIdAsync(product.ProducerId, cancellationToken);
        var traceability = await _traceabilityRepository.GetByProductIdAsync(product.Id, cancellationToken);
        var certificate = await _certificateRepository.GetActiveByProductIdAsync(product.Id, cancellationToken);

        return new ArCraftScanResultDto
        {
            IsRecognized = true,
            ScannedAt = now,
            Product = new ArScannedProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Story = product.Story,
                PrimaryImageUrl = product.Images
                    .OrderByDescending(i => i.IsPrimary)
                    .ThenBy(i => i.DisplayOrder)
                    .FirstOrDefault()?.ImageUrl,
                MakingProcessVideoUrl = product.MakingProcessVideoUrl,
                Price = product.Price,
                CategoryName = product.Category.Name,
                ProducerId = product.ProducerId,
                ProducerName = product.Producer.FullName,
                DistrictName = product.District.Name,
            },
            CraftStory = craftStory is null ? null : new ArCraftOriginDto
            {
                Origin = craftStory.Origin,
                Since = craftStory.Since,
                Summary = craftStory.Summary,
                Chapters = craftStory.Chapters
                    .OrderBy(c => c.DisplayOrder)
                    .Select(c => new StoryChapterDto { Heading = c.Heading, Body = c.Body, DisplayOrder = c.DisplayOrder })
                    .ToList(),
            },
            ProducerStory = producerStory is null ? null : new ArProducerHeritageDto
            {
                HeritageId = producerStory.HeritageId,
                Generations = producerStory.Generations,
                FoundingYear = producerStory.FoundingYear,
                Quote = producerStory.Quote,
                Chapters = producerStory.Chapters
                    .OrderBy(c => c.DisplayOrder)
                    .Select(c => new StoryChapterDto { Heading = c.Heading, Body = c.Body, DisplayOrder = c.DisplayOrder })
                    .ToList(),
            },
            TraceabilitySummary = traceability?.Summary,
            IsCertified = certificate is not null,
            CertificateNumber = certificate?.CertificateNumber,
        };
    }
}
