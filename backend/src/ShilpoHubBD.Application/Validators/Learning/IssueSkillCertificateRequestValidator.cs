using FluentValidation;
using ShilpoHubBD.Application.DTOs.Learning;

namespace ShilpoHubBD.Application.Validators.Learning;

public class IssueSkillCertificateRequestValidator : AbstractValidator<IssueSkillCertificateRequest>
{
    public IssueSkillCertificateRequestValidator()
    {
        RuleFor(x => x.RecipientUserId).NotEmpty();
        RuleFor(x => x.HeritageSkillId).NotEmpty();
    }
}
