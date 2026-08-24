using System.Net;
using ShilpoHubBD.Application.DTOs.Learning;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Learning;

namespace ShilpoHubBD.Application.Services.Learning;

public class TrainingCertificateService : ITrainingCertificateService
{
    private readonly ITrainingCertificateRepository _trainingCertificateRepository;
    private readonly IEnrollmentRepository _enrollmentRepository;

    public TrainingCertificateService(
        ITrainingCertificateRepository trainingCertificateRepository, IEnrollmentRepository enrollmentRepository)
    {
        _trainingCertificateRepository = trainingCertificateRepository;
        _enrollmentRepository = enrollmentRepository;
    }

    public async Task<TrainingCertificateDto> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var certificate = await _trainingCertificateRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Training certificate not found.");

        return ToDto(certificate);
    }

    public async Task<List<TrainingCertificateDto>> GetMineAsync(Guid apprenticeUserId, CancellationToken cancellationToken)
    {
        var certificates = await _trainingCertificateRepository.GetByApprenticeAsync(apprenticeUserId, cancellationToken);
        return certificates.Select(ToDto).ToList();
    }

    public async Task<TrainingCertificateVerificationResultDto> VerifyAsync(
        VerifyTrainingCertificateRequest request, CancellationToken cancellationToken)
    {
        var certificateNumber = request.CertificateNumber.Trim();
        var certificate = await _trainingCertificateRepository.GetByCertificateNumberAsync(certificateNumber, cancellationToken);

        if (certificate is null)
        {
            return new TrainingCertificateVerificationResultDto
            {
                IsValid = false,
                CertificateNumber = certificateNumber,
                Message = "No training certificate was found with this number.",
            };
        }

        if (certificate.IsRevoked)
        {
            return new TrainingCertificateVerificationResultDto
            {
                IsValid = false,
                CertificateNumber = certificate.CertificateNumber,
                CourseTitle = certificate.CourseTitle,
                ApprenticeName = certificate.ApprenticeName,
                MentorName = certificate.MentorName,
                IssuedAt = certificate.IssuedAt,
                Message = "This training certificate has been revoked and is no longer valid.",
            };
        }

        return new TrainingCertificateVerificationResultDto
        {
            IsValid = true,
            CertificateNumber = certificate.CertificateNumber,
            CourseTitle = certificate.CourseTitle,
            ApprenticeName = certificate.ApprenticeName,
            MentorName = certificate.MentorName,
            IssuedAt = certificate.IssuedAt,
            Message = "This training certificate is authentic.",
        };
    }

    public async Task RevokeAsync(Guid id, Guid userId, bool isAdmin, CancellationToken cancellationToken)
    {
        var certificate = await _trainingCertificateRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Training certificate not found.");

        if (!isAdmin)
        {
            var enrollment = await _enrollmentRepository.GetByIdAsync(certificate.EnrollmentId, cancellationToken)
                ?? throw new NotFoundException("Enrollment not found.");

            var authorUserId = enrollment.Course.Mentor?.UserId ?? enrollment.Course.TrainerProfile?.UserId;
            if (authorUserId != userId)
            {
                throw new UnauthorizedAccessException("You do not have permission to manage this training certificate.");
            }
        }

        if (certificate.IsRevoked)
        {
            throw new ConflictException("This training certificate has already been revoked.");
        }

        certificate.IsRevoked = true;
        certificate.RevokedAt = DateTime.UtcNow;
        await _trainingCertificateRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task<(string FileName, string Html)> GetDownloadAsync(Guid id, CancellationToken cancellationToken)
    {
        var certificate = await _trainingCertificateRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Training certificate not found.");

        var fileName = $"TrainingCertificate-{certificate.CertificateNumber}.html";
        return (fileName, RenderHtml(certificate));
    }

    private static string RenderHtml(TrainingCertificate certificate)
    {
        var status = certificate.IsRevoked ? "REVOKED" : "AUTHENTIC";
        var statusColor = certificate.IsRevoked ? "#b91c1c" : "#15803d";

        return $$"""
            <!doctype html>
            <html lang="en">
            <head>
            <meta charset="utf-8" />
            <title>Certificate of Training - {{WebUtility.HtmlEncode(certificate.CertificateNumber)}}</title>
            <style>
                body { font-family: Georgia, 'Times New Roman', serif; background: #f5f1e8; margin: 0; padding: 40px; }
                .certificate { max-width: 720px; margin: 0 auto; background: #fffdf7; border: 3px double #8a6d3b; padding: 48px; text-align: center; }
                h1 { font-size: 28px; letter-spacing: 2px; color: #4a3b22; margin-bottom: 4px; }
                .subtitle { color: #7a6a4a; margin-bottom: 32px; }
                .field { margin: 12px 0; font-size: 16px; }
                .field .label { color: #7a6a4a; font-size: 12px; text-transform: uppercase; letter-spacing: 1px; display: block; }
                .field .value { color: #2a2116; font-size: 18px; }
                .status { display: inline-block; margin-top: 24px; padding: 8px 20px; border-radius: 4px; font-weight: bold; letter-spacing: 1px; color: #fff; background: {{statusColor}}; }
                .number { margin-top: 24px; font-size: 14px; color: #7a6a4a; }
            </style>
            </head>
            <body>
                <div class="certificate">
                    <h1>Certificate of Training</h1>
                    <div class="subtitle">ShilpoHubBD - Heritage Learning & Mentorship</div>

                    <div class="field">
                        <span class="label">Course</span>
                        <span class="value">{{WebUtility.HtmlEncode(certificate.CourseTitle)}}</span>
                    </div>
                    <div class="field">
                        <span class="label">Apprentice</span>
                        <span class="value">{{WebUtility.HtmlEncode(certificate.ApprenticeName)}}</span>
                    </div>
                    <div class="field">
                        <span class="label">Mentor</span>
                        <span class="value">{{WebUtility.HtmlEncode(certificate.MentorName)}}</span>
                    </div>
                    <div class="field">
                        <span class="label">Issued</span>
                        <span class="value">{{certificate.IssuedAt:MMMM d, yyyy}}</span>
                    </div>

                    <div class="status">{{status}}</div>
                    <div class="number">Certificate No. {{WebUtility.HtmlEncode(certificate.CertificateNumber)}}</div>
                </div>
            </body>
            </html>
            """;
    }

    private static TrainingCertificateDto ToDto(TrainingCertificate certificate) => new()
    {
        Id = certificate.Id,
        EnrollmentId = certificate.EnrollmentId,
        CertificateNumber = certificate.CertificateNumber,
        CourseTitle = certificate.CourseTitle,
        ApprenticeName = certificate.ApprenticeName,
        MentorName = certificate.MentorName,
        IsRevoked = certificate.IsRevoked,
        RevokedAt = certificate.RevokedAt,
        IssuedAt = certificate.IssuedAt,
    };
}
