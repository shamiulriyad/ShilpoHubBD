using FluentValidation;
using ShilpoHubBD.Application.DTOs.ProductDevelopment;

namespace ShilpoHubBD.Application.Validators.ProductDevelopment;

public class AddDevelopmentCommentRequestValidator : AbstractValidator<AddDevelopmentCommentRequest>
{
    public AddDevelopmentCommentRequestValidator()
    {
        RuleFor(x => x.Content).NotEmpty().MaximumLength(2000);
    }
}
