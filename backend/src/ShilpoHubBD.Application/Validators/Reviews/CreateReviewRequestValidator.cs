using FluentValidation;
using ShilpoHubBD.Application.DTOs.Reviews;

namespace ShilpoHubBD.Application.Validators.Reviews;

public class CreateReviewRequestValidator : AbstractValidator<CreateReviewRequest>
{
    public CreateReviewRequestValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Rating).InclusiveBetween(1, 5);
        RuleFor(x => x.Comment).NotEmpty().MaximumLength(2000);
        RuleForEach(x => x.ImageUrls).NotEmpty().MaximumLength(2000);
    }
}
