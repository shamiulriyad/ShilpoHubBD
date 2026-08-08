using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Certificate;

namespace ShilpoHubBD.Data.Repositories;

public class CertificateRepository : ICertificateRepository
{
    private readonly ShilpoHubDbContext _context;

    public CertificateRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    public Task<Certificate?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => _context.Certificates.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public Task<Certificate?> GetByCertificateNumberAsync(string certificateNumber, CancellationToken cancellationToken)
        => _context.Certificates.FirstOrDefaultAsync(c => c.CertificateNumber == certificateNumber, cancellationToken);

    public Task<Certificate?> GetActiveByProductIdAsync(Guid productId, CancellationToken cancellationToken)
        => _context.Certificates.FirstOrDefaultAsync(c => c.ProductId == productId && !c.IsRevoked, cancellationToken);

    public Task<List<Certificate>> GetByProducerAsync(Guid producerId, CancellationToken cancellationToken)
        => _context.Certificates
            .Where(c => c.ProducerId == producerId)
            .OrderByDescending(c => c.IssuedAt)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Certificate certificate, CancellationToken cancellationToken)
        => await _context.Certificates.AddAsync(certificate, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}
