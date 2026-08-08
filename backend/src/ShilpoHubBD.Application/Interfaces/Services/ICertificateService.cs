using ShilpoHubBD.Application.DTOs.Certificate;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface ICertificateService
{
    Task<CertificateDto> GenerateAsync(Guid producerId, GenerateCertificateRequest request, CancellationToken cancellationToken);
    Task<CertificateDto> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<CertificateDto>> GetMineAsync(Guid producerId, CancellationToken cancellationToken);
    Task RevokeAsync(Guid id, Guid producerId, bool isAdmin, CancellationToken cancellationToken);
    Task<CertificateVerificationResultDto> VerifyAsync(VerifyCertificateRequest request, CancellationToken cancellationToken);
    Task<(string FileName, string Html)> GetDownloadAsync(Guid id, CancellationToken cancellationToken);
}
