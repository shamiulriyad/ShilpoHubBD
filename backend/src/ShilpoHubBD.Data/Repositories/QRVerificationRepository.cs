using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.QRVerification;

namespace ShilpoHubBD.Data.Repositories;

public class QRVerificationRepository : IQRVerificationRepository
{
    private readonly ShilpoHubDbContext _context;

    public QRVerificationRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    private IQueryable<QRCode> WithProduct()
        => _context.QRCodes.Include(q => q.Product).ThenInclude(p => p.Producer)
            .Include(q => q.Product).ThenInclude(p => p.District);

    private IQueryable<QRVerificationRecord> HistoryWithDetails()
        => _context.QRVerificationRecords
            .Include(r => r.QRCode).ThenInclude(q => q!.Product)
            .AsSplitQuery();

    public Task<QRCode?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => WithProduct().FirstOrDefaultAsync(q => q.Id == id, cancellationToken);

    public Task<QRCode?> GetByCodeAsync(string code, CancellationToken cancellationToken)
        => WithProduct().FirstOrDefaultAsync(q => q.Code == code, cancellationToken);

    public Task<QRCode?> GetActiveByProductIdAsync(Guid productId, CancellationToken cancellationToken)
        => WithProduct().FirstOrDefaultAsync(q => q.ProductId == productId && q.IsActive, cancellationToken);

    public async Task AddQRCodeAsync(QRCode qrCode, CancellationToken cancellationToken)
        => await _context.QRCodes.AddAsync(qrCode, cancellationToken);

    public async Task AddVerificationRecordAsync(QRVerificationRecord record, CancellationToken cancellationToken)
        => await _context.QRVerificationRecords.AddAsync(record, cancellationToken);

    public async Task<(List<QRVerificationRecord> Items, int TotalCount)> GetHistoryForUserAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken)
    {
        var records = HistoryWithDetails()
            .Where(r => r.VerifiedByUserId == userId)
            .OrderByDescending(r => r.VerifiedAt);

        var totalCount = await records.CountAsync(cancellationToken);
        var items = await records
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<(List<QRVerificationRecord> Items, int TotalCount)> GetHistoryForProductAsync(Guid productId, int page, int pageSize, CancellationToken cancellationToken)
    {
        var records = HistoryWithDetails()
            .Where(r => r.QRCode != null && r.QRCode.ProductId == productId)
            .OrderByDescending(r => r.VerifiedAt);

        var totalCount = await records.CountAsync(cancellationToken);
        var items = await records
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}
