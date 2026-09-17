using FluentValidation;
using ShilpoHubBD.Application.DTOs.SentimentAnalysis;

namespace ShilpoHubBD.Application.Validators.SentimentAnalysis;

public class AnalyzeSentimentRequestValidator : AbstractValidator<AnalyzeSentimentRequest>
{
    public AnalyzeSentimentRequestValidator()
    {
        RuleFor(x => x.Text).NotEmpty().MaximumLength(5000);
    }
}
