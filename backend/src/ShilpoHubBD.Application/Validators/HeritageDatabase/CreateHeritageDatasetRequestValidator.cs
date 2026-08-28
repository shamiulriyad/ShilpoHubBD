using FluentValidation;
using ShilpoHubBD.Application.DTOs.HeritageDatabase;
using ShilpoHubBD.Domain.Entities.HeritageDatabase;

namespace ShilpoHubBD.Application.Validators.HeritageDatabase;

public class CreateHeritageDatasetRequestValidator : AbstractValidator<CreateHeritageDatasetRequest>
{
    public CreateHeritageDatasetRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.SourceOrganization).MaximumLength(200);
        RuleFor(x => x.SourceReference).MaximumLength(500);
        RuleFor(x => x.License).MaximumLength(200);
        RuleFor(x => x.Tags).MaximumLength(500);
        RuleFor(x => x.Category)
            .NotEmpty()
            .Must(v => Enum.TryParse<HeritageDatasetCategory>(v, true, out _))
            .WithMessage("Category must be a valid dataset category.");
        RuleFor(x => x.AccessLevel)
            .Must(v => string.IsNullOrWhiteSpace(v) || Enum.TryParse<HeritageDatasetAccessLevel>(v, true, out _))
            .WithMessage("AccessLevel must be one of: Public, Researcher, Restricted.");
        RuleFor(x => x.SourceType)
            .Must(v => string.IsNullOrWhiteSpace(v) || Enum.TryParse<HeritageDatasetSourceType>(v, true, out _))
            .WithMessage("SourceType must be a valid source type.");
    }
}
