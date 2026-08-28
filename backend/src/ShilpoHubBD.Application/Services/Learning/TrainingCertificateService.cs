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
    private readonly IMentorRepository _mentorRepository;
    private readonly IAcademyMemberProfileRepository _academyMemberProfileRepository;
    private readonly IHeritageSkillRepository _heritageSkillRepository;
    private readonly IUserRepository _userRepository;

    public TrainingCertificateService(
        ITrainingCertificateRepository trainingCertificateRepository,
        IMentorRepository mentorRepository,
        IAcademyMemberProfileRepository academyMemberProfileRepository,
        IHeritageSkillRepository heritageSkillRepository,
        IUserRepository userRepository)
    {
        _trainingCertificateRepository = trainingCertificateRepository;
        _mentorRepository = mentorRepository;
        _academyMemberProfileRepository = academyMemberProfileRepository;
        _heritageSkillRepository = heritageSkillRepository;
        _userRepository = userRepository;
    }

    public async Task<TrainingCertificateDto> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var certificate = await _trainingCertificateRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Certificate not found.");

        return ToDto(certificate);
    }

    public async Task<List<TrainingCertificateDto>> GetMineAsync(Guid recipientUserId, CancellationToken cancellationToken)
    {
        var certificates = await _trainingCertificateRepository.GetByRecipientAsync(recipientUserId, cancellationToken);
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
                Message = "No certificate was found with this number.",
            };
        }

        if (certificate.IsRevoked)
        {
            return new TrainingCertificateVerificationResultDto
            {
                IsValid = false,
                CertificateNumber = certificate.CertificateNumber,
                Type = certificate.Type.ToString(),
                Title = certificate.Title,
                RecipientName = certificate.RecipientName,
                IssuerName = certificate.IssuerName,
                IssuedAt = certificate.IssuedAt,
                Message = "This certificate has been revoked and is no longer valid.",
            };
        }

        return new TrainingCertificateVerificationResultDto
        {
            IsValid = true,
            CertificateNumber = certificate.CertificateNumber,
            Type = certificate.Type.ToString(),
            Title = certificate.Title,
            RecipientName = certificate.RecipientName,
            IssuerName = certificate.IssuerName,
            IssuedAt = certificate.IssuedAt,
            Message = "This certificate is authentic.",
        };
    }

    public async Task<TrainingCertificateDto> IssueSkillCertificateAsync(
        Guid issuerUserId, IssueSkillCertificateRequest request, CancellationToken cancellationToken)
    {
        var issuerName = await ResolveIssuerNameAsync(issuerUserId, cancellationToken);

        var heritageSkill = await _heritageSkillRepository.GetByIdAsync(request.HeritageSkillId, cancellationToken)
            ?? throw new NotFoundException("Heritage skill not found.");

        var recipientProfile = await _academyMemberProfileRepository.GetByUserIdAsync(request.RecipientUserId, cancellationToken)
            ?? throw new NotFoundException("The recipient does not have an academy member profile.");

        var recipientSkill = await _academyMemberProfileRepository.GetSkillAsync(recipientProfile.Id, request.HeritageSkillId, cancellationToken);
        if (recipientSkill is null || recipientSkill.Level < SkillLevel.Advanced)
        {
            throw new ConflictException(
                "The learner must reach an advanced skill level in this heritage skill before a certificate can be issued.");
        }

        if (await _trainingCertificateRepository.GetActiveSkillCertificateAsync(
                request.RecipientUserId, request.HeritageSkillId, cancellationToken) is not null)
        {
            throw new ConflictException("A skill certificate has already been issued to this learner for this heritage skill.");
        }

        var recipient = await _userRepository.GetByIdAsync(request.RecipientUserId, cancellationToken)
            ?? throw new NotFoundException("Recipient not found.");

        var now = DateTime.UtcNow;
        var certificate = new TrainingCertificate
        {
            Id = Guid.NewGuid(),
            Type = CertificateType.Skill,
            RecipientUserId = recipient.Id,
            IssuerUserId = issuerUserId,
            HeritageSkillId = heritageSkill.Id,
            CertificateNumber = GenerateCertificateNumber(now, CertificateType.Skill),
            Title = heritageSkill.Name,
            RecipientName = recipient.FullName,
            IssuerName = issuerName,
            IsRevoked = false,
            IssuedAt = now,
        };

        await _trainingCertificateRepository.AddAsync(certificate, cancellationToken);
        await _trainingCertificateRepository.SaveChangesAsync(cancellationToken);

        var created = await _trainingCertificateRepository.GetByIdAsync(certificate.Id, cancellationToken);
        return ToDto(created!);
    }

    public async Task RevokeAsync(Guid id, Guid userId, bool isAdmin, CancellationToken cancellationToken)
    {
        var certificate = await _trainingCertificateRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Certificate not found.");

        if (!isAdmin && certificate.IssuerUserId != userId)
        {
            throw new UnauthorizedAccessException("You do not have permission to manage this certificate.");
        }

        if (certificate.IsRevoked)
        {
            throw new ConflictException("This certificate has already been revoked.");
        }

        certificate.IsRevoked = true;
        certificate.RevokedAt = DateTime.UtcNow;
        await _trainingCertificateRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task<(string FileName, string Html)> GetDownloadAsync(Guid id, CancellationToken cancellationToken)
    {
        var certificate = await _trainingCertificateRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Certificate not found.");

        var fileName = $"Certificate-{certificate.CertificateNumber}.html";
        return (fileName, RenderHtml(certificate));
    }

    private async Task<string> ResolveIssuerNameAsync(Guid userId, CancellationToken cancellationToken)
    {
        var mentor = await _mentorRepository.GetByUserIdAsync(userId, cancellationToken);
        if (mentor is not null)
        {
            return mentor.User.FullName;
        }

        var trainerProfile = await _academyMemberProfileRepository.GetByUserIdAsync(userId, cancellationToken);
        if (trainerProfile is not null && trainerProfile.Role == AcademyMemberRole.Trainer)
        {
            return trainerProfile.User.FullName;
        }

        throw new ConflictException("You must have a mentor profile or a trainer academy profile before issuing skill certificates.");
    }

    private static string GenerateCertificateNumber(DateTime issuedAt, CertificateType type)
    {
        var prefix = type switch
        {
            CertificateType.Skill => "SKL",
            CertificateType.Apprenticeship => "APR",
            _ => "TRN",
        };
        return $"SH-{prefix}-{issuedAt:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
    }

    private static string RenderHtml(TrainingCertificate certificate)
    {
        var status = certificate.IsRevoked ? "REVOKED" : "AUTHENTIC";
        var statusColor = certificate.IsRevoked ? "#b91c1c" : "#15803d";
        var heading = certificate.Type switch
        {
            CertificateType.Skill => "Certificate of Skill Mastery",
            CertificateType.Apprenticeship => "Certificate of Apprenticeship",
            _ => "Certificate of Training",
        };
        var subjectLabel = certificate.Type switch
        {
            CertificateType.Skill => "Heritage Skill",
            CertificateType.Apprenticeship => "Program",
            _ => "Course",
        };

        return $$"""
            <!doctype html>
            <html lang="en">
            <head>
            <meta charset="utf-8" />
            <title>{{WebUtility.HtmlEncode(heading)}} - {{WebUtility.HtmlEncode(certificate.CertificateNumber)}}</title>
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
                    <h1>{{WebUtility.HtmlEncode(heading)}}</h1>
                    <div class="subtitle">ShilpoHubBD - Heritage Learning & Mentorship</div>

                    <div class="field">
                        <span class="label">{{subjectLabel}}</span>
                        <span class="value">{{WebUtility.HtmlEncode(certificate.Title)}}</span>
                    </div>
                    <div class="field">
                        <span class="label">Recipient</span>
                        <span class="value">{{WebUtility.HtmlEncode(certificate.RecipientName)}}</span>
                    </div>
                    <div class="field">
                        <span class="label">Issued By</span>
                        <span class="value">{{WebUtility.HtmlEncode(certificate.IssuerName)}}</span>
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
        Type = certificate.Type.ToString(),
        RecipientUserId = certificate.RecipientUserId,
        EnrollmentId = certificate.EnrollmentId,
        ApprenticeEnrollmentId = certificate.ApprenticeEnrollmentId,
        HeritageSkillId = certificate.HeritageSkillId,
        CertificateNumber = certificate.CertificateNumber,
        Title = certificate.Title,
        RecipientName = certificate.RecipientName,
        IssuerName = certificate.IssuerName,
        IsRevoked = certificate.IsRevoked,
        RevokedAt = certificate.RevokedAt,
        IssuedAt = certificate.IssuedAt,
    };
}
