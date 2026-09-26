using FluentValidation;
using ShilpoHubBD.Application.DTOs.Community;

namespace ShilpoHubBD.Application.Validators.Community;

public class CreateQuestionRequestValidator : AbstractValidator<CreateQuestionRequest>
{
    public CreateQuestionRequestValidator()
    {
        RuleFor(x => x.Body).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.ImageUrl).MaximumLength(500).Must(u => u is null || u.StartsWith("/uploads/")).WithMessage("Use an image uploaded through the chat image upload.");
    }
}
