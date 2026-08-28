using FluentValidation;
using ShilpoHubBD.Application.DTOs.BusinessPartnerAnalytics;

namespace ShilpoHubBD.Application.Validators.BusinessPartnerAnalytics;

public class AnalyticsQueryParametersValidator : AbstractValidator<AnalyticsQueryParameters>
{
    public AnalyticsQueryParametersValidator()
    {
        RuleFor(x => x.Industry).MaximumLength(150);
        RuleFor(x => x)
            .Must(x => !x.DateFrom.HasValue || !x.DateTo.HasValue || x.DateFrom <= x.DateTo)
            .WithMessage("DateFrom must be on or before DateTo.")
            .WithName("DateFrom");
    }
}
