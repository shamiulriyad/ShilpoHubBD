using ShilpoHubBD.Application.DTOs.Employment;
using ShilpoHubBD.Application.DTOs.Portfolio;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Employment;

namespace ShilpoHubBD.Application.Services.Employment;

public class JobApplicationService : IJobApplicationService
{
    private readonly IJobApplicationRepository _applicationRepository;
    private readonly IJobListingRepository _jobListingRepository;
    private readonly IAcademyMemberProfileRepository _academyMemberProfileRepository;
    private readonly IPortfolioService _portfolioService;

    public JobApplicationService(
        IJobApplicationRepository applicationRepository,
        IJobListingRepository jobListingRepository,
        IAcademyMemberProfileRepository academyMemberProfileRepository,
        IPortfolioService portfolioService)
    {
        _applicationRepository = applicationRepository;
        _jobListingRepository = jobListingRepository;
        _academyMemberProfileRepository = academyMemberProfileRepository;
        _portfolioService = portfolioService;
    }

    public async Task<JobApplicationDto> ApplyAsync(Guid applicantUserId, CreateJobApplicationRequest request, CancellationToken cancellationToken)
    {
        var jobListing = await _jobListingRepository.GetByIdAsync(request.JobListingId, cancellationToken)
            ?? throw new NotFoundException("Job listing not found.");

        if (jobListing.Status != JobListingStatus.Published)
        {
            throw new ConflictException("You can only apply to published job listings.");
        }

        if (jobListing.BusinessPartnerProfile.UserId == applicantUserId)
        {
            throw new ConflictException("You cannot apply to your own job listing.");
        }

        if (await _applicationRepository.HasOpenApplicationAsync(jobListing.Id, applicantUserId, cancellationToken))
        {
            throw new ConflictException("You already have a pending or shortlisted application for this job listing.");
        }

        var application = new JobApplication
        {
            Id = Guid.NewGuid(),
            JobListingId = jobListing.Id,
            ApplicantUserId = applicantUserId,
            CoverMessage = request.CoverMessage.Trim(),
            Status = JobApplicationStatus.Pending,
            AppliedAt = DateTime.UtcNow,
        };

        await _applicationRepository.AddAsync(application, cancellationToken);
        await _applicationRepository.SaveChangesAsync(cancellationToken);

        var created = await _applicationRepository.GetByIdAsync(application.Id, cancellationToken);
        return ToDto(created!);
    }

    public async Task<JobApplicationDto> ShortlistAsync(
        Guid employerUserId, Guid applicationId, RespondJobApplicationRequest request, CancellationToken cancellationToken)
    {
        var application = await GetOwnedByEmployerAsync(employerUserId, applicationId, cancellationToken);

        if (application.Status != JobApplicationStatus.Pending)
        {
            throw new ConflictException("Only a pending application can be shortlisted.");
        }

        application.Status = JobApplicationStatus.Shortlisted;
        application.ResponseMessage = request.ResponseMessage?.Trim();
        application.RespondedAt = DateTime.UtcNow;

        await _applicationRepository.SaveChangesAsync(cancellationToken);
        return ToDto(application);
    }

    public async Task<JobApplicationDto> RejectAsync(
        Guid employerUserId, Guid applicationId, RespondJobApplicationRequest request, CancellationToken cancellationToken)
    {
        var application = await GetOwnedByEmployerAsync(employerUserId, applicationId, cancellationToken);

        if (application.Status != JobApplicationStatus.Pending && application.Status != JobApplicationStatus.Shortlisted)
        {
            throw new ConflictException("Only a pending or shortlisted application can be rejected.");
        }

        application.Status = JobApplicationStatus.Rejected;
        application.ResponseMessage = request.ResponseMessage?.Trim();
        application.RespondedAt = DateTime.UtcNow;

        await _applicationRepository.SaveChangesAsync(cancellationToken);
        return ToDto(application);
    }

    public async Task<JobApplicationDto> HireAsync(
        Guid employerUserId, Guid applicationId, RespondJobApplicationRequest request, CancellationToken cancellationToken)
    {
        var application = await GetOwnedByEmployerAsync(employerUserId, applicationId, cancellationToken);

        if (application.Status != JobApplicationStatus.Shortlisted)
        {
            throw new ConflictException("Only a shortlisted application can be hired.");
        }

        application.Status = JobApplicationStatus.Hired;
        application.ResponseMessage = request.ResponseMessage?.Trim();
        application.RespondedAt = DateTime.UtcNow;

        await _applicationRepository.SaveChangesAsync(cancellationToken);
        return ToDto(application);
    }

    public async Task<JobApplicationDto> WithdrawAsync(Guid applicantUserId, Guid applicationId, CancellationToken cancellationToken)
    {
        var application = await _applicationRepository.GetByIdAsync(applicationId, cancellationToken)
            ?? throw new NotFoundException("Application not found.");

        if (application.ApplicantUserId != applicantUserId)
        {
            throw new UnauthorizedAccessException("You do not have permission to withdraw this application.");
        }

        if (application.Status != JobApplicationStatus.Pending && application.Status != JobApplicationStatus.Shortlisted)
        {
            throw new ConflictException("Only a pending or shortlisted application can be withdrawn.");
        }

        application.Status = JobApplicationStatus.Withdrawn;
        application.RespondedAt = DateTime.UtcNow;

        await _applicationRepository.SaveChangesAsync(cancellationToken);
        return ToDto(application);
    }

    public async Task<JobApplicationDto> GetByIdAsync(Guid userId, Guid applicationId, CancellationToken cancellationToken)
    {
        var application = await _applicationRepository.GetByIdAsync(applicationId, cancellationToken)
            ?? throw new NotFoundException("Application not found.");

        if (application.ApplicantUserId != userId && application.JobListing.BusinessPartnerProfile.UserId != userId)
        {
            throw new UnauthorizedAccessException("You do not have permission to view this application.");
        }

        return ToDto(application);
    }

    public async Task<List<JobApplicationListItemDto>> GetMyApplicationsAsync(Guid applicantUserId, CancellationToken cancellationToken)
    {
        var applications = await _applicationRepository.GetByApplicantAsync(applicantUserId, cancellationToken);
        return applications.Select(ToListItemDto).ToList();
    }

    public async Task<List<JobApplicationListItemDto>> GetByJobListingAsync(Guid employerUserId, Guid jobListingId, CancellationToken cancellationToken)
    {
        var jobListing = await _jobListingRepository.GetByIdAsync(jobListingId, cancellationToken)
            ?? throw new NotFoundException("Job listing not found.");

        if (jobListing.BusinessPartnerProfile.UserId != employerUserId)
        {
            throw new UnauthorizedAccessException("You do not have permission to view applications for this job listing.");
        }

        var applications = await _applicationRepository.GetByJobListingAsync(jobListingId, cancellationToken);
        return applications.Select(ToListItemDto).ToList();
    }

    public async Task<PortfolioDto> GetCandidatePortfolioAsync(Guid employerUserId, Guid applicationId, CancellationToken cancellationToken)
    {
        var application = await GetOwnedByEmployerAsync(employerUserId, applicationId, cancellationToken);

        var candidateProfile = await _academyMemberProfileRepository.GetByUserIdAsync(application.ApplicantUserId, cancellationToken)
            ?? throw new NotFoundException("This candidate does not have an academy member profile.");

        return await _portfolioService.GetPortfolioForProfileAsync(candidateProfile.Id, cancellationToken);
    }

    private async Task<JobApplication> GetOwnedByEmployerAsync(Guid employerUserId, Guid applicationId, CancellationToken cancellationToken)
    {
        var application = await _applicationRepository.GetByIdAsync(applicationId, cancellationToken)
            ?? throw new NotFoundException("Application not found.");

        if (application.JobListing.BusinessPartnerProfile.UserId != employerUserId)
        {
            throw new UnauthorizedAccessException("You do not have permission to manage this application.");
        }

        return application;
    }

    private static JobApplicationListItemDto ToListItemDto(JobApplication application) => new()
    {
        Id = application.Id,
        JobListingId = application.JobListingId,
        JobTitle = application.JobListing.Title,
        ApplicantUserId = application.ApplicantUserId,
        ApplicantName = application.Applicant.FullName,
        Status = application.Status.ToString(),
        AppliedAt = application.AppliedAt,
    };

    private static JobApplicationDto ToDto(JobApplication application) => new()
    {
        Id = application.Id,
        JobListingId = application.JobListingId,
        JobTitle = application.JobListing.Title,
        EmployerName = application.JobListing.BusinessPartnerProfile.CompanyName,
        ApplicantUserId = application.ApplicantUserId,
        ApplicantName = application.Applicant.FullName,
        CoverMessage = application.CoverMessage,
        Status = application.Status.ToString(),
        AppliedAt = application.AppliedAt,
        RespondedAt = application.RespondedAt,
        ResponseMessage = application.ResponseMessage,
    };
}
