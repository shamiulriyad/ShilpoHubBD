using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.QRVerification;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IQRVerificationService
{
    Task<QRCodeDto> GenerateAsync(Guid producerId, GenerateQRCodeRequest request, CancellationToken cancellationToken);
    Task RevokeAsync(Guid id, Guid producerId, bool isAdmin, CancellationToken cancellationToken);
    Task<QRVerificationResultDto> VerifyAsync(Guid? userId, VerifyQRRequest request, CancellationToken cancellationToken);
    Task<PagedResult<QRVerificationHistoryItemDto>> GetMyHistoryAsync(Guid userId, QRVerificationQueryParameters query, CancellationToken cancellationToken);
    Task<PagedResult<QRVerificationHistoryItemDto>> GetProductHistoryAsync(Guid productId, Guid producerId, bool isAdmin, QRVerificationQueryParameters query, CancellationToken cancellationToken);
}
