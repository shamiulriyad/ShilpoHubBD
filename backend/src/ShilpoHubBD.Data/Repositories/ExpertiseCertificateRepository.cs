using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Certificate;
using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Data.Repositories;

public class ExpertiseCertificateRepository : IExpertiseCertificateRepository
{
    private readonly ShilpoHubDbContext _context;

    public ExpertiseCertificateRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProducerRatingStat>> GetRatingStatsAsync(Guid? producerId, CancellationToken cancellationToken)
    {
        var reviews = _context.Reviews.Where(r => r.ProductId != null);
        if (producerId.HasValue)
        {
            reviews = reviews.Where(r => r.Product!.ProducerId == producerId.Value);
        }

        return await reviews
            .GroupBy(r => new { r.Product!.ProducerId, r.Product.Producer.FullName })
            .Select(g => new ProducerRatingStat(g.Key.ProducerId, g.Key.FullName, g.Count(), g.Average(r => (double)(r.ProducerRating ?? r.Rating))))
            .ToListAsync(cancellationToken);
    }

    public Task<List<ExpertiseCertificate>> GetByProducerAsync(Guid producerId, CancellationToken cancellationToken)
        => _context.ExpertiseCertificates.Include(c => c.Producer)
            .Where(c => c.ProducerId == producerId).OrderByDescending(c => c.IssuedAt).ToListAsync(cancellationToken);

    public Task<List<ExpertiseCertificate>> GetAllActiveAsync(CancellationToken cancellationToken)
        => _context.ExpertiseCertificates.Include(c => c.Producer).Where(c => !c.IsRevoked).ToListAsync(cancellationToken);

    public async Task<(string? Expertise, bool Approved)> GetProfileAsync(Guid producerId, CancellationToken cancellationToken)
    {
        var profile = await _context.UserProfiles.Where(p => p.UserId == producerId)
            .Select(p => new { p.Expertise, p.Status }).FirstOrDefaultAsync(cancellationToken);
        return (profile?.Expertise, profile?.Status == UserProfileStatus.Approved);
    }

    public Task<bool> NumberExistsAsync(string number, CancellationToken cancellationToken)
        => _context.ExpertiseCertificates.AnyAsync(c => c.CertificateNumber == number, cancellationToken);

    public async Task AddAsync(ExpertiseCertificate certificate, CancellationToken cancellationToken)
        => await _context.ExpertiseCertificates.AddAsync(certificate, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}
