using FluentValidation;
using ShilpoHubBD.Application.DTOs.Marketplace;

namespace ShilpoHubBD.Application.Validators.Marketplace;

public class CreateWorkshopGalleryItemRequestValidator : AbstractValidator<CreateWorkshopGalleryItemRequest>
{
    public CreateWorkshopGalleryItemRequestValidator()
    {
        RuleFor(x => x.MediaUrl).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.MediaType).IsInEnum();
        RuleFor(x => x.Caption).MaximumLength(500);
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}
