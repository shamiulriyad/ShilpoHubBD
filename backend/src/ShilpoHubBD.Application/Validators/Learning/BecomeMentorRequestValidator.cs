using FluentValidation;
using ShilpoHubBD.Application.DTOs.Learning;

namespace ShilpoHubBD.Application.Validators.Learning;

public class BecomeMentorRequestValidator : AbstractValidator<BecomeMentorRequest>
{
    public BecomeMentorRequestValidator()
    {
        RuleFor(x => x.ProofImageUrl).NotEmpty().MaximumLength(1000)
            .Matches(@"^/uploads/images/[a-f0-9]{32}\.(jpg|png|webp)$")
            .WithMessage("Upload a photo of your craft expertise as proof.");
        RuleFor(x => x.Bio).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.Expertise).NotEmpty().MaximumLength(500);
        RuleFor(x => x.YearsOfExperience).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Location).MaximumLength(200);
        RuleFor(x => x.AvailabilityNote).MaximumLength(500);
        RuleFor(x => x.PreferredCategory).MaximumLength(100);
    }
}
