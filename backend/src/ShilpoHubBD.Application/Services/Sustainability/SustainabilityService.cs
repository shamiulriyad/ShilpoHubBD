using Microsoft.Extensions.Options;
using ShilpoHubBD.Application.DTOs.Sustainability;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Application.Options;
using ShilpoHubBD.Domain.Entities.Sustainability;

namespace ShilpoHubBD.Application.Services.Sustainability;

public class SustainabilityService : ISustainabilityService
{
    private readonly ISustainabilityRepository _sustainabilityRepository;
    private readonly SustainabilityScoreOptions _scoreOptions;

    public SustainabilityService(ISustainabilityRepository sustainabilityRepository, IOptions<SustainabilityScoreOptions> scoreOptions)
    {
        _sustainabilityRepository = sustainabilityRepository;
        _scoreOptions = scoreOptions.Value;
    }

    public async Task<SustainabilityProfileDto> GetMyProfileAsync(Guid producerId, CancellationToken cancellationToken)
    {
        var profile = await GetOrCreateProfileAsync(producerId, cancellationToken);
        return ToDto(profile);
    }

    public async Task<SustainabilityProfileDto> GetByProducerIdAsync(Guid producerId, CancellationToken cancellationToken)
    {
        var profile = await _sustainabilityRepository.GetByProducerIdAsync(producerId, cancellationToken)
            ?? throw new NotFoundException("Sustainability profile not found for this producer.");

        return ToDto(profile);
    }

    public async Task<SustainableMaterialRecordDto> AddMaterialRecordAsync(
        Guid producerId, CreateMaterialRecordRequest request, CancellationToken cancellationToken)
    {
        var profile = await GetOrCreateProfileAsync(producerId, cancellationToken);

        var record = new SustainableMaterialRecord
        {
            Id = Guid.NewGuid(),
            SustainabilityProfileId = profile.Id,
            ProductId = request.ProductId,
            MaterialName = request.MaterialName.Trim(),
            QuantityUsed = request.QuantityUsed,
            Unit = request.Unit.Trim(),
            IsRecycled = request.IsRecycled,
            IsRenewable = request.IsRenewable,
            IsLocallySourced = request.IsLocallySourced,
            IsBiodegradable = request.IsBiodegradable,
            CarbonSavingsPerUnitKg = request.CarbonSavingsPerUnitKg,
            RecordedAt = DateTime.UtcNow,
        };

        await _sustainabilityRepository.AddMaterialRecordAsync(record, cancellationToken);
        profile.MaterialRecords.Add(record);
        Recalculate(profile);

        await _sustainabilityRepository.SaveChangesAsync(cancellationToken);
        return ToMaterialRecordDto(record);
    }

    public async Task<SustainableMaterialCertificationDto> AddCertificationAsync(
        Guid producerId, CreateMaterialCertificationRequest request, CancellationToken cancellationToken)
    {
        var profile = await GetOrCreateProfileAsync(producerId, cancellationToken);

        var certification = new SustainableMaterialCertification
        {
            Id = Guid.NewGuid(),
            SustainabilityProfileId = profile.Id,
            MaterialName = request.MaterialName.Trim(),
            CertifyingBody = request.CertifyingBody.Trim(),
            CertificateReference = request.CertificateReference.Trim(),
            IssuedAt = request.IssuedAt,
            ExpiresAt = request.ExpiresAt,
            IsVerified = false,
            CreatedAt = DateTime.UtcNow,
        };

        await _sustainabilityRepository.AddCertificationAsync(certification, cancellationToken);
        profile.Certifications.Add(certification);
        Recalculate(profile);

        await _sustainabilityRepository.SaveChangesAsync(cancellationToken);
        return ToCertificationDto(certification);
    }

    public async Task<SustainableMaterialCertificationDto> VerifyCertificationAsync(Guid certificationId, CancellationToken cancellationToken)
    {
        var certification = await _sustainabilityRepository.GetCertificationByIdAsync(certificationId, cancellationToken)
            ?? throw new NotFoundException("Material certification not found.");

        if (certification.IsVerified)
        {
            throw new ConflictException("This certification has already been verified.");
        }

        certification.IsVerified = true;

        var profile = await _sustainabilityRepository.GetByIdAsync(certification.SustainabilityProfileId, cancellationToken);
        if (profile is not null)
        {
            Recalculate(profile);
        }

        await _sustainabilityRepository.SaveChangesAsync(cancellationToken);
        return ToCertificationDto(certification);
    }

    private async Task<SustainabilityProfile> GetOrCreateProfileAsync(Guid producerId, CancellationToken cancellationToken)
    {
        var profile = await _sustainabilityRepository.GetByProducerIdAsync(producerId, cancellationToken);
        if (profile is not null)
        {
            return profile;
        }

        var now = DateTime.UtcNow;
        profile = new SustainabilityProfile
        {
            Id = Guid.NewGuid(),
            ProducerId = producerId,
            EcoScore = 0,
            BadgeLevel = GreenBadgeLevel.None,
            TotalCarbonSavingsKg = 0,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await _sustainabilityRepository.AddAsync(profile, cancellationToken);
        await _sustainabilityRepository.SaveChangesAsync(cancellationToken);
        return profile;
    }

    private void Recalculate(SustainabilityProfile profile)
    {
        var points =
            profile.MaterialRecords.Count(r => r.IsRecycled) * _scoreOptions.PointsPerRecycledMaterial +
            profile.MaterialRecords.Count(r => r.IsRenewable) * _scoreOptions.PointsPerRenewableMaterial +
            profile.MaterialRecords.Count(r => r.IsLocallySourced) * _scoreOptions.PointsPerLocallySourcedMaterial +
            profile.MaterialRecords.Count(r => r.IsBiodegradable) * _scoreOptions.PointsPerBiodegradableMaterial +
            profile.Certifications.Count(c => c.IsVerified) * _scoreOptions.PointsPerVerifiedCertification;

        var now = DateTime.UtcNow;
        profile.EcoScore = Math.Min(points, _scoreOptions.MaxEcoScore);
        profile.TotalCarbonSavingsKg = profile.MaterialRecords.Sum(r => r.QuantityUsed * r.CarbonSavingsPerUnitKg);

        profile.BadgeLevel = profile.EcoScore >= _scoreOptions.GoldBadgeThreshold ? GreenBadgeLevel.Gold
            : profile.EcoScore >= _scoreOptions.SilverBadgeThreshold ? GreenBadgeLevel.Silver
            : profile.EcoScore >= _scoreOptions.BronzeBadgeThreshold ? GreenBadgeLevel.Bronze
            : GreenBadgeLevel.None;
        profile.LastCalculatedAt = now;
        profile.UpdatedAt = now;
    }

    private static SustainabilityProfileDto ToDto(SustainabilityProfile profile) => new()
    {
        Id = profile.Id,
        ProducerId = profile.ProducerId,
        EcoScore = profile.EcoScore,
        BadgeLevel = profile.BadgeLevel.ToString(),
        TotalCarbonSavingsKg = profile.TotalCarbonSavingsKg,
        MaterialRecords = profile.MaterialRecords.OrderByDescending(r => r.RecordedAt).Select(ToMaterialRecordDto).ToList(),
        Certifications = profile.Certifications.OrderByDescending(c => c.CreatedAt).Select(ToCertificationDto).ToList(),
        LastCalculatedAt = profile.LastCalculatedAt,
        CreatedAt = profile.CreatedAt,
        UpdatedAt = profile.UpdatedAt,
    };

    private static SustainableMaterialRecordDto ToMaterialRecordDto(SustainableMaterialRecord record) => new()
    {
        Id = record.Id,
        ProductId = record.ProductId,
        MaterialName = record.MaterialName,
        QuantityUsed = record.QuantityUsed,
        Unit = record.Unit,
        IsRecycled = record.IsRecycled,
        IsRenewable = record.IsRenewable,
        IsLocallySourced = record.IsLocallySourced,
        IsBiodegradable = record.IsBiodegradable,
        CarbonSavingsPerUnitKg = record.CarbonSavingsPerUnitKg,
        TotalCarbonSavingsKg = record.QuantityUsed * record.CarbonSavingsPerUnitKg,
        RecordedAt = record.RecordedAt,
    };

    private static SustainableMaterialCertificationDto ToCertificationDto(SustainableMaterialCertification certification) => new()
    {
        Id = certification.Id,
        MaterialName = certification.MaterialName,
        CertifyingBody = certification.CertifyingBody,
        CertificateReference = certification.CertificateReference,
        IssuedAt = certification.IssuedAt,
        ExpiresAt = certification.ExpiresAt,
        IsVerified = certification.IsVerified,
        CreatedAt = certification.CreatedAt,
    };
}
