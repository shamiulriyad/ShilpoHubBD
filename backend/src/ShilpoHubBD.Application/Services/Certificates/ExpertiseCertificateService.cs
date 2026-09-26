using ShilpoHubBD.Application.DTOs.Certificates;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Certificate;

namespace ShilpoHubBD.Application.Services.Certificates;

// Producers earn an expertise level from what customers rate them; an admin then issues the certificate.
public class ExpertiseCertificateService : IExpertiseCertificateService
{
    // Highest first. A producer earns the first level whose rating count and average they both reach.
    private static readonly (ExpertiseLevel Level, int MinRatings, decimal MinAverage)[] Rules =
    {
        (ExpertiseLevel.Gold, 25, 4.6m),
        (ExpertiseLevel.Silver, 10, 4.3m),
        (ExpertiseLevel.Bronze, 5, 4.0m),
    };

    private readonly IExpertiseCertificateRepository _repository;

    public ExpertiseCertificateService(IExpertiseCertificateRepository repository)
    {
        _repository = repository;
    }

    public async Task<ExpertiseProgressDto> GetMineAsync(Guid producerId, CancellationToken cancellationToken)
    {
        var stat = (await _repository.GetRatingStatsAsync(producerId, cancellationToken)).FirstOrDefault();
        var certificates = await _repository.GetByProducerAsync(producerId, cancellationToken);
        var (_, approved) = await _repository.GetProfileAsync(producerId, cancellationToken);

        var count = stat?.Count ?? 0;
        var average = stat is null ? 0m : Math.Round((decimal)stat.Average, 2);
        var earned = Earned(count, average);
        var highest = certificates.Where(c => !c.IsRevoked).Select(c => (ExpertiseLevel?)c.Level).Max();

        return new ExpertiseProgressDto
        {
            AverageRating = average,
            RatingCount = count,
            EarnedLevel = earned?.ToString(),
            HighestIssuedLevel = highest?.ToString(),
            NextLevel = Next(earned)?.ToString(),
            AwaitingAdmin = earned.HasValue && (highest is null || earned > highest),
            ProfileApproved = approved,
            Rules = Rules.Reverse().Select(r => new ExpertiseLevelRuleDto { Level = r.Level.ToString(), MinRatings = r.MinRatings, MinAverage = r.MinAverage }).ToList(),
            Certificates = certificates.Select(ToDto).ToList(),
        };
    }

    public async Task<List<EligibleProducerDto>> GetEligibleAsync(CancellationToken cancellationToken)
    {
        var stats = await _repository.GetRatingStatsAsync(null, cancellationToken);
        var issued = (await _repository.GetAllActiveAsync(cancellationToken))
            .GroupBy(c => c.ProducerId).ToDictionary(g => g.Key, g => g.Max(c => c.Level));

        var result = new List<EligibleProducerDto>();
        foreach (var stat in stats)
        {
            var average = Math.Round((decimal)stat.Average, 2);
            var earned = Earned(stat.Count, average);
            if (earned is null)
            {
                continue;
            }

            if (issued.TryGetValue(stat.ProducerId, out var highest) && earned <= highest)
            {
                continue;
            }

            var (expertise, approved) = await _repository.GetProfileAsync(stat.ProducerId, cancellationToken);
            result.Add(new EligibleProducerDto
            {
                ProducerId = stat.ProducerId,
                ProducerName = stat.ProducerName,
                Expertise = expertise,
                AverageRating = average,
                RatingCount = stat.Count,
                EarnedLevel = earned.Value.ToString(),
                HighestIssuedLevel = issued.TryGetValue(stat.ProducerId, out var h) ? h.ToString() : null,
                ProfileApproved = approved,
            });
        }

        return result.OrderByDescending(r => r.AverageRating).ToList();
    }

    public async Task<ExpertiseCertificateDto> IssueAsync(Guid producerId, Guid adminUserId, CancellationToken cancellationToken)
    {
        var stat = (await _repository.GetRatingStatsAsync(producerId, cancellationToken)).FirstOrDefault()
            ?? throw new ConflictException("This producer has no customer ratings yet.");
        var average = Math.Round((decimal)stat.Average, 2);
        var earned = Earned(stat.Count, average)
            ?? throw new ConflictException("This producer has not reached the first expertise level yet.");

        var existing = await _repository.GetByProducerAsync(producerId, cancellationToken);
        var highest = existing.Where(c => !c.IsRevoked).Select(c => (ExpertiseLevel?)c.Level).Max();
        if (highest.HasValue && earned <= highest)
        {
            throw new ConflictException($"A {highest} certificate is already issued. The producer has not earned a higher level.");
        }

        var (expertise, approved) = await _repository.GetProfileAsync(producerId, cancellationToken);
        if (!approved || string.IsNullOrWhiteSpace(expertise))
        {
            throw new ConflictException("The producer needs an approved profile with their expertise before a certificate can be issued.");
        }

        var now = DateTime.UtcNow;
        var certificate = new ExpertiseCertificate
        {
            Id = Guid.NewGuid(),
            ProducerId = producerId,
            CertificateNumber = await UniqueNumberAsync(now, cancellationToken),
            Level = earned,
            Expertise = expertise!,
            AverageRating = average,
            RatingCount = stat.Count,
            IssuedByUserId = adminUserId,
            IssuedAt = now,
        };

        await _repository.AddAsync(certificate, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        certificate.Producer = (await _repository.GetByProducerAsync(producerId, cancellationToken)).First(c => c.Id == certificate.Id).Producer;
        return ToDto(certificate);
    }

    private static ExpertiseLevel? Earned(int count, decimal average)
    {
        foreach (var rule in Rules)
        {
            if (count >= rule.MinRatings && average >= rule.MinAverage)
            {
                return rule.Level;
            }
        }

        return null;
    }

    private static ExpertiseLevel? Next(ExpertiseLevel? earned)
        => earned switch
        {
            null => ExpertiseLevel.Bronze,
            ExpertiseLevel.Bronze => ExpertiseLevel.Silver,
            ExpertiseLevel.Silver => ExpertiseLevel.Gold,
            _ => null,
        };

    private async Task<string> UniqueNumberAsync(DateTime now, CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 5; attempt++)
        {
            var candidate = $"EXP-{now:yyyy}-{Random.Shared.Next(0, 1000000):D6}";
            if (!await _repository.NumberExistsAsync(candidate, cancellationToken))
            {
                return candidate;
            }
        }

        throw new ConflictException("Could not generate a certificate number. Try again.");
    }

    private static ExpertiseCertificateDto ToDto(ExpertiseCertificate c) => new()
    {
        Id = c.Id,
        ProducerId = c.ProducerId,
        ProducerName = c.Producer?.FullName ?? string.Empty,
        CertificateNumber = c.CertificateNumber,
        Level = c.Level.ToString(),
        Expertise = c.Expertise,
        AverageRating = c.AverageRating,
        RatingCount = c.RatingCount,
        IssuedAt = c.IssuedAt,
        IsRevoked = c.IsRevoked,
    };
}
