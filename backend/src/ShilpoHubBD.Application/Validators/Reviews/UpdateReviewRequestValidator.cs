using FluentValidation;
using ShilpoHubBD.Application.DTOs.Reviews;

namespace ShilpoHubBD.Application.Validators.Reviews;

public class UpdateReviewRequestValidator : AbstractValidator<UpdateReviewRequest>
{
    public UpdateReviewRequestValidator()
    {
        RuleFor(x => x.Rating).InclusiveBetween(1, 5);
        RuleFor(x => x.Comment).NotEmpty().MaximumLength(2000);
        RuleForEach(x => x.ImageUrls).NotEmpty().MaximumLength(2000);
    }
}
