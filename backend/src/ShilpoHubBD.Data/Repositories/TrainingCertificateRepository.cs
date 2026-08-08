using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Learning;

namespace ShilpoHubBD.Data.Repositories;

public class TrainingCertificateRepository : ITrainingCertificateRepository
{
    private readonly ShilpoHubDbContext _context;

    public TrainingCertificateRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    public Task<TrainingCertificate?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => _context.TrainingCertificates
            .Include(c => c.Enrollment)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public Task<TrainingCertificate?> GetByEnrollmentIdAsync(Guid enrollmentId, CancellationToken cancellationToken)
        => _context.TrainingCertificates.FirstOrDefaultAsync(c => c.EnrollmentId == enrollmentId, cancellationToken);

    public Task<TrainingCertificate?> GetByCertificateNumberAsync(string certificateNumber, CancellationToken cancellationToken)
        => _context.TrainingCertificates.FirstOrDefaultAsync(c => c.CertificateNumber == certificateNumber, cancellationToken);

    public Task<List<TrainingCertificate>> GetByApprenticeAsync(Guid apprenticeId, CancellationToken cancellationToken)
        => _context.TrainingCertificates
            .Include(c => c.Enrollment)
            .Where(c => c.Enrollment.ApprenticeId == apprenticeId)
            .OrderByDescending(c => c.IssuedAt)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(TrainingCertificate certificate, CancellationToken cancellationToken)
        => await _context.TrainingCertificates.AddAsync(certificate, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}
