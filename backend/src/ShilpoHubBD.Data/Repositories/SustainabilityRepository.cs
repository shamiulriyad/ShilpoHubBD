using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Sustainability;

namespace ShilpoHubBD.Data.Repositories;

public class SustainabilityRepository : ISustainabilityRepository
{
    private readonly ShilpoHubDbContext _context;

    public SustainabilityRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    private IQueryable<SustainabilityProfile> WithDetails()
        => _context.SustainabilityProfiles
            .Include(p => p.MaterialRecords)
            .Include(p => p.Certifications)
            .AsSplitQuery();

    public Task<SustainabilityProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => WithDetails().FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<SustainabilityProfile?> GetByProducerIdAsync(Guid producerId, CancellationToken cancellationToken)
        => WithDetails().FirstOrDefaultAsync(p => p.ProducerId == producerId, cancellationToken);

    public Task<SustainableMaterialCertification?> GetCertificationByIdAsync(Guid certificationId, CancellationToken cancellationToken)
        => _context.SustainableMaterialCertifications
            .Include(c => c.SustainabilityProfile)
            .FirstOrDefaultAsync(c => c.Id == certificationId, cancellationToken);

    public async Task AddAsync(SustainabilityProfile profile, CancellationToken cancellationToken)
        => await _context.SustainabilityProfiles.AddAsync(profile, cancellationToken);

    public async Task AddMaterialRecordAsync(SustainableMaterialRecord record, CancellationToken cancellationToken)
        => await _context.SustainableMaterialRecords.AddAsync(record, cancellationToken);

    public async Task AddCertificationAsync(SustainableMaterialCertification certification, CancellationToken cancellationToken)
        => await _context.SustainableMaterialCertifications.AddAsync(certification, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}
