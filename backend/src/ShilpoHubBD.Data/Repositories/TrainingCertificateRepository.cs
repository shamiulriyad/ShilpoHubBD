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

    private IQueryable<TrainingCertificate> WithDetails()
        => _context.TrainingCertificates
            .Include(c => c.Enrollment)
            .Include(c => c.ApprenticeEnrollment)
            .Include(c => c.HeritageSkill)
            .AsSplitQuery();

    public Task<TrainingCertificate?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => WithDetails().FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public Task<TrainingCertificate?> GetByEnrollmentIdAsync(Guid enrollmentId, CancellationToken cancellationToken)
        => _context.TrainingCertificates.FirstOrDefaultAsync(c => c.EnrollmentId == enrollmentId, cancellationToken);

    public Task<TrainingCertificate?> GetByApprenticeEnrollmentIdAsync(Guid apprenticeEnrollmentId, CancellationToken cancellationToken)
        => _context.TrainingCertificates.FirstOrDefaultAsync(c => c.ApprenticeEnrollmentId == apprenticeEnrollmentId, cancellationToken);

    public Task<TrainingCertificate?> GetActiveSkillCertificateAsync(Guid recipientUserId, Guid heritageSkillId, CancellationToken cancellationToken)
        => _context.TrainingCertificates.FirstOrDefaultAsync(
            c => c.Type == CertificateType.Skill
                && c.RecipientUserId == recipientUserId
                && c.HeritageSkillId == heritageSkillId
                && !c.IsRevoked,
            cancellationToken);

    public Task<TrainingCertificate?> GetByCertificateNumberAsync(string certificateNumber, CancellationToken cancellationToken)
        => WithDetails().FirstOrDefaultAsync(c => c.CertificateNumber == certificateNumber, cancellationToken);

    public Task<List<TrainingCertificate>> GetByRecipientAsync(Guid recipientUserId, CancellationToken cancellationToken)
        => WithDetails()
            .Where(c => c.RecipientUserId == recipientUserId)
            .OrderByDescending(c => c.IssuedAt)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(TrainingCertificate certificate, CancellationToken cancellationToken)
        => await _context.TrainingCertificates.AddAsync(certificate, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}
