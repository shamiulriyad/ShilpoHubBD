using System.Net;
using ShilpoHubBD.Application.DTOs.Certificate;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;

namespace ShilpoHubBD.Application.Services.Certificate;

public class CertificateService : ICertificateService
{
    private readonly ICertificateRepository _certificateRepository;
    private readonly IProductRepository _productRepository;

    public CertificateService(ICertificateRepository certificateRepository, IProductRepository productRepository)
    {
        _certificateRepository = certificateRepository;
        _productRepository = productRepository;
    }

    public async Task<CertificateDto> GenerateAsync(Guid producerId, GenerateCertificateRequest request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken)
            ?? throw new NotFoundException("Product not found.");

        if (product.ProducerId != producerId)
        {
            throw new UnauthorizedAccessException("You can only generate certificates for your own products.");
        }

        var existing = await _certificateRepository.GetActiveByProductIdAsync(request.ProductId, cancellationToken);
        if (existing is not null)
        {
            return ToDto(existing);
        }

        var now = DateTime.UtcNow;
        var certificate = new Domain.Entities.Certificate.Certificate
        {
            Id = Guid.NewGuid(),
            ProductId = product.Id,
            ProducerId = producerId,
            CertificateNumber = GenerateCertificateNumber(now),
            ProductName = product.Name,
            ProducerName = product.Producer.FullName,
            District = product.District.Name,
            Category = product.Category.Name,
            IsRevoked = false,
            IssuedAt = now,
        };

        await _certificateRepository.AddAsync(certificate, cancellationToken);
        await _certificateRepository.SaveChangesAsync(cancellationToken);

        return ToDto(certificate);
    }

    public async Task<CertificateDto> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var certificate = await _certificateRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Certificate not found.");

        return ToDto(certificate);
    }

    public async Task<List<CertificateDto>> GetMineAsync(Guid producerId, CancellationToken cancellationToken)
    {
        var certificates = await _certificateRepository.GetByProducerAsync(producerId, cancellationToken);
        return certificates.Select(ToDto).ToList();
    }

    public async Task RevokeAsync(Guid id, Guid producerId, bool isAdmin, CancellationToken cancellationToken)
    {
        var certificate = await _certificateRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Certificate not found.");

        if (!isAdmin && certificate.ProducerId != producerId)
        {
            throw new UnauthorizedAccessException("You do not have permission to manage this certificate.");
        }

        if (certificate.IsRevoked)
        {
            throw new ConflictException("This certificate has already been revoked.");
        }

        certificate.IsRevoked = true;
        certificate.RevokedAt = DateTime.UtcNow;
        await _certificateRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task<CertificateVerificationResultDto> VerifyAsync(VerifyCertificateRequest request, CancellationToken cancellationToken)
    {
        var certificateNumber = request.CertificateNumber.Trim();
        var certificate = await _certificateRepository.GetByCertificateNumberAsync(certificateNumber, cancellationToken);

        if (certificate is null)
        {
            return new CertificateVerificationResultDto
            {
                IsValid = false,
                CertificateNumber = certificateNumber,
                Message = "No certificate was found with this number.",
            };
        }

        if (certificate.IsRevoked)
        {
            return new CertificateVerificationResultDto
            {
                IsValid = false,
                CertificateNumber = certificate.CertificateNumber,
                ProductName = certificate.ProductName,
                ProducerName = certificate.ProducerName,
                District = certificate.District,
                Category = certificate.Category,
                IssuedAt = certificate.IssuedAt,
                Message = "This certificate has been revoked and is no longer valid.",
            };
        }

        return new CertificateVerificationResultDto
        {
            IsValid = true,
            CertificateNumber = certificate.CertificateNumber,
            ProductName = certificate.ProductName,
            ProducerName = certificate.ProducerName,
            District = certificate.District,
            Category = certificate.Category,
            IssuedAt = certificate.IssuedAt,
            Message = "This certificate is authentic.",
        };
    }

    public async Task<(string FileName, string Html)> GetDownloadAsync(Guid id, CancellationToken cancellationToken)
    {
        var certificate = await _certificateRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Certificate not found.");

        var fileName = $"Certificate-{certificate.CertificateNumber}.html";
        return (fileName, RenderHtml(certificate));
    }

    private static string GenerateCertificateNumber(DateTime issuedAt)
        => $"SH-{issuedAt:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";

    private static string RenderHtml(Domain.Entities.Certificate.Certificate certificate)
    {
        var status = certificate.IsRevoked ? "REVOKED" : "AUTHENTIC";
        var statusColor = certificate.IsRevoked ? "#b91c1c" : "#15803d";

        return $$"""
            <!doctype html>
            <html lang="en">
            <head>
            <meta charset="utf-8" />
            <title>Certificate of Authenticity - {{WebUtility.HtmlEncode(certificate.CertificateNumber)}}</title>
            <style>
                body { font-family: Georgia, 'Times New Roman', serif; background: #f5f1e8; margin: 0; padding: 40px; }
                .certificate { max-width: 720px; margin: 0 auto; background: #fffdf7; border: 3px double #8a6d3b; padding: 48px; text-align: center; }
                h1 { font-size: 28px; letter-spacing: 2px; color: #4a3b22; margin-bottom: 4px; }
                .subtitle { color: #7a6a4a; margin-bottom: 32px; }
                .field { margin: 12px 0; font-size: 16px; }
                .field .label { color: #7a6a4a; font-size: 12px; text-transform: uppercase; letter-spacing: 1px; display: block; }
                .field .value { color: #2a2116; font-size: 18px; }
                .status { display: inline-block; margin-top: 24px; padding: 8px 20px; border-radius: 4px; font-weight: bold; letter-spacing: 1px; color: #fff; background: {{statusColor}}; }
                .number { margin-top: 24px; font-size: 14px; color: #7a6a4a; }
            </style>
            </head>
            <body>
                <div class="certificate">
                    <h1>Certificate of Authenticity</h1>
                    <div class="subtitle">ShilpoHubBD - Bangladeshi Heritage Marketplace</div>

                    <div class="field">
                        <span class="label">Product</span>
                        <span class="value">{{WebUtility.HtmlEncode(certificate.ProductName)}}</span>
                    </div>
                    <div class="field">
                        <span class="label">Producer</span>
                        <span class="value">{{WebUtility.HtmlEncode(certificate.ProducerName)}}</span>
                    </div>
                    <div class="field">
                        <span class="label">District</span>
                        <span class="value">{{WebUtility.HtmlEncode(certificate.District)}}</span>
                    </div>
                    <div class="field">
                        <span class="label">Category</span>
                        <span class="value">{{WebUtility.HtmlEncode(certificate.Category)}}</span>
                    </div>
                    <div class="field">
                        <span class="label">Issued</span>
                        <span class="value">{{certificate.IssuedAt:MMMM d, yyyy}}</span>
                    </div>

                    <div class="status">{{status}}</div>
                    <div class="number">Certificate No. {{WebUtility.HtmlEncode(certificate.CertificateNumber)}}</div>
                </div>
            </body>
            </html>
            """;
    }

    private static CertificateDto ToDto(Domain.Entities.Certificate.Certificate certificate) => new()
    {
        Id = certificate.Id,
        ProductId = certificate.ProductId,
        ProductName = certificate.ProductName,
        ProducerId = certificate.ProducerId,
        ProducerName = certificate.ProducerName,
        District = certificate.District,
        Category = certificate.Category,
        CertificateNumber = certificate.CertificateNumber,
        IsRevoked = certificate.IsRevoked,
        RevokedAt = certificate.RevokedAt,
        IssuedAt = certificate.IssuedAt,
    };
}
