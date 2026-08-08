using ShilpoHubBD.Application.DTOs.Learning;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface ITrainingCertificateService
{
    Task<TrainingCertificateDto> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<TrainingCertificateDto>> GetMineAsync(Guid apprenticeUserId, CancellationToken cancellationToken);
    Task<TrainingCertificateVerificationResultDto> VerifyAsync(VerifyTrainingCertificateRequest request, CancellationToken cancellationToken);
    Task RevokeAsync(Guid id, Guid userId, bool isAdmin, CancellationToken cancellationToken);
    Task<(string FileName, string Html)> GetDownloadAsync(Guid id, CancellationToken cancellationToken);
}
