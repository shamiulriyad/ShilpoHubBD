using FluentValidation;
using ShilpoHubBD.Application.DTOs.AIShopping;

namespace ShilpoHubBD.Application.Validators.AIShopping;

public class InteriorPreviewRequestValidator : AbstractValidator<InteriorPreviewRequest>
{
    public InteriorPreviewRequestValidator()
    {
        RuleFor(x => x.ProductName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.RoomType).NotEmpty().MaximumLength(100);
    }
}
