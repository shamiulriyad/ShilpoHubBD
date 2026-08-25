using ShilpoHubBD.Application.DTOs.Apprenticeship;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Apprenticeship;

namespace ShilpoHubBD.Application.Services.Apprenticeship;

public class ProgramApplicationService : IProgramApplicationService
{
    private readonly IProgramApplicationRepository _applicationRepository;
    private readonly IApprenticeshipProgramRepository _programRepository;
    private readonly IApprenticeEnrollmentRepository _enrollmentRepository;

    public ProgramApplicationService(
        IProgramApplicationRepository applicationRepository,
        IApprenticeshipProgramRepository programRepository,
        IApprenticeEnrollmentRepository enrollmentRepository)
    {
        _applicationRepository = applicationRepository;
        _programRepository = programRepository;
        _enrollmentRepository = enrollmentRepository;
    }

    public async Task<ProgramApplicationDto> ApplyAsync(Guid applicantUserId, CreateProgramApplicationRequest request, CancellationToken cancellationToken)
    {
        var program = await _programRepository.GetByIdAsync(request.ProgramId, cancellationToken)
            ?? throw new NotFoundException("Program not found.");

        if (program.Status != ProgramStatus.Published)
        {
            throw new ConflictException("You can only apply to published programs.");
        }

        if (ProviderUserId(program) == applicantUserId)
        {
            throw new ConflictException("You cannot apply to your own program.");
        }

        if (await _applicationRepository.HasOpenApplicationAsync(program.Id, applicantUserId, cancellationToken))
        {
            throw new ConflictException("You already have a pending or accepted application for this program.");
        }

        var application = new ProgramApplication
        {
            Id = Guid.NewGuid(),
            ProgramId = program.Id,
            ApplicantUserId = applicantUserId,
            Message = request.Message.Trim(),
            Status = ApplicationStatus.Pending,
            AppliedAt = DateTime.UtcNow,
        };

        await _applicationRepository.AddAsync(application, cancellationToken);
        await _applicationRepository.SaveChangesAsync(cancellationToken);

        var created = await _applicationRepository.GetByIdAsync(application.Id, cancellationToken);
        return ToDto(created!);
    }

    public async Task<ProgramApplicationDto> AcceptAsync(
        Guid providerUserId, Guid applicationId, RespondProgramApplicationRequest request, CancellationToken cancellationToken)
    {
        var application = await GetOwnedByProviderAsync(providerUserId, applicationId, cancellationToken);

        if (application.Status != ApplicationStatus.Pending)
        {
            throw new ConflictException("Only a pending application can be accepted.");
        }

        if (application.Program.Capacity.HasValue)
        {
            var activeCount = await _enrollmentRepository.GetActiveCountByProgramAsync(application.ProgramId, cancellationToken);
            if (activeCount >= application.Program.Capacity.Value)
            {
                throw new ConflictException("This program has reached its capacity.");
            }
        }

        application.Status = ApplicationStatus.Accepted;
        application.ResponseMessage = request.ResponseMessage?.Trim();
        application.RespondedAt = DateTime.UtcNow;

        var enrollment = new ApprenticeEnrollment
        {
            Id = Guid.NewGuid(),
            ProgramId = application.ProgramId,
            ApprenticeUserId = application.ApplicantUserId,
            ApplicationId = application.Id,
            Status = ApprenticeEnrollmentStatus.Active,
            EnrolledAt = DateTime.UtcNow,
        };
        await _enrollmentRepository.AddAsync(enrollment, cancellationToken);

        await _applicationRepository.SaveChangesAsync(cancellationToken);
        return ToDto(application);
    }

    public async Task<ProgramApplicationDto> RejectAsync(
        Guid providerUserId, Guid applicationId, RespondProgramApplicationRequest request, CancellationToken cancellationToken)
    {
        var application = await GetOwnedByProviderAsync(providerUserId, applicationId, cancellationToken);

        if (application.Status != ApplicationStatus.Pending)
        {
            throw new ConflictException("Only a pending application can be rejected.");
        }

        application.Status = ApplicationStatus.Rejected;
        application.ResponseMessage = request.ResponseMessage?.Trim();
        application.RespondedAt = DateTime.UtcNow;

        await _applicationRepository.SaveChangesAsync(cancellationToken);
        return ToDto(application);
    }

    public async Task<ProgramApplicationDto> WithdrawAsync(Guid applicantUserId, Guid applicationId, CancellationToken cancellationToken)
    {
        var application = await _applicationRepository.GetByIdAsync(applicationId, cancellationToken)
            ?? throw new NotFoundException("Application not found.");

        if (application.ApplicantUserId != applicantUserId)
        {
            throw new UnauthorizedAccessException("You do not have permission to withdraw this application.");
        }

        if (application.Status != ApplicationStatus.Pending)
        {
            throw new ConflictException("Only a pending application can be withdrawn.");
        }

        application.Status = ApplicationStatus.Withdrawn;
        application.RespondedAt = DateTime.UtcNow;

        await _applicationRepository.SaveChangesAsync(cancellationToken);
        return ToDto(application);
    }

    public async Task<ProgramApplicationDto> GetByIdAsync(Guid userId, Guid applicationId, CancellationToken cancellationToken)
    {
        var application = await _applicationRepository.GetByIdAsync(applicationId, cancellationToken)
            ?? throw new NotFoundException("Application not found.");

        if (application.ApplicantUserId != userId && ProviderUserId(application.Program) != userId)
        {
            throw new UnauthorizedAccessException("You do not have permission to view this application.");
        }

        return ToDto(application);
    }

    public async Task<List<ProgramApplicationListItemDto>> GetMyApplicationsAsync(Guid applicantUserId, CancellationToken cancellationToken)
    {
        var applications = await _applicationRepository.GetByApplicantAsync(applicantUserId, cancellationToken);
        return applications.Select(ToListItemDto).ToList();
    }

    public async Task<List<ProgramApplicationListItemDto>> GetByProgramAsync(Guid providerUserId, Guid programId, CancellationToken cancellationToken)
    {
        var program = await _programRepository.GetByIdAsync(programId, cancellationToken)
            ?? throw new NotFoundException("Program not found.");

        if (ProviderUserId(program) != providerUserId)
        {
            throw new UnauthorizedAccessException("You do not have permission to view applications for this program.");
        }

        var applications = await _applicationRepository.GetByProgramAsync(programId, cancellationToken);
        return applications.Select(ToListItemDto).ToList();
    }

    private async Task<ProgramApplication> GetOwnedByProviderAsync(Guid providerUserId, Guid applicationId, CancellationToken cancellationToken)
    {
        var application = await _applicationRepository.GetByIdAsync(applicationId, cancellationToken)
            ?? throw new NotFoundException("Application not found.");

        if (ProviderUserId(application.Program) != providerUserId)
        {
            throw new UnauthorizedAccessException("You do not have permission to manage this application.");
        }

        return application;
    }

    private static Guid? ProviderUserId(Domain.Entities.Apprenticeship.ApprenticeshipProgram program)
        => program.Mentor?.UserId ?? program.TrainerProfile?.UserId;

    private static string ProviderNameOf(Domain.Entities.Apprenticeship.ApprenticeshipProgram program)
        => program.Mentor?.User.FullName ?? program.TrainerProfile?.User.FullName ?? string.Empty;

    private static ProgramApplicationListItemDto ToListItemDto(ProgramApplication application) => new()
    {
        Id = application.Id,
        ProgramId = application.ProgramId,
        ProgramTitle = application.Program.Title,
        ApplicantUserId = application.ApplicantUserId,
        ApplicantName = application.Applicant.FullName,
        Status = application.Status.ToString(),
        AppliedAt = application.AppliedAt,
    };

    private static ProgramApplicationDto ToDto(ProgramApplication application) => new()
    {
        Id = application.Id,
        ProgramId = application.ProgramId,
        ProgramTitle = application.Program.Title,
        ProviderName = ProviderNameOf(application.Program),
        ApplicantUserId = application.ApplicantUserId,
        ApplicantName = application.Applicant.FullName,
        Message = application.Message,
        Status = application.Status.ToString(),
        AppliedAt = application.AppliedAt,
        RespondedAt = application.RespondedAt,
        ResponseMessage = application.ResponseMessage,
    };
}
