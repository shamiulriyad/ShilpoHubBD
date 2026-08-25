using FluentValidation;
using ShilpoHubBD.Application.DTOs.Mentorship;

namespace ShilpoHubBD.Application.Validators.Mentorship;

public class CreateMentorshipRequestRequestValidator : AbstractValidator<CreateMentorshipRequestRequest>
{
    public CreateMentorshipRequestRequestValidator()
    {
        RuleFor(x => x.MentorProfileId).NotEmpty();
        RuleFor(x => x.Message).NotEmpty().MaximumLength(2000);
    }
}
