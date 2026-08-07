using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Data.Repositories;

public class WorkshopGalleryRepository : IWorkshopGalleryRepository
{
    private readonly ShilpoHubDbContext _context;

    public WorkshopGalleryRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    public Task<List<WorkshopGalleryItem>> GetByProducerIdAsync(Guid producerId, CancellationToken cancellationToken)
        => _context.WorkshopGalleryItems
            .Where(i => i.ProducerId == producerId)
            .OrderBy(i => i.DisplayOrder)
            .ToListAsync(cancellationToken);

    public Task<WorkshopGalleryItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => _context.WorkshopGalleryItems.FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

    public async Task AddAsync(WorkshopGalleryItem item, CancellationToken cancellationToken)
        => await _context.WorkshopGalleryItems.AddAsync(item, cancellationToken);

    public void Remove(WorkshopGalleryItem item)
        => _context.WorkshopGalleryItems.Remove(item);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}
