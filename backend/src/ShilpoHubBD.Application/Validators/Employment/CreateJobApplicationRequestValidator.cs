using FluentValidation;
using ShilpoHubBD.Application.DTOs.Employment;

namespace ShilpoHubBD.Application.Validators.Employment;

public class CreateJobApplicationRequestValidator : AbstractValidator<CreateJobApplicationRequest>
{
    public CreateJobApplicationRequestValidator()
    {
        RuleFor(x => x.JobListingId).NotEmpty();
        RuleFor(x => x.CoverMessage).NotEmpty().MaximumLength(2000);
    }
}
