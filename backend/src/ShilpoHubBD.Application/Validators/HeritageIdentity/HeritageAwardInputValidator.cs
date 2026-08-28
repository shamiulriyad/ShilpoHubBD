using FluentValidation;
using ShilpoHubBD.Application.DTOs.HeritageIdentity;

namespace ShilpoHubBD.Application.Validators.HeritageIdentity;

public class HeritageAwardInputValidator : AbstractValidator<HeritageAwardInput>
{
    public HeritageAwardInputValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.IssuingOrganization).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Year).InclusiveBetween(1700, 2100);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.ImageUrl).MaximumLength(500);
    }
}
