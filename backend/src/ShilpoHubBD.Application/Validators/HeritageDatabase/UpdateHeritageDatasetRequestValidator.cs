using FluentValidation;
using ShilpoHubBD.Application.DTOs.HeritageDatabase;
using ShilpoHubBD.Domain.Entities.HeritageDatabase;

namespace ShilpoHubBD.Application.Validators.HeritageDatabase;

public class UpdateHeritageDatasetRequestValidator : AbstractValidator<UpdateHeritageDatasetRequest>
{
    public UpdateHeritageDatasetRequestValidator()
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
        RuleFor(x => x.Status)
            .NotEmpty()
            .Must(v => Enum.TryParse<HeritageDatasetStatus>(v, true, out _))
            .WithMessage("Status must be one of: Draft, Published, Archived, Deprecated.");
        RuleFor(x => x.AccessLevel)
            .NotEmpty()
            .Must(v => Enum.TryParse<HeritageDatasetAccessLevel>(v, true, out _))
            .WithMessage("AccessLevel must be one of: Public, Researcher, Restricted.");
        RuleFor(x => x.SourceType)
            .NotEmpty()
            .Must(v => Enum.TryParse<HeritageDatasetSourceType>(v, true, out _))
            .WithMessage("SourceType must be a valid source type.");
    }
}
