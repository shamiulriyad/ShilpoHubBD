using ShilpoHubBD.Application.DTOs.Employment;
using ShilpoHubBD.Application.DTOs.Portfolio;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IJobApplicationService
{
    Task<JobApplicationDto> ApplyAsync(Guid applicantUserId, CreateJobApplicationRequest request, CancellationToken cancellationToken);

    Task<JobApplicationDto> ShortlistAsync(Guid employerUserId, Guid applicationId, RespondJobApplicationRequest request, CancellationToken cancellationToken);

    Task<JobApplicationDto> RejectAsync(Guid employerUserId, Guid applicationId, RespondJobApplicationRequest request, CancellationToken cancellationToken);

    Task<JobApplicationDto> HireAsync(Guid employerUserId, Guid applicationId, RespondJobApplicationRequest request, CancellationToken cancellationToken);

    Task<JobApplicationDto> WithdrawAsync(Guid applicantUserId, Guid applicationId, CancellationToken cancellationToken);

    Task<JobApplicationDto> GetByIdAsync(Guid userId, Guid applicationId, CancellationToken cancellationToken);

    Task<List<JobApplicationListItemDto>> GetMyApplicationsAsync(Guid applicantUserId, CancellationToken cancellationToken);

    Task<List<JobApplicationListItemDto>> GetByJobListingAsync(Guid employerUserId, Guid jobListingId, CancellationToken cancellationToken);

    Task<PortfolioDto> GetCandidatePortfolioAsync(Guid employerUserId, Guid applicationId, CancellationToken cancellationToken);
}
