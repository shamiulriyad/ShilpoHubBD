using ShilpoHubBD.Application.DTOs.Apprenticeship;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Apprenticeship;
using ShilpoHubBD.Domain.Entities.Learning;

namespace ShilpoHubBD.Application.Services.Apprenticeship;

public class ApprenticeEnrollmentService : IApprenticeEnrollmentService
{
    private readonly IApprenticeEnrollmentRepository _enrollmentRepository;
    private readonly IApprenticeshipProgramRepository _programRepository;
    private readonly ITrainingCertificateRepository _trainingCertificateRepository;

    public ApprenticeEnrollmentService(
        IApprenticeEnrollmentRepository enrollmentRepository,
        IApprenticeshipProgramRepository programRepository,
        ITrainingCertificateRepository trainingCertificateRepository)
    {
        _enrollmentRepository = enrollmentRepository;
        _programRepository = programRepository;
        _trainingCertificateRepository = trainingCertificateRepository;
    }

    public async Task<List<ApprenticeEnrollmentListItemDto>> GetMyEnrollmentsAsync(Guid apprenticeUserId, CancellationToken cancellationToken)
    {
        var enrollments = await _enrollmentRepository.GetByApprenticeAsync(apprenticeUserId, cancellationToken);
        return enrollments.Select(ToListItemDto).ToList();
    }

    public async Task<ApprenticeEnrollmentDto> GetByIdAsync(Guid userId, bool isAdmin, Guid enrollmentId, CancellationToken cancellationToken)
    {
        var enrollment = await _enrollmentRepository.GetByIdAsync(enrollmentId, cancellationToken)
            ?? throw new NotFoundException("Enrollment not found.");

        if (!isAdmin && enrollment.ApprenticeUserId != userId && ProviderUserId(enrollment.Program) != userId)
        {
            throw new UnauthorizedAccessException("You do not have permission to view this enrollment.");
        }

        return ToDto(enrollment);
    }

    public async Task<List<ApprenticeEnrollmentListItemDto>> GetByProgramAsync(Guid providerUserId, Guid programId, CancellationToken cancellationToken)
    {
        await GetOwnedProgramAsync(providerUserId, programId, cancellationToken);

        var enrollments = await _enrollmentRepository.GetByProgramAsync(programId, cancellationToken);
        return enrollments.Select(ToListItemDto).ToList();
    }

    public async Task<ApprenticeEnrollmentDto> UpdateMilestoneProgressAsync(
        Guid providerUserId, Guid enrollmentId, Guid milestoneId, UpdateMilestoneProgressRequest request, CancellationToken cancellationToken)
    {
        var enrollment = await GetEnrollmentOwnedByProviderAsync(providerUserId, enrollmentId, cancellationToken);

        if (enrollment.Program.Milestones.All(m => m.Id != milestoneId))
        {
            throw new NotFoundException("Milestone not found in this program.");
        }

        var progress = enrollment.MilestoneProgress.FirstOrDefault(p => p.MilestoneId == milestoneId);
        if (progress is null)
        {
            progress = new ApprenticeMilestoneProgress
            {
                Id = Guid.NewGuid(),
                EnrollmentId = enrollment.Id,
                MilestoneId = milestoneId,
                IsCompleted = request.IsCompleted,
                CompletedAt = request.IsCompleted ? DateTime.UtcNow : null,
                Notes = request.Notes?.Trim(),
            };
            await _enrollmentRepository.AddMilestoneProgressAsync(progress, cancellationToken);
            enrollment.MilestoneProgress.Add(progress);
        }
        else
        {
            progress.IsCompleted = request.IsCompleted;
            progress.CompletedAt = request.IsCompleted ? DateTime.UtcNow : null;
            progress.Notes = request.Notes?.Trim();
        }

        await _enrollmentRepository.SaveChangesAsync(cancellationToken);
        return ToDto(enrollment);
    }

    public async Task<ApprenticeEnrollmentDto> CompleteAsync(Guid providerUserId, Guid enrollmentId, CancellationToken cancellationToken)
    {
        var enrollment = await GetEnrollmentOwnedByProviderAsync(providerUserId, enrollmentId, cancellationToken);

        if (enrollment.Status == ApprenticeEnrollmentStatus.Completed)
        {
            throw new ConflictException("This enrollment has already been marked complete.");
        }

        var now = DateTime.UtcNow;
        enrollment.Status = ApprenticeEnrollmentStatus.Completed;
        enrollment.CompletedAt = now;

        var existingCertificate = await _trainingCertificateRepository.GetByApprenticeEnrollmentIdAsync(enrollment.Id, cancellationToken);
        if (existingCertificate is null)
        {
            var certificate = new TrainingCertificate
            {
                Id = Guid.NewGuid(),
                Type = CertificateType.Apprenticeship,
                RecipientUserId = enrollment.ApprenticeUserId,
                IssuerUserId = ProviderUserId(enrollment.Program),
                ApprenticeEnrollmentId = enrollment.Id,
                CertificateNumber = GenerateCertificateNumber(now),
                Title = enrollment.Program.Title,
                RecipientName = enrollment.Apprentice.FullName,
                IssuerName = ProviderNameOf(enrollment.Program),
                IsRevoked = false,
                IssuedAt = now,
            };

            await _trainingCertificateRepository.AddAsync(certificate, cancellationToken);
        }

        await _enrollmentRepository.SaveChangesAsync(cancellationToken);
        return ToDto(enrollment);
    }

    private async Task<ApprenticeshipProgram> GetOwnedProgramAsync(Guid userId, Guid programId, CancellationToken cancellationToken)
    {
        var program = await _programRepository.GetByIdAsync(programId, cancellationToken)
            ?? throw new NotFoundException("Program not found.");

        if (ProviderUserId(program) != userId)
        {
            throw new UnauthorizedAccessException("You do not have permission to manage this program.");
        }

        return program;
    }

    private async Task<ApprenticeEnrollment> GetEnrollmentOwnedByProviderAsync(Guid providerUserId, Guid enrollmentId, CancellationToken cancellationToken)
    {
        var enrollment = await _enrollmentRepository.GetByIdAsync(enrollmentId, cancellationToken)
            ?? throw new NotFoundException("Enrollment not found.");

        if (ProviderUserId(enrollment.Program) != providerUserId)
        {
            throw new UnauthorizedAccessException("You do not have permission to manage this enrollment.");
        }

        return enrollment;
    }

    private static Guid? ProviderUserId(ApprenticeshipProgram program)
        => program.Mentor?.UserId ?? program.TrainerProfile?.UserId;

    private static string ProviderNameOf(ApprenticeshipProgram program)
        => program.Mentor?.User.FullName ?? program.TrainerProfile?.User.FullName ?? string.Empty;

    private static string GenerateCertificateNumber(DateTime issuedAt)
        => $"SH-APR-{issuedAt:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";

    private static decimal CalculateProgressPercent(ApprenticeEnrollment enrollment)
    {
        var totalMilestones = enrollment.Program.Milestones.Count;
        if (totalMilestones == 0)
        {
            return 0;
        }

        var completedMilestones = enrollment.MilestoneProgress.Count(p => p.IsCompleted);
        return Math.Round(completedMilestones / (decimal)totalMilestones * 100m, 1);
    }

    private static ApprenticeEnrollmentListItemDto ToListItemDto(ApprenticeEnrollment enrollment) => new()
    {
        Id = enrollment.Id,
        ProgramId = enrollment.ProgramId,
        ProgramTitle = enrollment.Program.Title,
        ApprenticeUserId = enrollment.ApprenticeUserId,
        ApprenticeName = enrollment.Apprentice.FullName,
        Status = enrollment.Status.ToString(),
        EnrolledAt = enrollment.EnrolledAt,
        CompletedAt = enrollment.CompletedAt,
        ProgressPercent = CalculateProgressPercent(enrollment),
    };

    private static ApprenticeEnrollmentDto ToDto(ApprenticeEnrollment enrollment)
    {
        var totalMilestones = enrollment.Program.Milestones.Count;
        var completedMilestones = enrollment.MilestoneProgress.Count(p => p.IsCompleted);

        return new ApprenticeEnrollmentDto
        {
            Id = enrollment.Id,
            ProgramId = enrollment.ProgramId,
            ProgramTitle = enrollment.Program.Title,
            ApprenticeUserId = enrollment.ApprenticeUserId,
            ApprenticeName = enrollment.Apprentice.FullName,
            Status = enrollment.Status.ToString(),
            EnrolledAt = enrollment.EnrolledAt,
            CompletedAt = enrollment.CompletedAt,
            TotalMilestones = totalMilestones,
            CompletedMilestones = completedMilestones,
            ProgressPercent = CalculateProgressPercent(enrollment),
            Milestones = enrollment.Program.Milestones
                .OrderBy(m => m.DisplayOrder)
                .Select(m =>
                {
                    var progress = enrollment.MilestoneProgress.FirstOrDefault(p => p.MilestoneId == m.Id);
                    return new MilestoneProgressDto
                    {
                        MilestoneId = m.Id,
                        Title = m.Title,
                        DisplayOrder = m.DisplayOrder,
                        IsCompleted = progress?.IsCompleted ?? false,
                        CompletedAt = progress?.CompletedAt,
                        Notes = progress?.Notes,
                    };
                })
                .ToList(),
        };
    }
}
