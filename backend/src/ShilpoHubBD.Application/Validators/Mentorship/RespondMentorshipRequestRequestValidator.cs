using FluentValidation;
using ShilpoHubBD.Application.DTOs.Mentorship;

namespace ShilpoHubBD.Application.Validators.Mentorship;

public class RespondMentorshipRequestRequestValidator : AbstractValidator<RespondMentorshipRequestRequest>
{
    public RespondMentorshipRequestRequestValidator()
    {
        RuleFor(x => x.ResponseMessage).MaximumLength(2000);
    }
}
