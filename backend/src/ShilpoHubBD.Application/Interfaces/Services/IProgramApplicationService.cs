using ShilpoHubBD.Application.DTOs.Apprenticeship;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IProgramApplicationService
{
    Task<ProgramApplicationDto> ApplyAsync(Guid applicantUserId, CreateProgramApplicationRequest request, CancellationToken cancellationToken);

    Task<ProgramApplicationDto> AcceptAsync(Guid providerUserId, Guid applicationId, RespondProgramApplicationRequest request, CancellationToken cancellationToken);

    Task<ProgramApplicationDto> RejectAsync(Guid providerUserId, Guid applicationId, RespondProgramApplicationRequest request, CancellationToken cancellationToken);

    Task<ProgramApplicationDto> WithdrawAsync(Guid applicantUserId, Guid applicationId, CancellationToken cancellationToken);

    Task<ProgramApplicationDto> GetByIdAsync(Guid userId, Guid applicationId, CancellationToken cancellationToken);

    Task<List<ProgramApplicationListItemDto>> GetMyApplicationsAsync(Guid applicantUserId, CancellationToken cancellationToken);

    Task<List<ProgramApplicationListItemDto>> GetByProgramAsync(Guid providerUserId, Guid programId, CancellationToken cancellationToken);
}
