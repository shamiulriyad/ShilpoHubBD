using ShilpoHubBD.Domain.Entities.QRVerification;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IQRVerificationRepository
{
    Task<QRCode?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<QRCode?> GetByCodeAsync(string code, CancellationToken cancellationToken);
    Task<QRCode?> GetActiveByProductIdAsync(Guid productId, CancellationToken cancellationToken);
    Task AddQRCodeAsync(QRCode qrCode, CancellationToken cancellationToken);
    Task AddVerificationRecordAsync(QRVerificationRecord record, CancellationToken cancellationToken);
    Task<(List<QRVerificationRecord> Items, int TotalCount)> GetHistoryForUserAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken);
    Task<(List<QRVerificationRecord> Items, int TotalCount)> GetHistoryForProductAsync(Guid productId, int page, int pageSize, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
