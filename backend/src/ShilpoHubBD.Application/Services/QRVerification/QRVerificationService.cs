using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.QRVerification;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.QRVerification;

namespace ShilpoHubBD.Application.Services.QRVerification;

public class QRVerificationService : IQRVerificationService
{
    private readonly IQRVerificationRepository _qrVerificationRepository;
    private readonly IProductRepository _productRepository;

    public QRVerificationService(IQRVerificationRepository qrVerificationRepository, IProductRepository productRepository)
    {
        _qrVerificationRepository = qrVerificationRepository;
        _productRepository = productRepository;
    }

    public async Task<QRCodeDto> GenerateAsync(Guid producerId, GenerateQRCodeRequest request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken)
            ?? throw new NotFoundException("Product not found.");

        if (product.ProducerId != producerId)
        {
            throw new UnauthorizedAccessException("You can only generate QR codes for your own products.");
        }

        var existing = await _qrVerificationRepository.GetActiveByProductIdAsync(request.ProductId, cancellationToken);
        if (existing is not null)
        {
            return ToDto(existing);
        }

        var qrCode = new QRCode
        {
            Id = Guid.NewGuid(),
            ProductId = request.ProductId,
            Code = Guid.NewGuid().ToString("N"),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
        };

        await _qrVerificationRepository.AddQRCodeAsync(qrCode, cancellationToken);
        await _qrVerificationRepository.SaveChangesAsync(cancellationToken);

        qrCode.Product = product;
        return ToDto(qrCode);
    }

    public async Task RevokeAsync(Guid id, Guid producerId, bool isAdmin, CancellationToken cancellationToken)
    {
        var qrCode = await _qrVerificationRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("QR code not found.");

        if (!isAdmin && qrCode.Product.ProducerId != producerId)
        {
            throw new UnauthorizedAccessException("You do not have permission to manage this QR code.");
        }

        if (!qrCode.IsActive)
        {
            throw new ConflictException("This QR code has already been revoked.");
        }

        qrCode.IsActive = false;
        await _qrVerificationRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task<QRVerificationResultDto> VerifyAsync(Guid? userId, VerifyQRRequest request, CancellationToken cancellationToken)
    {
        var code = request.Code.Trim();
        var qrCode = await _qrVerificationRepository.GetByCodeAsync(code, cancellationToken);
        var now = DateTime.UtcNow;
        var isValid = qrCode is { IsActive: true };

        var record = new QRVerificationRecord
        {
            Id = Guid.NewGuid(),
            ScannedCode = code,
            QRCodeId = qrCode?.Id,
            VerifiedByUserId = userId,
            IsValid = isValid,
            VerifiedAt = now,
        };

        await _qrVerificationRepository.AddVerificationRecordAsync(record, cancellationToken);
        await _qrVerificationRepository.SaveChangesAsync(cancellationToken);

        if (qrCode is null)
        {
            return new QRVerificationResultDto
            {
                IsValid = false,
                Message = "No product was found for this QR code.",
                VerifiedAt = now,
            };
        }

        if (!qrCode.IsActive)
        {
            return new QRVerificationResultDto
            {
                IsValid = false,
                ProductId = qrCode.ProductId,
                ProductName = qrCode.Product.Name,
                Message = "This QR code has been revoked and is no longer valid.",
                VerifiedAt = now,
            };
        }

        return new QRVerificationResultDto
        {
            IsValid = true,
            ProductId = qrCode.ProductId,
            ProductName = qrCode.Product.Name,
            ProducerName = qrCode.Product.Producer.FullName,
            District = qrCode.Product.District.Name,
            Message = "This product is verified authentic.",
            VerifiedAt = now,
        };
    }

    public async Task<PagedResult<QRVerificationHistoryItemDto>> GetMyHistoryAsync(Guid userId, QRVerificationQueryParameters query, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _qrVerificationRepository.GetHistoryForUserAsync(userId, query.Page, query.PageSize, cancellationToken);
        return new PagedResult<QRVerificationHistoryItemDto>
        {
            Items = items.Select(ToHistoryItemDto).ToList(),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }

    public async Task<PagedResult<QRVerificationHistoryItemDto>> GetProductHistoryAsync(Guid productId, Guid producerId, bool isAdmin, QRVerificationQueryParameters query, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(productId, cancellationToken)
            ?? throw new NotFoundException("Product not found.");

        if (!isAdmin && product.ProducerId != producerId)
        {
            throw new UnauthorizedAccessException("You do not have permission to view this product's verification history.");
        }

        var (items, totalCount) = await _qrVerificationRepository.GetHistoryForProductAsync(productId, query.Page, query.PageSize, cancellationToken);
        return new PagedResult<QRVerificationHistoryItemDto>
        {
            Items = items.Select(ToHistoryItemDto).ToList(),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }

    private static QRCodeDto ToDto(QRCode qrCode) => new()
    {
        Id = qrCode.Id,
        ProductId = qrCode.ProductId,
        ProductName = qrCode.Product.Name,
        Code = qrCode.Code,
        IsActive = qrCode.IsActive,
        CreatedAt = qrCode.CreatedAt,
    };

    private static QRVerificationHistoryItemDto ToHistoryItemDto(QRVerificationRecord record) => new()
    {
        Id = record.Id,
        ScannedCode = record.ScannedCode,
        IsValid = record.IsValid,
        ProductId = record.QRCode?.ProductId,
        ProductName = record.QRCode?.Product.Name,
        VerifiedAt = record.VerifiedAt,
    };
}
